using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class MarriageService
    {
        private InteractiveService _interactive;
        private SoraContext _soraContext;
        
        private const int MARRIAGE_SCALE = 10;

        public MarriageService(InteractiveService interactiveService, SoraContext soraContext)
        {
            _interactive = interactiveService;
            _soraContext = soraContext;
        }

        public async Task CheckLimit(SocketCommandContext context, SocketUser user)
        {
            var userDb = Utility.OnlyGetUser(user, _soraContext);
            if (userDb == null)
            {
                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], $"💍 {Utility.GiveUsernameDiscrimComb(user)} has a limit of 1. Married to 0 users"));
                return;
            }
            int marryLimit =((int)(Math.Floor((double) (EpService.CalculateLevel(userDb.Exp) / 10)))) +1;
            await context.Channel.SendMessageAsync("", embed:
                Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], $"💍 {Utility.GiveUsernameDiscrimComb(user)} has a limit of {marryLimit}. Married to {userDb.Marriages.Count} users"));
        }

        public async Task Marry(SocketCommandContext context, SocketUser user)
        {
            var requestorDb = Utility.GetOrCreateUser(context.User, _soraContext);
            var askedDb = Utility.GetOrCreateUser(user, _soraContext);
            int allowedMarriagesRequestor = ((int)(Math.Floor((double) (EpService.CalculateLevel(requestorDb.Exp) / 10)))) +1;
            int allowedMarriagesAsked = ((int)(Math.Floor((double) (EpService.CalculateLevel(askedDb.Exp) / 10)))) +1;
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
                        $"{Utility.GiveUsernameDiscrimComb(user)} already reached their marriage limit. They must level up to increase the limit"));//TODO this sounds like shit. change it
                return;
            }
            //Check for duplicate
            if (requestorDb.Marriages.Any(x => x.PartnerId == user.Id) || askedDb.Marriages.Any(x=> x.PartnerId == context.User.Id))
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
            var response = await _interactive.WaitForMessage(user, context.Channel, TimeSpan.FromSeconds(45));
            if (response == null)
            {
                await context.Channel.SendMessageAsync("", embed: 
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], 
                        $"{Utility.GiveUsernameDiscrimComb(user)} didn't answer in time >.<"));
                return;
            }
            if ((!response.Content.Contains(" yes ", StringComparison.OrdinalIgnoreCase) && !response.Content.Contains(" yes,", StringComparison.OrdinalIgnoreCase)
            && !response.Content.Contains("yes ", StringComparison.OrdinalIgnoreCase) && !response.Content.Contains("yes,", StringComparison.OrdinalIgnoreCase)) && !response.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
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
            askedDb.Marriages.Add(new Marriage()
            {
                PartnerId = context.User.Id,
                Since = DateTime.UtcNow
            });
            _soraContext.SaveChangesThreadSafe();
            await context.Channel.SendMessageAsync("", embed: 
                Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], 
                    $"You are now married 💑").WithImageUrl("https://media.giphy.com/media/iQ5rGja9wWB9K/giphy.gif"));
        }
        
    }
}