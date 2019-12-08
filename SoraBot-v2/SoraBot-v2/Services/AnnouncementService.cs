using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class AnnouncementService
    {


        private readonly string _defaultLeave = "{user#} left the guild";
        private readonly string _defaultJoin = "{user} Welcome to **{server}**";

        public async Task ClientOnUserLeft(SocketGuildUser socketGuildUser)
        {
            Task.Run(async () =>
            {
                using (SoraContext soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(socketGuildUser.Guild.Id, soraContext);
                    //check if channel is initialized
                    if (guildDb.LeaveChannelId == 0)
                        return;
                    //check if channel exists
                    var channel = socketGuildUser.Guild.GetTextChannel(guildDb.LeaveChannelId);
                    if (channel == null)
                    {
                        guildDb.LeaveChannelId = 0;
                        await soraContext.SaveChangesAsync();
                        return;
                    }

                    //check sendmessageperms perms
                    if (await Utility.CheckReadWritePerms(socketGuildUser.Guild, channel) == false)
                        return;
                    //he has perms and channel exists so post the message
                    string message = guildDb.LeaveMessage;
                    if (string.IsNullOrWhiteSpace(message))
                        message = _defaultLeave;
                    string editedMessage = ReplaceInfo(socketGuildUser, message);

                    if (guildDb.EmbedLeave)
                    {
                        var eb = new EmbedBuilder()
                        {
                            Color = Utility.PurpleEmbed,
                            Description = editedMessage
                        };

                        await channel.SendMessageAsync("", embed: eb.Build());
                    }
                    else
                    {
                        await channel.SendMessageAsync(editedMessage);
                    }
                }
            });
        }


        public async Task ClientOnUserJoined(SocketGuildUser socketGuildUser)
        {
            Task.Run(async () =>
            {
                using (SoraContext soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(socketGuildUser.Guild.Id, soraContext);
                    //check if channel is initialized
                    if (guildDb.WelcomeChannelId == 0)
                        return;
                    //check if channel exists
                    var channel = socketGuildUser.Guild.GetTextChannel(guildDb.WelcomeChannelId);
                    if (channel == null)
                    {
                        guildDb.WelcomeChannelId = 0;
                        await soraContext.SaveChangesAsync();
                        return;
                    }

                    //check sendmessageperms perms
                    if (await Utility.CheckReadWritePerms(socketGuildUser.Guild, channel) == false)
                        return;
                    //he has perms and channel exists so post the message
                    string message = guildDb.WelcomeMessage;
                    if (string.IsNullOrWhiteSpace(message))
                        message = _defaultJoin;
                    string editedMessage = ReplaceInfo(socketGuildUser, message);

                    if (guildDb.EmbedWelcome)
                    {
                        var eb = new EmbedBuilder()
                        {
                            Color = Utility.PurpleEmbed,
                            Description = editedMessage
                        };

                        await channel.SendMessageAsync("", embed: eb.Build());
                    }
                    else
                    {
                        await channel.SendMessageAsync(editedMessage);
                    }
                }
            });
        }

        private string ReplaceInfo(SocketGuildUser user, string message)
        {
            string edited = message.Replace("{user}", $"{user.Mention}");
            edited = edited.Replace("{user#}", $"{Utility.GiveUsernameDiscrimComb(user)}");
            edited = edited.Replace("{server}", $"{user.Guild.Name}");
            edited = edited.Replace("{count}", $"{user.Guild.MemberCount}");
            return edited;
        }

        public async Task ToggleWelcomeEmbed(SocketCommandContext context)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.EmbedWelcome = !guildDb.EmbedWelcome;
                await soraContext.SaveChangesAsync();
                if (guildDb.EmbedWelcome)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Welcome announcements will now be done in embeds!").Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Welcome announcements will now be done as normal messages!").Build());
                }
            }
        }

        public async Task ToggleLeaveEmbed(SocketCommandContext context)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.EmbedLeave = !guildDb.EmbedLeave;
                await soraContext.SaveChangesAsync();
                if (guildDb.EmbedLeave)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Leave announcements will now be done in embeds!").Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Leave announcements will now be done as normal messages!").Build());
                }
            }
        }

        public async Task<bool> SetWelcomeMessage(SocketCommandContext context, string message)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return false;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.WelcomeMessage = message;
                await soraContext.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> SetLeaveMessage(SocketCommandContext context, string message)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return false;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.LeaveMessage = message;
                await soraContext.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> SetWelcomeChannel(SocketCommandContext context, SocketChannel channel)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return false;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.WelcomeChannelId = channel.Id;
                await soraContext.SaveChangesAsync();
            }
            return true;
        }

        public async Task RemoveWelcomeChannel(SocketCommandContext context)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.WelcomeChannelId = 0;
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], "Successfully removed Welcome channel. No join announcements will be done anymore").Build());
        }

        public async Task RemoveLeaveChannel(SocketCommandContext context)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.LeaveChannelId = 0;
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], "Successfully removed Leave channel. No leave announcements will be done anymore").Build());
        }

        public async Task<bool> SetLeaveChannel(SocketCommandContext context, SocketChannel channel)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return false;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.LeaveChannelId = channel.Id;
                await soraContext.SaveChangesAsync();
            }
            return true;
        }
    }
}