using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SoraBot_v2.Services;
using SoraBot_v2.WebApiModels;
using Humanizer;
using Microsoft.EntityFrameworkCore.Internal;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Controllers
{
    [Route("/api/[controller]")]
    public class SoraApiController : Controller
    {
        private DiscordSocketClient _client;
        private BanService _banService;

        public SoraApiController(DiscordSocketClient client, BanService banService)
        {
            _client = client;
            _banService = banService;
        }
        
        [HttpGet("GetSoraStats/", Name = "SoraStats")]
        [EnableCors("AllowLocal")]
        public SoraStats GetSoraStats()
        {
            var userCount = 0;
            foreach (var g in _client.Guilds)
            {
                userCount += g.MemberCount;
            }
            return new SoraStats() {
                CommandsExecuted = CommandHandler.CommandsExecuted,
                MessagesReceived = CommandHandler.MessagesReceived,
                Version = Utility.SORA_VERSION,
                Ping = _client.Latency,
                GuildCount = _client.Guilds.Count,
                UserCount = userCount
            };
        }

        public List<SocketGuild> GetPermGuilds(ulong userId)
        {
            var guilds = _client.Guilds.Where(x => x.GetUser(userId) != null).ToList();
            if (guilds.Count == 0)
                return null;
            
            List<SocketGuild> permGuilds = new List<SocketGuild>();
            foreach (var g in guilds)
            {
                var u = g.GetUser(userId);
                if (g.OwnerId == userId || u.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    permGuilds.Add(g);
                }
            }
            return permGuilds;
        }

        [HttpGet("GetAllWaifus/", Name = "GetAllWaifus")]
        [EnableCors("AllowLocal")]
        public AllWaifus GetAllWaifus(ulong userId)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var waifus = new AllWaifus();
                    // add up all waifus
                    foreach (var waifu in soraContext.Waifus)
                    {
                        waifus.Waifus.Add(waifu);
                    }
                    // send all waifus
                    return waifus;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        [HttpGet("GetUserWaifus/{userId}", Name = "GetUserWaifus")]
        [EnableCors("AllowLocal")]
        public UserWaifusAPI GetUserWaifus(ulong userId)
        {
            try
            {
                var user = _client.GetUser(userId);
                using (var soraContext = new SoraContext())
                {
                    var userwaifus = new UserWaifusAPI();
                    var userdb = Utility.OnlyGetUser(userId, soraContext);
                    if (userdb == null || userdb.UserWaifus.Count == 0)
                    {
                        userwaifus.Success = false;
                        return userwaifus;
                    }

                    userwaifus.Success = true;
                    userwaifus.Username = user?.Username ?? "Undefined";
                    userwaifus.AvatarUrl = user?.GetAvatarUrl();

                    foreach (var userWaifu in userdb.UserWaifus)
                    {
                        var waifu = soraContext.Waifus.FirstOrDefault(x => x.Id == userWaifu.WaifuId);
                        if (waifu == null)
                            continue;
                        
                        userwaifus.Waifus.Add(new UserWaifuAPI()
                        {
                            Count = userWaifu.Count,
                            Id = waifu.Id,
                            ImageUrl = waifu.ImageUrl,
                            Name = waifu.Name,
                            Rarity = WaifuService.GetRarityString(waifu.Rarity),
                            SortRarity = waifu.Rarity
                        });
                    }

                    if (userwaifus.Waifus.Count ==0)
                    {
                        userwaifus.Success = false;
                        return userwaifus;
                    }

                    userwaifus.Waifus = userwaifus.Waifus.OrderByDescending(x => x.SortRarity).ToList();

                    return userwaifus;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        [HttpGet("GetGuilds/{userId}", Name = "GetGuilds")]
        [EnableCors("AllowLocal")]
        public UserGuilds GetGuilds(ulong userId)
        {
            try
            {
                var permGuilds = GetPermGuilds(userId);
                if (permGuilds == null || permGuilds.Count == 0)
                    return null;
                var send = new UserGuilds();
                send.UserId = userId.ToString();
                using (var soraContext = new SoraContext())
                {
                    foreach (var guild in permGuilds)
                    {
                        var gdb = Utility.GetOrCreateGuild(guild.Id, soraContext);
                        var bot = guild.GetUser(_client.CurrentUser.Id);
                        var b = bot.GuildPermissions;
                        int online = guild.Users.Count(socketGuildUser =>
                            socketGuildUser.Status != UserStatus.Invisible &&
                            socketGuildUser.Status != UserStatus.Offline);
                        send.Guilds.Add(new WebGuild()
                        {
                            GuildId = guild.Id.ToString(),
                            IconUrl = guild.IconUrl,
                            MemberCount = guild.MemberCount,
                            Name = guild.Name,
                            Owner = new WebUser()
                            {
                                AvatarUrl = guild.Owner.GetAvatarUrl(),
                                Discriminator = guild.Owner.Discriminator,
                                Id = guild.OwnerId.ToString(),
                                Name = guild.Owner.Username
                            },
                            OnlineMembers = online,
                            Region = (guild.VoiceRegionId).Humanize().Transform(To.LowerCase, To.TitleCase),
                            RoleCount = guild.Roles.Count,
                            TextChannelCount = guild.TextChannels.Count,
                            VoiceChannelCount = guild.VoiceChannels.Count,
                            AfkChannel =
                            (guild.AFKChannel == null
                                ? $"No AFK Channel"
                                : $"{guild.AFKChannel.Name}\nin {(guild.AFKTimeout / 60)} Min"),
                            EmoteCount = guild.Emotes.Count,
                            Prefix = gdb.Prefix,
                            IsDjRestricted = gdb.IsDjRestricted,
                            StarMinimum = gdb.StarMinimum,
                            TagCount = gdb.Tags.Count,
                            StarMessageCount = gdb.StarMessages.Count,
                            SarCount = gdb.SelfAssignableRoles.Count,
                            ModCaseCount = gdb.Cases.Count,
                            SoraPerms = new SoraPerms()
                            {
                                AddReactions = b.AddReactions,
                                BanMembers = b.BanMembers,
                                KickMembers = b.KickMembers,
                                ManageChannels = b.ManageChannels,
                                ReadMessageHistory = b.ReadMessageHistory,
                                ManageRoles = b.ManageRoles,
                                ManageMessages = b.ManageMessages,
                                ReadMessages = b.ReadMessages,
                                EmbedLinks = b.EmbedLinks,
                                SendMessages = b.SendMessages
                            }
                        });
                    }
                    return send;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpPost("EditStarboard/", Name = "EditStarboard")]
        [EnableCors("EditStarboard")]
        public bool EditStarboard([FromBody] WebStarboardEdit star)
        {
            try
            {
                var permGuilds = GetPermGuilds(ulong.Parse(star.UserId));
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return false;
                }
                var guildId = ulong.Parse(star.GuildId);
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return false;
                }
                var channel = guild.GetTextChannel(ulong.Parse(star.ChannelId));
                if (channel == null)
                {
                    Console.WriteLine("FOUND NO CHANNEL WITH ID");
                    return false;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    //change
                    guildDb.StarChannelId = star.Disabled ? 0 : channel.Id;
                    guildDb.StarMinimum = star.StarMin;
                    soraContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
        
        [HttpPost("BanEvent/", Name = "BanEvent")]
        [EnableCors("AllowLocal")]
        public bool BanEvent([FromBody] BanUserEvent banUserEvent)
        {
            try
            {
                _banService.BanUserEvent(banUserEvent.UserId);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
        
        [HttpPost("UnBanEvent/", Name = "UnBanEvent")]
        [EnableCors("AllowLocal")]
        public bool UnBanEvent([FromBody] BanUserEvent banUserEvent)
        {
            try
            {
                _banService.UnBanUserEvent(banUserEvent.UserId);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        [HttpGet("GetGuildLevels/{userid}/{guildid}", Name = "GetGuildLevels")]
        [EnableCors("AllowLocal")]
        public GuildLevels GetGuildLevels(ulong userId, ulong guildId)
        {
            try
            {
                var permGuilds = GetPermGuilds(userId);
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return null;
                }
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return null;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    var resp = new GuildLevels()
                    {
                        EnabledLvlUpMessage = guildDb.EnabledLvlUpMessage,
                        LevelUpMessage = (string.IsNullOrWhiteSpace(guildDb.LevelUpMessage) ? GuildLevelRoleService.DEFAULT_MSG : guildDb.LevelUpMessage),
                        SendLvlDm = guildDb.SendLvlDm
                    };
                    var banned = guildDb.LevelRoles.Where(x => x.Banned).ToList();
                    var normal = guildDb.LevelRoles.Where(x => !x.Banned).ToList();
                    var otherRoles = guild.Roles;
                    foreach (var role in otherRoles)
                    {                        
                        if(role.Name == "@everyone")
                            continue;
                        if(role.IsManaged)
                            continue;
                        
                        var addRole = new WebLevelRole()
                        {
                            RoleId = role.Id.ToString(),
                            RGBColor = $"rgb({role.Color.R}, {role.Color.G}, {role.Color.B})",
                            RoleName = role.Name
                        };
                        //check if role is already set to be rewarded
                        var lvlRole = normal.FirstOrDefault(x => x.RoleId == role.Id);
                        var ban = banned.FirstOrDefault(x => x.RoleId == role.Id);
                        addRole.Banned = ban?.Banned ?? false;
                        addRole.RequiredLevel = lvlRole?.RequiredLevel ?? 0;
                        resp.LevelRoles.Add(addRole);
                    }
                    return resp;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpGet("GetStarboard/{userid}/{guildid}", Name = "GetStarboard")]
        [EnableCors("AllowLocal")]
        public WebStarboard GetStarboard(ulong userId, ulong guildId)
        {
            try
            {
                var permGuilds = GetPermGuilds(userId);
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return null;
                }
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return null;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    var resp = new WebStarboard()
                    {
                        StarChannelId = guildDb.StarChannelId.ToString(),
                        StarMinimum = guildDb.StarMinimum
                    };
                    foreach (var chan in guild.TextChannels)
                    {
                        resp.Channels.Add(new WebGuildChannel()
                        {
                            Name = chan.Name,
                            Id = chan.Id.ToString()
                        });
                    }
                    return resp;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpGet("GuildExists/{guildid}", Name = "GuildExists")]
        [EnableCors("AllowLocal")]
        public bool GuildExists(ulong guildId)
        {
            if (_client.GetGuild(guildId) == null)
            {
                return false;
            }
            return true;
        }

        [HttpGet("GetGlobalLeaderboard/", Name = "GetGlobalLeaderboard")]
        [EnableCors("AllowLocal")]
        public GlobalLeaderboard GetGlobalLeaderboard()
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var users = soraContext.Users.ToList();
                    var sorted = users.OrderByDescending(x => x.Exp).ToList();
                    var resp = new GlobalLeaderboard(){ShardId = _client.ShardId};
                    for (int i = 0; i < (sorted.Count > 150 ? 150 : sorted.Count); i++)
                    {
                        var guser = sorted[i];
                        var user = _client.GetUser(guser.UserId);
                        if (user == null)
                        {
                            continue;
                        }
                        resp.Ranks.Add(new GuildRank()
                        {
                            Rank = i+1,
                            AvatarUrl = user.GetAvatarUrl() ?? "https://i.imgur.com/PvYs6dc.png",
                            Discrim = user.Discriminator,
                            Exp = (int)guser.Exp,
                            Name = user.Username,
                            NextExp = ExpService.CalculateNeededExp(ExpService.CalculateLevel(guser.Exp)+1),
                            UserId = user.Id+""
                        });
                        if(resp.Ranks.Count >= 100)
                            break;
                    }
                    return resp;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpGet("GetGuildLeaderboard/{guildid}", Name = "GetGuildLeaderboard")]
        [EnableCors("AllowLocal")]
        public GuildLeaderboard GetGuildLeaderboard(ulong guildId)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var guild = _client.GetGuild(guildId);
                    if (guild == null)
                    {
                        return new GuildLeaderboard()
                        {
                            Success = false
                        };
                    }
                    var resp = new GuildLeaderboard()
                    {
                        Success = true,
                        AvatarUrl = guild.IconUrl ?? "https://i.imgur.com/PvYs6dc.png",
                        GuildName = guild.Name
                    };
                    var levelRoles = soraContext.GuildLevelRoles.Where(x => x.GuildId == guildId).ToList();
                    var guildUsers = soraContext.GuildUsers.Where(x => x.GuildId == guildId).ToList();
                    var sorted = guildUsers.OrderByDescending(x => x.Exp).ToList();
                    var sortedLvls = levelRoles.OrderBy(x => x.RequiredLevel).ToList();
                    for(int i = 0; i<sortedLvls.Count; i++)
                    {
                        var role = sortedLvls[i];
                        var r = guild.GetRole(role.RoleId);
                        if (r == null || role.Banned)
                        {
                            continue;
                        }
                        resp.RoleRewards.Add(new RoleReward()
                        {
                            Name = r.Name,
                            Color = $"rgb({r.Color.R}, {r.Color.G}, {r.Color.B})",
                            LevelReq = role.RequiredLevel
                        });
                    }
                    int rank = 1;
                    for(int i = 0; i<sorted.Count; i++)
                    {
                        var user = sorted[i];
                        if (rank > 100)
                        {
                            break;
                        }
                        var u = guild.GetUser(user.UserId);
                        if (u == null)
                        {
                            continue;
                        }
                        resp.Ranks.Add(new GuildRank()
                        {
                            AvatarUrl = u.GetAvatarUrl() ?? "https://i.imgur.com/PvYs6dc.png",
                            Discrim = u.Discriminator,
                            Exp = (int)user.Exp,
                            Name = u.Username,
                            NextExp = ExpService.CalculateNeededExp(ExpService.CalculateLevel(user.Exp)+1),
                            Rank = rank
                        });
                        rank++;
                    }
                    return resp;
                }   
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return new GuildLeaderboard(){Success = false};
        }

        [HttpGet("GetGuildAnnouncements/{userid}/{guildid}", Name = "GetGuildAnnouncements")]
        [EnableCors("AllowLocal")]
        public GuildAnnouncements GetGuildAnnouncements(ulong userId,ulong guildId)
        {
            try
            {
                var permGuilds = GetPermGuilds(userId);
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return null;
                }
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return null;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    var resp = new GuildAnnouncements()
                    {
                        WelcomeChannelId = guildDb.WelcomeChannelId.ToString(),
                        WelcomeMessage = guildDb.WelcomeMessage,
                        LeaveChannelId = guildDb.LeaveChannelId.ToString(),
                        LeaveMessage = guildDb.LeaveMessage,
                        EmbedWelcome = guildDb.EmbedWelcome,
                        EmbedLeave = guildDb.EmbedLeave
                    };
                    foreach (var chan in guild.TextChannels)
                    {
                        resp.Channels.Add(new WebGuildChannel()
                        {
                            Name = chan.Name,
                            Id = chan.Id.ToString()
                        });
                    }
                    return resp;
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpPost("EditGuildLevels/", Name = "EditGuildLevels")]
        [EnableCors("AllowLocal")]
        public bool EditGuildLevels([FromBody] GuildLevelEdit lvlEdit)
        {
            try
            {
                var permGuilds = GetPermGuilds(ulong.Parse(lvlEdit.UserId));
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return false;
                }
                var guildId = ulong.Parse(lvlEdit.GuildId);
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return false;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    var banned = guildDb.LevelRoles.Where(x => x.Banned).ToList();
                    //clear the banned list
                    foreach (var role in banned)
                    {
                        if (!lvlEdit.BannedRoles.Contains(role.RoleId.ToString()))
                        {
                            guildDb.LevelRoles.Remove(role);
                        }
                    }
                    
                    foreach (var bannedRole in lvlEdit.BannedRoles)
                    {
                        var id = ulong.Parse(bannedRole);
                        //check if already in the list
                        if(banned.Any(x=> x.RoleId == id))
                            continue;
                        //otherwise add
                        //check if role exists!
                        var role = guild.GetRole(id);
                        if (role == null)
                            continue;
                        
                        guildDb.LevelRoles.Add(new GuildLevelRole()
                        {
                            Banned = true,
                            GuildId = guildId,
                            RequiredLevel = 0,
                            RoleId = id
                        });
                    }
                    //check for all the other roles now
                    foreach (var lvlRole in lvlEdit.Roles)
                    {
                        //check if role exists
                        var id = ulong.Parse(lvlRole.RoleId);
                        var role = guild.GetRole(id);
                        var fR = guildDb.LevelRoles.FirstOrDefault(x => x.RoleId == id);
                        if (role == null)
                        {
                            //role didnt exist BUT it was in the rewards list
                            if (fR != null)
                            {
                                guildDb.LevelRoles.Remove(fR);
                            }
                            continue;
                        }
                        //otherwise update or create new entry for new role!
                        //if required level is 0 then remove it
                        if (fR != null)
                        {
                            if (lvlRole.LvlReq == 0)
                            {
                                if (!fR.Banned)
                                {
                                    guildDb.LevelRoles.Remove(fR);
                                }
                            }
                            else
                            {
                                fR.RequiredLevel = lvlRole.LvlReq;
                            }
                        } //otherwise add
                        else
                        {
                            if (lvlRole.LvlReq == 0)
                                continue;
                            guildDb.LevelRoles.Add(new GuildLevelRole()
                            {
                                Banned = false,
                                GuildId = guild.Id,
                                RequiredLevel = lvlRole.LvlReq,
                                RoleId = role.Id
                            });
                        }
                    }
                    guildDb.EnabledLvlUpMessage = lvlEdit.EnableAnn;
                    guildDb.SendLvlDm = lvlEdit.SendDm;
                    guildDb.LevelUpMessage = lvlEdit.LvlUpMsg;
                    soraContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        [HttpPost("EditPrefix/", Name = "EditPrefix")]
        [EnableCors("AllowLocal")]
        public bool ChangePrefix([FromBody] WebPrefix prefix)
        {
            try
            {
                var permGuilds = GetPermGuilds(ulong.Parse(prefix.UserId));
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return false;
                }
                var guildId = ulong.Parse(prefix.GuildId);
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return false;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    guildDb.Prefix = prefix.Prefix;
                    soraContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }


        [HttpPost("EditAnnouncement/", Name = "EditAnnouncement")]
        [EnableCors("AllowLocal")]
        public bool EditAnnouncement([FromBody] EditAnn announc)
        {
            try
            {
                var permGuilds = GetPermGuilds(ulong.Parse(announc.UserId));
                if (permGuilds == null || permGuilds.Count == 0)
                {
                    Console.WriteLine("FOUND NO PERM GUILDS");
                    return false;
                }
                var guildId = ulong.Parse(announc.GuildId);
                var guild = permGuilds.FirstOrDefault(x => x.Id == guildId);
                if (guild == null)
                {
                    Console.WriteLine("FOUND NO GUILD WITH ID");
                    return false;
                }
                var channel = guild.GetTextChannel(ulong.Parse(announc.ChannelId));
                if (channel == null)
                {
                    Console.WriteLine("FOUND NO CHANNEL WITH ID");
                    return false;
                }
                using (var soraContext = new SoraContext())
                {
                    var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                    switch (announc.Type)
                    {
                        case "w":
                            guildDb.WelcomeChannelId = announc.Disabled ? 0 : channel.Id;
                            guildDb.EmbedWelcome = announc.Embed;
                            guildDb.WelcomeMessage = announc.Message;
                            
                            break;
                        case "l":
                            guildDb.EmbedLeave = announc.Embed;
                            guildDb.LeaveChannelId =  announc.Disabled ? 0 : channel.Id;
                            guildDb.LeaveMessage = announc.Message;
                            break;
                        default:
                            return false;
                    }
                    soraContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
            return false;
        }
    }
}