using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class ExpService
    {
        private DiscordSocketClient _client;
        private GuildLevelRoleService _roleService;

        public ExpService(DiscordSocketClient client, GuildLevelRoleService roleService)
        {
            _client = client;
            _roleService = roleService;
        }
        
        public static int CalculateLevel(float exp)
        {
            return (int)Math.Floor(0.15F * Math.Sqrt(exp));
        }

        public static int CalculateNeededExp(int lvl)
        {
            return (int)Math.Pow((lvl/0.15F), 2.0);
        }
        
        public async Task ToggleEpGain(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                userDb.Notified = !userDb.Notified;
                await soraContext.SaveChangesAsync();
                if (userDb.Notified)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "You will now be notified on level up!").Build());
                    return;
                }
            }

            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "You will NOT be notified on level up!").Build());
        }
        
        private int GetIndexOfItem(List<User> list, ulong key)
        {
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index].UserId == key)
                    return index;
            }
            return -1;
        }
        
        private int GetIndexOfItem(List<GuildUser> list, ulong key)
        {
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index].UserId == key)
                    return index;
            }
            return -1;
        }

        public async Task GetLocalTop10List(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync($"Check out **{context.Guild.Name}'s leaderboard** here: http://sorabot.pw/guild/{context.Guild.Id}/leaderboard ｡◕ ‿ ◕｡");
        }

        public async Task GetGlobalTop10(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync($"Check out the **Global Leaderboard** here: http://sorabot.pw/globalleader °˖✧◝(⁰▿⁰)◜✧˖°");
        }


        public async Task IncreaseEpOnMessageReceive(SocketMessage msg)
        {
            using (var _soraContext = new SoraContext())
            {
                //Don't prcoess the command if it was a system message
                var message = msg as SocketUserMessage;
                if (message == null) return;
                //dont process if it was a bot
                if (message.Author.IsBot) return;
                
                //Create a command Context
                var context = new SocketCommandContext(_client, message);
                if (context.IsPrivate) return;
                
                var userDb = Utility.GetOrCreateUser(context.User.Id, _soraContext);
                //Check for cooldown
                if (userDb.CanGainAgain.CompareTo(DateTime.UtcNow) > 0)
                    return;
                //Reset cooldown
                userDb.CanGainAgain = DateTime.UtcNow.AddSeconds(10);
                int previousLevel = CalculateLevel(userDb.Exp);
                var epGain = (int)Math.Round(context.Message.Content.Length / 10F);
                if (epGain > 50)
                    epGain = 50;
                userDb.Exp += epGain;
                int currentLevel = CalculateLevel(userDb.Exp);
                await _soraContext.SaveChangesAsync();
                //Guild user gain
                Task.Run(async () =>
                {
                    await _roleService.OnUserExpGain(epGain, context);
                });
                
                //Notifying
                if (previousLevel != currentLevel && userDb.Notified)
                {
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(255, 204, 77),
                        Title = $"🏆 You leveled up! You are now level {currentLevel} \\ (•◡•) /"
                    };
                    await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb.Build());
                }
            }
        }
    }
}