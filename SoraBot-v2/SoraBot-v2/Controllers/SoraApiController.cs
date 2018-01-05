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
using SoraBot_v2.Data;

namespace SoraBot_v2.Controllers
{
    [Route("/api/[controller]")]
    public class SoraApiController : Controller
    {
        private DiscordSocketClient _client;

        public SoraApiController(DiscordSocketClient client)
        {
            _client = client;
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
                            guildDb.EmbedWelcome = announc.Embed;
                            guildDb.WelcomeChannelId = channel.Id;
                            guildDb.WelcomeMessage = announc.Message;
                            break;
                        case "l":
                            guildDb.EmbedLeave = announc.Embed;
                            guildDb.LeaveChannelId = channel.Id;
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