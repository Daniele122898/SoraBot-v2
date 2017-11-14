using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class AfkService
    {
        private const int SECONDS_AFTER_REPOST = 30;

        private async Task AddAfk(SocketCommandContext context, User userDb ,string message, bool updated)
        {
            if (message == null)
                message = "";
            message = message.Length < 80 ? message : message.Substring(0, 80) + "...";
            DateTime timeToTriggerAgain = DateTime.UtcNow;

            userDb.Afk.IsAfk = true;
            userDb.Afk.Message = message;
            userDb.Afk.TimeToTriggerAgain = timeToTriggerAgain;

            var eb = new EmbedBuilder()
            {
                Color = Utility.GreenSuccessEmbed,
                Title = $"{Utility.SuccessLevelEmoji[0]} {(updated ? "Your AFK status has been updated!" : "You are now set AFK")}"
            };
            await context.Channel.SendMessageAsync("", embed: eb);
        }

        public async Task ToggleAFK(SocketCommandContext context, string message, SoraContext soraContext)
        {
            var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
            if (userDb.Afk == null)
            {
                userDb.Afk = new Afk()
                {
                    IsAfk = false,
                    UserForeignId = context.User.Id
                };
            }
            if (!userDb.Afk.IsAfk)
            {
                //ADD AFK
                await AddAfk(context, userDb, message, false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    //REMOVE
                    userDb.Afk.IsAfk = false;
                    userDb.Afk.Message = "";

                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                            "AFK has been removed"));

                }
                else
                {
                    //UPDATE
                    await AddAfk(context, userDb, message, true);
                }
            }
            await soraContext.SaveChangesAsync();
        }

        public async Task Client_MessageReceived(SocketMessage msg, IServiceProvider _services)
        {
            if (msg.Author.Id == 270931284489011202 || msg.Author.Id == 276304865934704642)
                return;
            if (msg.MentionedUsers.Count < 1)
                return;

            using (var soraContext = _services.GetService<SoraContext>())
            {
                //Get GUILD PREFIX
                string prefix = Utility.GetGuildPrefixFast(soraContext, ((SocketGuildChannel)msg.Channel).Guild.Id, "$");
                if (msg.Content.StartsWith(prefix))
                    return;

                foreach (var user in msg.MentionedUsers)
                {
                    var userDb = Utility.GetOrCreateUser(user.Id, soraContext);
                    if(userDb.Afk == null)
                        return; //if null none was set up
                    if (userDb.Afk.IsAfk)
                    {
                        //CAN TRIGGER AGAIN?
                        if(userDb.Afk.TimeToTriggerAgain.CompareTo(DateTime.UtcNow)>0)
                            return;

                        userDb.Afk.TimeToTriggerAgain = DateTime.UtcNow.AddSeconds(SECONDS_AFTER_REPOST);

                        var eb = new EmbedBuilder()
                        {
                            Color = Utility.PurpleEmbed,
                            Author = new EmbedAuthorBuilder()
                            {
                                IconUrl = user.GetAvatarUrl()?? Utility.StandardDiscordAvatar,
                                Name = $"{user.Username} is currently AFK"
                            },
                            Description = userDb.Afk.Message
                        };

                        await soraContext.SaveChangesAsync();
                        await msg.Channel.SendMessageAsync("", embed: eb);
                    }
                }
            }
        }
    }
}