using System;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private readonly BanService _banService;
        private readonly OwnerService _ownerService;

        public OwnerModule(BanService banService, OwnerService ownerService)
        {
            _banService = banService;
            _ownerService = ownerService;
        }

        [Command("leavebotservers", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task LeaveBotServers()
        {
            await _ownerService.CollectBotServerInfoAndLeaveAfter(Context);
        }

        [Command("obanUser")]
        [RequireOwner]
        public async Task BanUser(ulong id, [Remainder] string reason)
        {
            var succ = await _banService.BanUser(id, reason);
            if (succ)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "User has been globally banned from using Sora.").Build());
                return;
            }

            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Couldn't Ban user. Either he's already banned or smth broke.").Build());
        }

        [Command("ounbanUser")]
        [RequireOwner]
        public async Task UnBanUser(ulong id)
        {
            var succ = await _banService.UnBanUser(id);
            if (succ)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "User has been globally unbanned from using Sora.").Build());
                return;
            }

            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Couldn't unban user. Either he's already banned or smth broke.").Build());
        }

        [Command("obaninfo")]
        [RequireOwner]
        public async Task BanUser(ulong id)
        {
            await _banService.GetBanInfo(Context, id);
        }

        [Command("gc")]
        [RequireOwner]
        public Task ForceGc()
        {
            GC.Collect();
            return Task.CompletedTask;
        }

        [Command("reloadconfig"), Alias("reconf")]
        [RequireOwner]
        public async Task ReloadConfig()
        {
            ConfigService.LoadConfig();
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "Successfully reloaded config.json").Build());
        }

        [Command("oginfo")]
        [RequireOwner]
        public async Task Guildinfo(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            if (guild == null)
            {
                await ReplyAsync("Guild not found");
                return;
            }

            await ReplyAsync($"```\n" +
                             $"Guild Name: {guild.Name}\n" +
                             $"User Count: {guild.Users.Count}\n" +
                             $"Owner: {Utility.GiveUsernameDiscrimComb(guild.Owner)}\n" +
                             $"```");
        }

        [Command("leaveguild")]
        [RequireOwner]
        public async Task LeaveGuild(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            if (guild == null)
            {
                await ReplyAsync("Guild not found");
                return;
            }

            await guild.LeaveAsync();
            await ReplyAsync("Left guild.");
        }
    }
}