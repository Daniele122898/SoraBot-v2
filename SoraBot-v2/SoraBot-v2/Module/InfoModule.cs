using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private CommandHandler _commandHandler;
        private SoraContext _soraContext;

        public InfoModule(CommandHandler commandHandler, SoraContext soraContext)
        {
            _commandHandler = commandHandler;
            _soraContext = soraContext;
        }

        [Command("userinfo"), Alias("whois", "uinfo"),
         Summary("Gives infos about the selected user. If none is mentioned the invoker will be taken")]
        public async Task UserInfo([Summary("User to display info of")] SocketUser userT = null)
        {
            SocketGuildUser user = (SocketGuildUser)(userT ?? Context.User);
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                ThumbnailUrl = user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                Title = $"{Utility.SuccessLevelEmoji[3]} {user.Username}",
                Description = $"Joined Discord on {user.CreatedAt.ToString().Remove(user.CreatedAt.ToString().Length - 6)}. That is {(int)(DateTime.Now.Subtract(user.CreatedAt.DateTime).TotalDays)} days ago!",
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Text = $"Requested by {Utility.GiveUsernameDiscrimComb(Context.User)} | User ID: {user.Id}"
                }
            };
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Status";
                x.Value = user.Status.Humanize().Transform(To.LowerCase, To.TitleCase);
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Game";
                x.Value =$"{(user.Game.HasValue ? user.Game.Value.Name : "*none*")}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Nickname";
                x.Value =$"{(user.Nickname == null ? "*none*" : $"{user.Nickname}")}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Discriminator";
                x.Value =$"#{user.Discriminator}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Joined Guild";
                if (user?.JoinedAt != null)
                    x.Value =
                        $"{user?.JoinedAt.ToString().Remove(user.JoinedAt.ToString().Length - 6)}\n*({(int) DateTime.Now.Subtract(((DateTimeOffset) user?.JoinedAt).DateTime).TotalDays} days ago)*";
            });
            var dbUser = Utility.OnlyGetUser(user.Id, _soraContext);
            string icon="";
            double aff = 0;
            if (dbUser != null)
            {
                aff = Utility.CalculateAffinity(dbUser.Interactions);
                icon = InteractionsService.MySwitch.First(sw => sw.Key((int) Math.Round(aff))).Value;
            }
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Affinity";
                x.Value =$"{(dbUser == null ? "-" : $"{aff}/100 {icon}")}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Roles";

                string roles = "";
                foreach (var role in user.Roles)
                {
                    if (role.Name != "@everyone")
                        roles += $"{role.Name}, ";
                }
                
                x.Value =$"{(string.IsNullOrWhiteSpace(roles)? "*none*": $"{roles.Remove(roles.Length-2)}")}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Avatar";
                x.Value =$"[Click to View]({user.GetAvatarUrl().Replace("?size=128" , "?size=1024") ?? Utility.StandardDiscordAvatar})";
            });
            

            if (dbUser != null && dbUser.Marriages.Count > 0)
            {
                string marriages = "";
                foreach (var marriage in dbUser.Marriages)
                {
                    SocketUser partner = Context.Client.GetUser(marriage.PartnerId);
                    if(partner == null)
                        continue;
                    marriages += $"{Utility.GiveUsernameDiscrimComb(partner)}, ";
                }
                if (!string.IsNullOrWhiteSpace(marriages))
                {
                    eb.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"Marriages";
                        x.Value = marriages.Remove(marriages.Length-2);
                    });
                }
            }

            await ReplyAsync("", embed: eb);
        }
        
        [Command("serverinfo"), Alias("sinfo", "ginfo", "guildinfo"), Summary("Gives infos about the guild")]
        public async Task GuildInfo()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Footer = Utility.RequestedBy(Context.User).WithText($"Requested by {Utility.GiveUsernameDiscrimComb(Context.User)} | Guild ID: {Context.Guild.Id}"),
                Title = $"{Utility.SuccessLevelEmoji[3]} **{Context.Guild.Name}**",
                ThumbnailUrl = Context.Guild.IconUrl ?? Utility.StandardDiscordAvatar,
                Description =$"Created on {Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}. That's {(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)} days ago!"
            };
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Owner";
                x.Value = $"{Utility.GiveUsernameDiscrimComb(Context.Guild.Owner)}";
            });
            int online= Context.Guild.Users.Count(socketGuildUser=> socketGuildUser.Status != UserStatus.Invisible && socketGuildUser.Status != UserStatus.Offline);
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Members";
                x.Value = $"{online} / {Context.Guild.MemberCount}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Region";
                x.Value = $"{(Context.Guild.VoiceRegionId).Humanize().Transform(To.LowerCase, To.TitleCase)}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Roles";
                x.Value = $"{Context.Guild.Roles.Count}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Channels [{Context.Guild.Channels.Count}]";
                x.Value = $"{Context.Guild.TextChannels.Count} Text | {Context.Guild.VoiceChannels.Count} Voice";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "AFK Channel";
                x.Value = $"{(Context.Guild.AFKChannel == null ? $"No AFK Channel" : $"{Context.Guild.AFKChannel.Name}\n*in {(int)(Context.Guild.AFKTimeout / 60)} Min*")}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Total Emotes";//TODO CHANGE FOR SHARD LATER
                x.Value = $"{Context.Guild.Emotes.Count}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Avatar URL";
                x.Value = $"[Click to view]({Context.Guild.IconUrl+"?size=1024" ?? Utility.StandardDiscordAvatar})";
            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Emotes";

                string val = "";
                foreach (var emote in Context.Guild.Emotes)
                {
                    if (val.Length < 950)
                        val += $"<:{emote.Name}:{emote.Id}> ";
                }
                if (string.IsNullOrWhiteSpace(val))
                    val = "No Custom Emotes";
                x.Value = val;
            });

            await ReplyAsync("", embed: eb);

        }

        [Command("support"), Summary("link to support server")]
        public async Task Support()
        {
            await Context.Channel.SendMessageAsync($"**Get support here**\n{Utility.DISCORD_INVITE}");
        }
        
        [Command("sys"), Alias("info"), Summary("Gives stats about Sora")]
        public async Task GetSysInfo()
        {
            var proc = System.Diagnostics.Process.GetCurrentProcess();

            Func<long, long> formatRamValue = d =>
            {
                while (d>1024)
                {
                    d /= 1024;
                }
                return d;
            };

            Func<long, string> formatRamUnit = d =>
            {
                var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB"};
                var unitCount = 0;
                while (d>1024)
                {
                    d /= 1024;
                    unitCount++;
                }
                return units[unitCount];
            };

            double VSZ = 0;
            double RSS = 0;
            if (File.Exists($"/proc/{proc.Id}/statm"))
            {
                var ramUsageInitial = File.ReadAllText($"/proc/{proc.Id}/statm");
                var ramUsage = ramUsageInitial.Split(' ')[0];
                VSZ = double.Parse(ramUsage);
                VSZ = VSZ * 4069 / 1048576;
                ramUsage = ramUsageInitial.Split(' ')[1];
                RSS = double.Parse(ramUsage);
                RSS = RSS*4069 / 1048576;
            }

            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = Utility.RequestedBy(Context.User),
                Title = $"{Utility.SuccessLevelEmoji[3]} **Sora Sys Info**",
                Url = "http://git.argus.moe/serenity/SoraBot-v2"
            };
            eb.AddField((x) =>
            {
                x.Name = "Uptime";
                x.IsInline = true;
                x.Value = (DateTime.Now - proc.StartTime).ToString(@"d'd 'hh\:mm\:ss");
            });
            eb.AddField((x) =>
            {
                x.Name = "Used RAM";
                x.IsInline = true;
                var mem = GC.GetTotalMemory(false);
                x.Value = $"{formatRamValue(mem):f2} {formatRamUnit(mem)} / {formatRamValue(proc.WorkingSet64):f2} {formatRamUnit(proc.WorkingSet64)}";
                //$"{(proc.PagedMemorySize64 == 0 ? $"{RSS:f1} MB / {VSZ:f1} MB" : $"{formatRamValue(proc.PagedMemorySize64):f2} {formatRamUnit(proc.PagedMemorySize64)} / {formatRamValue(proc.WorkingSet64):f2} {formatRamUnit(proc.WorkingSet64)}")}\n" +
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Messages Received";
                x.Value = $"{_commandHandler.MessagesReceived} since restart";
            });
            eb.AddField((x) =>
            {
                x.Name = "Commands Executed";
                x.IsInline = true;
                x.Value = $"{_commandHandler.CommandsExecuted+1} since restart";
            });
            eb.AddField((x) =>
            {
                x.Name = "Connected Guilds";
                x.IsInline = true;
                x.Value = $"{Context.Client.Guilds.Count}";
            });
            var channelCount = 0;
            var userCount = 0;
            foreach (var g in Context.Client.Guilds)
            {
                channelCount += g.Channels.Count;
                userCount += g.MemberCount;
            }
            eb.AddField((x) =>
            {
                x.Name = "Watching Channels";
                x.IsInline = true;
                x.Value = $"{channelCount}";
            });
            eb.AddField((x) =>
            {
                x.Name = "Users with access";
                x.IsInline = true;
                x.Value = $"{userCount}";
            });
            eb.AddField((x) =>
            {
                x.Name = "Music";
                x.IsInline = true;
                x.Value = $"Use `{Utility.GetGuildPrefix(Context.Guild, _soraContext)}msys` for music stats"; //TODO COUNT MUSIC STREAMS
            });
            eb.AddField((x) =>
            {
                x.Name = "Ping";
                x.IsInline = true;
                x.Value = $"{Context.Client.Latency} ms";
            });
            eb.AddField((x) =>
            {
                x.Name = "Sora Version";
                x.IsInline = true;
                x.Value = $"{Utility.SORA_VERSION}";
            });
            eb.AddField((x) =>
            {
                x.Name = "Sora's Official Guild";
                x.IsInline = true;
                x.Value = $"[Join here]({Utility.DISCORD_INVITE})";
            });
            eb.AddField((x) =>
            {
                x.Name = "Invite me";
                x.IsInline = true;
                x.Value = $"[Click here to invite]({Utility.SORA_INVITE})";
            });
            await ReplyAsync("", embed: eb);
        }
    }
}