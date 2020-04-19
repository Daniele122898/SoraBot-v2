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
using SoraBot.Common.Utils;
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

        [Command("userinfo"), Alias("whois", "uinfo")]
        [Summary("Gives infos about the @mentioned user. If none is mentioned it will show infos about you")]
        public async Task UserInfo(
            [Summary("@User to get info about. Mention no one to get info about yourself")]
            DiscordGuildUser userT = null)
        {
            var user = userT?.GuildUser ?? (IGuildUser) Context.User;
            var coins = _coinRepo.GetCoins(user.Id);
            var waifu = await _waifuRepo.GetFavWaifuOfUser(user.Id).ConfigureAwait(false);
            var footer = RequestedByMe();
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Title = $"{InfoEmoji} {Formatter.UsernameDiscrim(user)}",
                Footer = footer.WithText(footer.Text + $" | ID: {user.Id.ToString()}"),
            };
            eb.AddField(x =>
            {
                x.Name = "Joined Discord";
                x.IsInline = false;
                x.Value = $"On {user.CreatedAt.ToString("dd/MM/yyyy")}. " +
                          $"That is {((int) DateTime.Now.Subtract(user.CreatedAt.DateTime).TotalDays).ToString()} days ago!";
            });
            eb.AddField(x =>
            {
                string joined = "_Unknown_";
                if (user.JoinedAt != null)
                {
                    joined = $"On {user.JoinedAt.Value.DateTime.ToString("dd/MM/yyyy")}. " +
                             $"That is {((int) DateTime.Now.Subtract(user.JoinedAt.Value.DateTime).TotalDays).ToString()} days ago!";
                }

                x.Name = "Joined Server";
                x.IsInline = false;
                x.Value = joined;
            });
            eb.AddField(x =>
            {
                x.Name = "Nickname";
                x.IsInline = true;
                x.Value = string.IsNullOrWhiteSpace(user.Nickname) ? "_none_" : user.Nickname;
            });
            eb.AddField(x =>
            {
                x.Name = "Avatar";
                x.IsInline = true;
                x.Value = $"[Click Here]({(user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl())})";
            });
            eb.AddField(x =>
            {
                x.Name = "Sora Coins";
                x.IsInline = true;
                x.Value = $"{coins.ToString()} SC";
            });
            eb.AddField(x =>
            {
                string roles = String.Join(", ",
                    Context.Guild.Roles
                        .Where(x => user.RoleIds.Any(y => y == x.Id) && !x.IsEveryone)
                        .Select(r => r.Name));
                x.Name = "Roles";
                x.IsInline = true;
                x.Value = string.IsNullOrWhiteSpace(roles) ? "_none_" : roles;
            });
            if (waifu.HasValue)
            {
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Favorite Waifu";
                    x.Value = waifu.Value.Name;
                });
                eb.ImageUrl = waifu.Value.ImageUrl;
            }

            await ReplyEmbed(eb);
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