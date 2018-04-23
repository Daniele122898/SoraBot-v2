using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class MarriageService
    {
        private readonly InteractiveService _interactive;

        private const int MARRIAGE_SCALE = 10;

        public MarriageService(InteractiveService interactiveService)
        {
            _interactive = interactiveService;
        }


        public async Task CheckLimit(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (userDb == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                            $"💍 {Utility.GiveUsernameDiscrimComb(user)} has a limit of 1. Married to 0 users"));
                    return;
                }
                int marryLimit = ((int)(Math.Floor((double)(ExpService.CalculateLevel(userDb.Exp) / 10)))) + 1;

                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                        $"💍 {Utility.GiveUsernameDiscrimComb(user)} has a limit of {marryLimit}. Married to {userDb.Marriages.Count} users"));
            }
        }

        public async Task ShowMarriages(SocketCommandContext context, SocketUser user)
        {
            using (var _soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(user.Id, _soraContext);
                if (userDb == null || userDb.Marriages.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        $"{Utility.GiveUsernameDiscrimComb(user)} has no marriages yet!"));
                    return;
                }

                var eb = new EmbedBuilder()
                {
                    Color = Utility.PurpleEmbed,
                    Title = $"💕 Marriages of {Utility.GiveUsernameDiscrimComb(user)}",
                    ThumbnailUrl = user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Footer = Utility.RequestedBy(context.User)
                };
                foreach (var marriage in userDb.Marriages)
                {
                    var partner = context.Client.GetUser(marriage.PartnerId);
                    eb.AddField(x =>
                    {
                        x.Name =
                            $"{(partner == null ? $"Unknown({marriage.Id})" : $"{Utility.GiveUsernameDiscrimComb(partner)}")}";
                        x.IsInline = true;
                        x.Value = $"*Since {marriage.Since.ToString("dd/MM/yyyy")}*";
                    });
                }

                await context.Channel.SendMessageAsync("", embed: eb);
            }

        }

        public async Task Divorce(SocketCommandContext context, ulong Id)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null || userDb.Marriages.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have no marriages yet!"));
                    return;
                }
                var result = userDb.Marriages.FirstOrDefault(x => x.PartnerId == Id);
                if (result == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not married to that person"));
                    return;
                }
                var parterDb = Utility.OnlyGetUser(Id, soraContext);
                userDb.Marriages.Remove(result);
                var remove = parterDb.Marriages.FirstOrDefault(x => x.PartnerId == context.User.Id);
                if (remove != null)
                    parterDb.Marriages.Remove(remove);

                await soraContext.SaveChangesAsync();

                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "You have been successfully divorced"));

                var divorced = context.Client.GetUser(Id);
                if (divorced != null)
                {
                    await (await divorced.GetOrCreateDMChannelAsync()).SendMessageAsync("",
                        embed: Utility.ResultFeedback(
                            Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3],
                            $"{Utility.GiveUsernameDiscrimComb(context.User)} has divorced you 😞"));
                }
            }
        }

        public async Task Marry(SocketCommandContext context, SocketUser user)
        {
            //Check if its urself
            if (user.Id == context.User.Id)
            {
                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        $"You can't and shouldn't marry yourself ;_;"));
                return;
            }
            using (var soraContext = new SoraContext())
            {
                var requestorDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                var askedDb = Utility.GetOrCreateUser(user.Id, soraContext);
                int allowedMarriagesRequestor =
                    ((int)(Math.Floor((double)(ExpService.CalculateLevel(requestorDb.Exp) / 10)))) + 1;
                int allowedMarriagesAsked =
                    ((int)(Math.Floor((double)(ExpService.CalculateLevel(askedDb.Exp) / 10)))) + 1;
                //check both limits
                if (requestorDb.Marriages.Count >= allowedMarriagesRequestor)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(context.User)}, you already reached your marriage limit. Level up to increase it"));
                    return;
                }
                if (askedDb.Marriages.Count >= allowedMarriagesAsked)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} already reached their marriage limit. They must level up to increase the limit")); //TODO this sounds like shit. change it
                    return;
                }
                //Check for duplicate
                if (requestorDb.Marriages.Any(x => x.PartnerId == user.Id) ||
                    askedDb.Marriages.Any(x => x.PartnerId == context.User.Id))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You cannot marry someone twice!"));
                    return;
                }
                //Proceed to ask for marriage
                var msg = await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                        $"{Utility.GiveUsernameDiscrimComb(user)}, do you want to marry {Utility.GiveUsernameDiscrimComb(context.User)}? 💍"));

                Criteria<SocketMessage> criteria = new Criteria<SocketMessage>();
                criteria.AddCriterion(new EnsureFromUserInChannel(user.Id, context.Channel.Id));

                var response = await _interactive.NextMessageAsync(context, criteria, TimeSpan.FromSeconds(45));
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} didn't answer in time >.<"));
                    return;
                }
                if ((!response.Content.Contains(" yes ", StringComparison.OrdinalIgnoreCase) &&
                     !response.Content.Contains(" yes,", StringComparison.OrdinalIgnoreCase)
                     && !response.Content.Contains("yes ", StringComparison.OrdinalIgnoreCase) &&
                     !response.Content.Contains("yes,", StringComparison.OrdinalIgnoreCase)) &&
                    !response.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} didn't answer with a yes ˚‧º·(˚ ˃̣̣̥᷄⌓˂̣̣̥᷅ )‧º·˚"));
                    return;
                }
                //Answer contains a yes
                requestorDb.Marriages.Add(new Marriage()
                {
                    PartnerId = user.Id,
                    Since = DateTime.UtcNow
                });
                //_soraContext.SaveChangesThreadSafe();
                askedDb.Marriages.Add(new Marriage()
                {
                    PartnerId = context.User.Id,
                    Since = DateTime.UtcNow
                });
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed:
                Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                    $"You are now married 💑").WithImageUrl("https://media.giphy.com/media/iQ5rGja9wWB9K/giphy.gif"));
        }

    }
}