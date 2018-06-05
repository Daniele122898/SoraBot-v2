using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public static class Utility
    {
        public const ulong OWNER_ID = 192750776005689344;
        
        public static Discord.Color PurpleEmbed = new Discord.Color(109, 41, 103);
        public static Discord.Color YellowWarningEmbed = new Discord.Color(255,204,77);
        public static Discord.Color GreenSuccessEmbed = new Discord.Color(119,178,85);
        public static Discord.Color RedFailiureEmbed = new Discord.Color(221,46,68);
        public static Discord.Color BlueInfoEmbed = new Discord.Color(59,136,195);
        public static string StandardDiscordAvatar = "http://i.imgur.com/tcpgezi.jpg";
        public static int TOTAL_SHARDS;
        public static string SORA_VERSION = ConfigService.GetConfigData("version");

        public const string DISCORD_INVITE = "https://discordapp.com/invite/Pah4yj5";

        public const string SORA_INVITE =
            "https://discordapp.com/oauth2/authorize?client_id=270931284489011202&scope=bot&permissions=305523831";
        
        public const string SORA_ADMIN_ROLE_NAME = "Sora-Admin";
        public const string SORA_DJ_ROLE_NAME = "Sora-DJ";

        public static string[] SuccessLevelEmoji = new string[]
        {
            "✅","⚠","❌","ℹ",""
        };

        private static readonly List<ulong> _ownersNotified = new List<ulong>();
        
        #region Gifs
        public static string[] Pats = new string[]
        {
            "https://media.giphy.com/media/3ohzdLjvu2Q8rQLspq/source.gif",
            "http://i.imgur.com/bDMMk0L.gif",
            "http://i.imgur.com/LxbKriA.gif",
            "http://i.imgur.com/gQ5r1li.gif",
            "http://i.imgur.com/yHsXnMg.gif",
            "http://i.imgur.com/M5kqhq9.gif",
            "http://i.imgur.com/ulbteUq.gif",
            "http://i.imgur.com/DwojHLE.gif",
            "http://i.imgur.com/uyvFoxz.gif",
            "http://i.imgur.com/arv0y4f.gif",
            "https://m.popkey.co/a5cfaf/1x6lW.gif",
            "http://i.imgur.com/otTgjpn.gif",
            "https://media.giphy.com/media/ye7OTQgwmVuVy/giphy.gif",
            "http://i.imgur.com/cPS1JlS.gif",
            "https://media.giphy.com/media/KZQlfylo73AMU/giphy.gif",
            "https://media.giphy.com/media/xgTs8CcCMbqb6/giphy.gif",
            "http://i.imgur.com/d2jbnvs.gif",
        };

        public static string[] Self5 = new string[]
        {
            "https://media.tenor.com/images/7c0aca89e85e83db5d83b1003772544a/tenor.gif",
            "https://media.giphy.com/media/jQTJVqu3Q1hmg/200.gif"
        };
        public static string[] High5 = new string[]
        {
            "https://68.media.tumblr.com/9ef425ac3528b8a56082535c6e9e8138/tumblr_mgitxqyFAP1r2wbr8o1_500.gif",
            "http://pa1.narvii.com/5727/f54721f405a05727c5903e3afb49f5cdc16ef07a_hq.gif",
            "https://media.giphy.com/media/cAiBXaCjbHTry/giphy.gif",
            "http://i.imgur.com/Mghkjt9.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/fc/b1/44/fcb1446b74166b0860ace50ed8b33686.gif",
            "https://static1.gamespot.com/uploads/original/745/7451470/2791604-2345202295-0H6Q9.gif",
            "https://68.media.tumblr.com/398ca8b1c1a0de03078f7dacd4d522b9/tumblr_o7leikmO391tkf3aao1_500.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/17/09/22/170922b20ee616f11629b43d92c45fa7.gif",
            "http://68.media.tumblr.com/0e5d981ef8d70fcc6093b98b3af09091/tumblr_inline_nx3q1hc86b1tyovn6_500.gif",
        };
        public static string[] Hugs = new string[]
        {
            "https://media.giphy.com/media/od5H3PmEG5EVq/giphy.gif",
            "http://i.imgur.com/t4hw0by.gif",
            "https://m.popkey.co/fca5d5/bXDgV.gif",
            "https://media.giphy.com/media/143v0Z4767T15e/giphy.gif",
            "http://i.imgur.com/vbiLwKl.gif",
            "http://i.imgur.com/vbiLwKl.gif",
            "http://i.imgur.com/xDjTItB.gif",
            "http://i.imgur.com/wmU5rg1.gif",
            "https://media.giphy.com/media/du8yT5dStTeMg/giphy.gif",
            "https://media.giphy.com/media/kvKFM3UWg2P04/giphy.gif",
            "https://media.giphy.com/media/wnsgren9NtITS/giphy.gif",
            "http://i.imgur.com/nbWWuYJ.gif",
            "http://i.imgur.com/ffsADGT.gif",
            "https://i.imgur.com/zRMrz14.gif",
            "http://i.imgur.com/TMOK7j2.gif",
            "http://i.imgur.com/6Q3WBmN.gif",
            "http://i.imgur.com/ffsADGT.gif",
            "http://i.imgur.com/lIsXgjx.gif",
            "http://i.imgur.com/Pz3iUsM.gif",
            "http://i.imgur.com/og4QYSX.gif",
            "http://i.imgur.com/lX4CbNN.gif",
            "http://i.imgur.com/dmAp3z4.gif",
            "http://i.imgur.com/0E0KfZa.gif",
            "http://i.imgur.com/Ih9RyRL.gif",
            "http://i.imgur.com/OFYn2hU.gif",
            "https://i.imgur.com/PV8bWpp.gif",
            "https://i.imgur.com/8smF7N2.gif",
            "https://cdn.weeb.sh/images/BkZngAYtb.gif",
            "https://cdn.weeb.sh/images/Hk3ox0tYW.gif",
            "https://cdn.weeb.sh/images/ryPix0Ft-.gif"

        };

        public static string[] Pokes = new string[]
        {
            "https://media.giphy.com/media/ovbDDmY4Kphtu/giphy.gif",
            "http://i.imgur.com/TtV7VRg.gif",
            "https://media.giphy.com/media/pWd3gD577gOqs/giphy.gif",
            "https://media.giphy.com/media/WvVzZ9mCyMjsc/giphy.gif",
            "https://media.giphy.com/media/LXTQN2kRbaqAw/giphy.gif",
            "http://i.imgur.com/1NzLne8.gif",
            "http://i.imgur.com/VtWJ8ak.gif",
            "http://i.imgur.com/rasGw2Z.gif",
            "http://i.imgur.com/g8k3KkH.gif",
            "https://cdn.weeb.sh/images/SyQzRaFFb.gif",
            "https://cdn.weeb.sh/images/rkeaUeJKD-.gif"
        };

        public static string[] Slaps= new string[]
        {
            "https://i.imgur.com/oY3UC4g.gif",
            "http://i.imgur.com/8Q45tO7.gif",
            "http://i.imgur.com/BpTaDPy.gif",
            "http://i.imgur.com/AB07ibk.gif",
            "http://i.imgur.com/MBX2kMu.gif",
            "http://i.imgur.com/CqhzJ72.gif",
            "http://i.imgur.com/Pxom6ma.gif",
            "http://i.imgur.com/6AnlHwX.gif",
            "http://i.imgur.com/yEL7bpC.gif",
            "http://i.imgur.com/K4boPM9.gif",
            "http://i.imgur.com/0DbTtr6.gif",
            "http://i.imgur.com/3rHE4Ee.gif",
            "http://i.imgur.com/ihkVAis.gif",
            "https://i.imgur.com/d9thUdx.gif"
        };

        public static string[] Kisses= new string[]
        {
            "http://i.imgur.com/I9CROFT.gif",
            "http://i.imgur.com/iK5fmug.gif",
            "http://i.imgur.com/dvRHPBL.gif",
            "http://i.imgur.com/brclvvu.gif",
            "http://i.imgur.com/jC0LGI1.gif",
            "http://i.imgur.com/nQ2jGRe.gif",
            "http://i.imgur.com/znX38JU.gif",
            "http://i.imgur.com/kRz9dq0.gif",
            "http://i.imgur.com/3cXlM6i.gif",
            "https://i.imgur.com/Wss0s6p.gif",
            "http://i.imgur.com/AzB99oj.gif",
            "http://i.imgur.com/zkuYWxW.gif",
            "http://i.imgur.com/4ttao29.gif",
            "http://i.imgur.com/USSPwRM.gif",
            "http://i.imgur.com/tCO461O.gif",
            "http://i.imgur.com/GW1BXj8.gif",
            "https://i.imgur.com/IsIR4V0.gif",
            "https://i.imgur.com/Bftud8V.gif",
            "https://i.imgur.com/LBWIJpu.gif",
            "https://i.imgur.com/bvLtDJ4.gif",
            "https://i.imgur.com/FHeoeYJ.gif",
            "https://i.imgur.com/IYorCav.gif",
            "https://i.imgur.com/34aL1AS.gif",
            "https://i.imgur.com/OgGHRYA.gif"
        };

        public static string[] Punches = new string[]
        {
            "http://i.imgur.com/wH4S2CX.gif",
            "http://i.imgur.com/G09HFZs.gif",
            "http://i.imgur.com/GbRgS8h.gif",
            "http://i.imgur.com/tiB026d.gif",
            "http://i.imgur.com/VXlBPm4.gif",
            "http://i.imgur.com/6w2SNY2.gif",
            "http://i.imgur.com/3XQr4pm.gif",
            "http://i.imgur.com/tlbnCVX.gif",
            "http://i.imgur.com/FThVNEf.gif",
            "http://i.imgur.com/KP230Rp.gif"
        };
        #endregion

        public static bool CheckIfSoraAdminExists(SocketGuild guild)
        {
            var admin = guild.Roles.FirstOrDefault(x=> x.Name.Equals(SORA_ADMIN_ROLE_NAME, StringComparison.OrdinalIgnoreCase));
            if (admin == null)
                return false;
            return true;
        }

        public static bool IsSoraAdmin(SocketGuildUser user)
        {
            return user.Roles.Any(x => x.Name.Equals(SORA_ADMIN_ROLE_NAME, StringComparison.Ordinal));
        }

        public static async Task<bool> HasAdminOrSoraAdmin(SocketCommandContext context)
        {
            var user = (SocketGuildUser) context.User;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator) && !IsSoraAdmin(user))
            {
                await context.Channel.SendMessageAsync("", embed:ResultFeedback(
                    RedFailiureEmbed, SuccessLevelEmoji[2], $"You need Administrator permissions or the {SORA_ADMIN_ROLE_NAME} role to do this!"));
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckReadWritePerms(SocketGuild guild, IGuildChannel channel, bool sendMessage = true)
        {
            var guildPerms = guild.CurrentUser.GuildPermissions;
            var chanPerms = guild.CurrentUser.GetPermissions(channel);
            
            if (guildPerms.Has(GuildPermission.Administrator))
                return true;
                        
            if(!guildPerms.Has(GuildPermission.SendMessages) ||
               !guildPerms.Has(GuildPermission.ReadMessages) ||
               !guildPerms.Has(GuildPermission.ReadMessageHistory)||
               !chanPerms.ReadMessages ||
               !chanPerms.ReadMessageHistory ||
               !chanPerms.SendMessages)
            {
                //Send message to owner if not done already. 
                if (sendMessage && !_ownersNotified.Contains(guild.OwnerId))
                {
                    await (await guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("",
                        embed: Utility.ResultFeedback(
                                Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], "Sora lacks permissions!")
                            .WithDescription(
                                "Sora needs global SendMessage, ReadMessage and ReadMessageHistory Permissons! He also requires " +
                                "those permissions in every channel he shall operate. This might be the starboard, user Announcements or Punishlogs!\n" +
                                "This message was sent because he tried to post or edit something and lacked permissions. Thus either the " +
                                "starboard, punish logs, User announcements (join/leave) or every functionality is malfunctioning!\n\n" +
                                $"Guild Affected: {guild.Name} / {guild.Id}\n" +
                                $"Channel Affected: {channel.Name} / {channel.Id}\nLink to channel: <#{channel.Id}>"));
                    _ownersNotified.Add(guild.OwnerId);
                }
                return false;
            }
            return true;
        }


        public static User OnlyGetUser(ulong Id, SoraContext soraContext)
        {
            var result = soraContext.Users.FirstOrDefault(x => x.UserId == Id);
            if (result != null)
            {
                //NECESSARY SHIT SINCE DB EXTENS PERIODICALLY ;(
                var inter = soraContext.Interactions.FirstOrDefault(x => x.UserForeignId == Id) ?? new Interactions();

                var afk = soraContext.Afk.FirstOrDefault(x => x.UserForeignId == Id);

                var marriages =  soraContext.Marriages.Where(x => x.UserForeignId == Id)?.ToList() ?? new List<Marriage>();

                var reminders = soraContext.Reminders.Where(x => x.UserForeignId == Id)?.ToList() ?? new List<Reminders>();

                var shareCentral = soraContext.ShareCentrals.Where(x => x.CreatorId == Id)?.ToList() ?? new List<ShareCentral>();

                var votings = soraContext.Votings.Where(x => x.VoterId == Id)?.ToList() ?? new List<Voting>();

                result.Votings = votings;
                result.ShareCentrals = shareCentral;
                result.Reminders = reminders;
                result.Interactions = inter;
                result.Afk = afk;
                result.Marriages = marriages;
            }
            return result;
        }

        public static GuildUser GetOrCreateGuildUser(ulong userId, ulong guildId, SoraContext soraContext)
        {
            GuildUser gUser = null;
            try
            {
                var guildDb = GetOrCreateGuild(guildId, soraContext);
                gUser = guildDb.Users.FirstOrDefault(x => x.UserId == userId);
                if (gUser == null)
                {
                    gUser = new GuildUser(){Exp = 0, Guild = guildDb, GuildId = guildId, UserId = userId};
                    guildDb.Users.Add(gUser);
                    soraContext.GuildUsers.Add(gUser);
                    soraContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return gUser;
        }

        public static Clan GetClan(Clan clan, SoraContext soraContext)
        {
            try
            {
                var members =
                    soraContext.Users.Where(x => !string.IsNullOrWhiteSpace(x.ClanName) && x.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                        .ToList() ?? new List<User>();
                clan.Members = members;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return clan;
        }

        public static Clan GetClan(string clanName, SoraContext soraContext)
        {
            Clan clan = null;
            try
            {
                clan = soraContext.Clans.FirstOrDefault(
                    x => x.Name.Equals(clanName, StringComparison.OrdinalIgnoreCase));
                //no clan found
                if (clan == null)
                    return null;

                var members =
                    soraContext.Users.Where(x => !string.IsNullOrWhiteSpace(x.ClanName) && x.ClanName.Equals(clanName, StringComparison.OrdinalIgnoreCase))
                        .ToList() ?? new List<User>();
                clan.Members = members;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return clan;
        }
        
        
        public static User GetOrCreateUser(ulong Id, SoraContext soraContext)
        {
            User result = new User();
            try
            {
                result = soraContext.Users.FirstOrDefault(x => x.UserId == Id);
                if (result == null)
                {
                    //User Not found => CREATE
                    var addedUser = soraContext.Users.Add(new User() {UserId = Id, ShareCentrals = new List<ShareCentral>(), Interactions = new Interactions(),Votings = new List<Voting>(),Reminders = new List<Reminders>(),Marriages = new List<Marriage>(),HasBg = false, Notified = false, ClanStaff = false, ClanName = ""});
                    //Set Default action to be false! // Interactions = new Interactions() , Afk = new Afk()
                    //addedUser.Entity.Afk.IsAfk = false; CHANGED
                    //soraContext.SaveChangesThreadSafe();
                    soraContext.SaveChanges();
                    return addedUser.Entity;
                }
                //NECESSARY SHIT SINCE DB EXTENS PERIODICALLY ;(
                var inter = soraContext.Interactions.FirstOrDefault(x => x.UserForeignId == Id) ?? new Interactions();
                var afk = soraContext.Afk.FirstOrDefault(x => x.UserForeignId == Id);
                var marriages =  soraContext.Marriages.Where(x => x.UserForeignId == Id)?.ToList() ?? new List<Marriage>();
                var reminders = soraContext.Reminders.Where(x => x.UserForeignId == Id)?.ToList() ?? new List<Reminders>();

                var shareCentral = soraContext.ShareCentrals.Where(x => x.CreatorId == Id)?.ToList() ?? new List<ShareCentral>();

                var votings = soraContext.Votings.Where(x => x.VoterId == Id)?.ToList() ?? new List<Voting>();
                if (result.ClanName == null)
                    result.ClanName = "";

                result.Votings = votings;
                result.ShareCentrals = shareCentral;
                result.Interactions = inter;
                result.Afk = afk;
                result.Marriages = marriages;
                result.Reminders = reminders;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //soraContext.SaveChangesThreadSafe();
            soraContext.SaveChanges();
            return result;
        }

        public static string GetGuildPrefix(SocketGuild guild, SoraContext soraContext)
        {
            var guildDb = GetOrCreateGuild(guild.Id, soraContext);
            return guildDb.Prefix;
        }
        
        // Faster version of GetGuildPrefix.
        // Tries to allocate as few stuff as possible.
        // We only want the prefix and nothing else.
        public static string GetGuildPrefixFast(SoraContext context, ulong gid, string fallback)
        {
            var guild = context.Guilds.FirstOrDefault(x => x.GuildId == gid);
            if (guild == null)
            {
                return fallback;
            }

            return guild.Prefix;
        }


        public static Guild GetOrCreateGuild(ulong guildId, SoraContext soraContext)
        {
            Guild result = new Guild();
            try
            {
                result = soraContext.Guilds.FirstOrDefault(x => x.GuildId == guildId);
                if (result == null)
                {
                    //Guild not found => Create
                    var addGuild = soraContext.Guilds.Add(new Guild() {GuildId = guildId, Prefix = "$", Tags = new List<Tags>(), Cases = new List<ModCase>(),SelfAssignableRoles = new List<Role>(),IsDjRestricted = false, StarMessages = new List<StarMessage>() ,StarMinimum = 1, Users = new List<GuildUser>(), LevelRoles = new List<GuildLevelRole>()});
                    //soraContext.SaveChangesThreadSafe();
                    soraContext.SaveChanges();
                    return addGuild.Entity;
                }
            
                //NECESSARY SHIT SINCE DB EXTENS PERIODICALLY ;(
                var foundTags = soraContext.Tags.Where(x => x.GuildForeignId == guildId)?.ToList() ?? new List<Tags>();
                var foundStars = soraContext.StarMessages.Where(x => x.GuildForeignId == guildId)?.ToList();
                if (foundStars == null)
                {
                    foundStars = new List<StarMessage>();
                    result.StarMinimum = 1;
                }

                var foundRoles = soraContext.SelfAssignableRoles.Where(x => x.GuildForeignId == guildId)?.ToList() ?? new List<Role>();

                var modCases = soraContext.Cases.Where(x => x.GuildForeignId == guildId)?.ToList() ??
                               new List<ModCase>();

                var users = soraContext.GuildUsers.Where(x => x.GuildId == guildId).ToList() ?? new List<GuildUser>();

                var levelRoles = soraContext.GuildLevelRoles.Where(x => x.GuildId == guildId).ToList() ??
                                 new List<GuildLevelRole>();

                result.LevelRoles = levelRoles;
                result.Users = users;
                result.Cases = modCases;
                result.SelfAssignableRoles = foundRoles;
                result.Tags = foundTags;
                result.StarMessages = foundStars;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            //guild found
            //soraContext.SaveChangesThreadSafe();
            soraContext.SaveChanges();
            return result;

        }

        public static double CalculateAffinity(Interactions interactions)
        {
            double total = interactions.Pats+ interactions.High5 + interactions.Hugs * 2 + interactions.Kisses* 3 + interactions.Slaps + interactions.Punches*2;
            double good = interactions.Pats + interactions.High5+interactions.Hugs * 2 + interactions.Kisses * 3;
            if (total == 0)
                return 0;
            if (good == 0)
                return 0;
            return Math.Round((100.0 / total * good), 2);
        }

        public static EmbedBuilder ResultFeedback(Discord.Color color, string symbol, string text)
        {
            var eb = new EmbedBuilder()
            {
                Color = color,
                Title = $"{symbol} {text}"
            };
            return eb;
        }

        public static EmbedFooterBuilder RequestedBy(SocketUser user)
        {
            return new EmbedFooterBuilder()
            {
                Text = $"Requested by {Utility.GiveUsernameDiscrimComb(user)}",
                IconUrl = user.GetAvatarUrl() ?? StandardDiscordAvatar
            };
        }

        public static string GiveUsernameDiscrimComb(SocketUser user)
        {
            return user == null ? "User Unknown" : $"{user.Username}#{user.Discriminator}";
        }
    }
}