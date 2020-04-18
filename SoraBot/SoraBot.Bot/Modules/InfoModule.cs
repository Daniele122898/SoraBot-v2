using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using SoraBot.Bot.Models;
using SoraBot.Bot.TypeReaders;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Configurations;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    [Name("Info")]
    [Summary("Commands for general information about users or Sora")]
    public class InfoModule : SoraSocketCommandModule
    {
        private readonly ICoinRepository _coinRepo;
        private readonly IWaifuRepository _waifuRepo;
        private readonly SoraBotConfig _config;

        public InfoModule(ICoinRepository coinRepo, IWaifuRepository waifuRepo, IOptions<SoraBotConfig> config)
        {
            _coinRepo = coinRepo;
            _waifuRepo = waifuRepo;
            _config = config.Value;
        }

        [Command("sys")]
        [Summary("Get stats about Sora")]
        public async Task GetSysInfo()
        {
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = RequestedByMe(),
                Title = "Sora System Information",
                Url = "https://github.com/Daniele122898/SoraBot-v2",
                Description = "These are statistics on the current Shard"
            };
            eb.AddField(x =>
            {
                using var proc = Process.GetCurrentProcess();
                x.Name = "Uptime";
                x.IsInline = true;
                x.Value = (DateTime.Now - proc.StartTime).Humanize(); //.ToString(@"d'd 'hh\:mm\:ss")
            });
            eb.AddField(x =>
            {
                x.Name = "Used RAM";
                x.IsInline = true;
                var mem = GC.GetTotalMemory(false);
                x.Value = $"{(mem.Bytes().Humanize("MB"))}";
            });
            eb.AddField(x =>
            {
                x.Name = "Connected Guilds";
                x.IsInline = true;
                x.Value = Context.Client.Guilds.Count.ToString();
            });
            var userCount = Context.Client.Guilds.Sum(x => x.MemberCount);
            eb.AddField(x =>
            {
                x.Name = "Total Users";
                x.IsInline = true;
                x.Value = userCount.ToString();
            });
            eb.AddField(x =>
            {
                x.Name = "Ping";
                x.IsInline = true;
                x.Value = $"{Context.Client.Latency.ToString()} ms";
            });
            eb.AddField((x) =>
            {
                x.Name = "Sora's Official Guild";
                x.IsInline = true;
                x.Value = $"[Join here]({_config.DiscordSupportInvite})";
            });
            eb.AddField((x) =>
            {
                x.Name = "Invite me";
                x.IsInline = true;
                x.Value = $"[Click here to invite]({_config.SoraBotInvite})";
            });
            eb.AddField((x) =>
            {
                x.Name = "Support me";
                x.IsInline = true;
                x.Value = $"[Support me on Patreon](https://www.patreon.com/Serenity_c7)";
            });
            await ReplyEmbed(eb);
        }
        
        [Command("avatar")]
        [Summary("Get the avatar of the @user or yourself if no one is tagged")]
        public async Task GetAvatar(
            [Summary("@User to get the avatar from, or no one to get your own")]
            [OverrideTypeReader(typeof(GuildUserTypeReader))]
            DiscordGuildUser userT = null)
        {
            var user = userT?.GuildUser ?? (IGuildUser)Context.User;
            var eb = new EmbedBuilder()
            {
                Footer  = RequestedByFooter(Context.User),
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 512) ?? user.GetDefaultAvatarUrl(),
                Color = Purple
            };
            await ReplyEmbed(eb);
        }
    }
}