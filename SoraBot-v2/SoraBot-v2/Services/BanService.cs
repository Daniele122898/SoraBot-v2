﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
        
        // Update incoming from webservice
        public void BanUserEvent(ulong id)
        {
            _bannedUsers.TryAdd(id, true);
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
            for (int i = 0; i < Utility.TOTAL_SHARDS; i++)
            {
                if(i == Utility.SHARD_ID)
                    continue;
                
                try
                {
                    using (var httpClient = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{(8087+i)}/api/SoraApi/BanEvent/"))
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
    }
}