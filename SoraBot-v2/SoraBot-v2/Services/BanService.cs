using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;

namespace SoraBot_v2.Services
{
    public class BanService
    {
        // list of all banned users to minimize latency in CommandHandler
        private ConcurrentDictionary<ulong, bool> _bannedUsers = new ConcurrentDictionary<ulong, bool>();

        // Fetch list once from DB
        public void FetchBannedUsers()
        {
            using (var soraContext = new SoraContext())
            {
                var bans = soraContext.Bans.ToList() ?? new List<Ban>();
                if (bans.Count == 0)
                {
                    // if there are no bans we dont need to read anything.
                    return;
                }
                // Save Bans in local Dictionary for fast access cache
                foreach (var ban in bans)
                {
                    _bannedUsers.TryAdd(ban.UserId, true);
                }
            }
        }

        public async Task GetBanInfo(SocketCommandContext context, ulong userId)
        {
            using (var soraContext = new SoraContext())
            {
                var ban = soraContext.Bans.FirstOrDefault(x => x.UserId == userId);
                if (ban == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "User is not banned or failed to fetch data."
                    ).Build());
                    return;
                }
                
                var eb = new EmbedBuilder()
                {
                    Color = Utility.BlueInfoEmbed,
                    Footer = Utility.RequestedBy(context.User),
                    Title = $"Global Ban of {userId}",
                    Description = $"Banned On: {ban.BannedAt.ToString("dd/MM/yyyy")}"
                };
                
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Reason";
                    x.Value = (string.IsNullOrWhiteSpace(ban.Reason) ? "Unknown" : ban.Reason);
                });

                await context.Channel.SendMessageAsync("", embed: eb.Build());
            }
        }
        
        // Update incoming from webservice
        public void BanUserEvent(ulong id)
        {
            _bannedUsers.TryAdd(id, true);
            Console.WriteLine($"!!! Got and registered Ban Event from ${id} !!!");
        }
        
        // UnBan user Command
        public async Task<bool> UnBanUser(ulong id)
        {
            if (!_bannedUsers.TryRemove(id, out _))
            {
                return false;
            }
            // remove from DB
            using (var soraContext = new SoraContext())
            {
                soraContext.Bans.Remove(soraContext.Bans.FirstOrDefault(x => x.UserId == id));
                await soraContext.SaveChangesAsync();
            }
            // notify other shards to ban user
            int port = int.Parse(ConfigService.GetConfigData("port"));
            for (int i = 0; i < Utility.TOTAL_SHARDS; i++)
            {
                if(i == Utility.SHARD_ID)
                    continue;
                
                try
                {
                    using (var httpClient = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{(port+i)}/api/SoraApi/UnBanEvent/"))
                    {
                        string json = JsonConvert.SerializeObject(new { userId = id });
                        request.Content = new StringContent(json);
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        response.Dispose(); 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await SentryService.SendMessage($"COULDN'T SEND UNBAN EVENT TO SHARD {i} FOR ID: {id}");
                }
            }
            return true;
        }
        
        
        // Ban user Command
        public async Task<bool> BanUser(ulong id, string reason)
        {
            if (!_bannedUsers.TryAdd(id, true))
            {
                return false;
            }
            // Add to DB
            using (var soraContext = new SoraContext())
            {
                if (string.IsNullOrWhiteSpace(reason))
                    reason = "";
                
                soraContext.Bans.Add(new Ban()
                {
                    UserId = id,
                    BannedAt = DateTime.UtcNow,
                    Reason = reason
                });
                await soraContext.SaveChangesAsync();
            }
            // notify other shards to ban user
            int port = int.Parse(ConfigService.GetConfigData("port"));
            for (int i = 0; i < Utility.TOTAL_SHARDS; i++)
            {
                if(i == Utility.SHARD_ID)
                    continue;
                
                try
                {
                    using (var httpClient = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{(port+i)}/api/SoraApi/BanEvent/"))
                    {
                        string json = JsonConvert.SerializeObject(new { userId = id });
                        request.Content = new StringContent(json);
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        response.Dispose(); 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await SentryService.SendMessage($"COULDN'T SEND BAN EVENT TO SHARD {i} FOR ID: {id}");
                }
            }
            return true;
        }
        
        // check if  User Id is banned
        public bool IsBanned(ulong id)
        {
            if (_bannedUsers.ContainsKey(id))
            {
                // User is banned
                return true;
            }
            return false;
        }
        
        // Update incoming from webservice
        public void UnBanUserEvent(ulong id)
        {
            _bannedUsers.TryRemove(id, out _);
            Console.WriteLine($"!!! Got and registered UnBan Event from {id} !!!");
        }
    }
}