using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class TransferData
    {
        private IServiceProvider _services;

        /*
        private string _dataDict = "transfer/";
        
        private readonly JsonSerializer jSerializer = new JsonSerializer();
        
        //starboard channels
        private ConcurrentDictionary<ulong, ulong> _starChannelDict = new ConcurrentDictionary<ulong, ulong>();
        //prefix
        private ConcurrentDictionary<ulong, string> _prefixDict = new ConcurrentDictionary<ulong, string>();
        //announcements
        private ConcurrentDictionary<ulong, annoucementStruct> _announcements =
            new ConcurrentDictionary<ulong, annoucementStruct>();
                
        public struct annoucementStruct
        {
            public ulong ChannelId;
            public ulong LeaveId;
            public string Message;
            public string LeaveMsg;
        }
        
        //punishlogs
        private class PunishData
        {
            public ulong channelID { get; set; }
        }
        private ConcurrentDictionary<ulong, PunishData> _punishLogs = new ConcurrentDictionary<ulong, PunishData>();
        
        //User EP
        private ConcurrentDictionary<ulong, UserStruct> _userEPDict = new ConcurrentDictionary<ulong, UserStruct>();
        
        private struct UserStruct
        {
            public float Ep;
        }
        
        private List<ulong> lvlSubsriberList = new List<ulong>();
*/
        
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
            /*
            //PREFIXES
            LoadGuildPrefixes();
            
            //starboard
            LoadStarboardChannels();
            
            //Announcements
            LoadAnnouncements();
            
            //Punishlogs
            LoadPunishlogs();
            
            //User EP
            LoadUserEp();
            LoadEpSubscriber();

            //JSON INITIALIZER
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
            */
        }

        public async Task MessageAllGuildOwners(SocketCommandContext context)
        {
            var client = context.Client;
            List<ulong> owners = new List<ulong>();
            foreach (var guild in client.Guilds)
            {
                if(guild?.Owner == null)
                    continue;
                if(!owners.Contains(guild.Owner.Id))
                    owners.Add(guild.Owner.Id);
            }
            owners = owners.Distinct().ToList();
            var eb = new EmbedBuilder()
            {
                Color = Utility.YellowWarningEmbed,
                Title = $"{Utility.SuccessLevelEmoji[1]} IMPORTANT NOTICE",
                ThumbnailUrl = client.CurrentUser.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                Description = $"Hello (\\*≧ω≦\\*)\n" +
                              $"I've never done anything like this and never will again but this is important for all you guild owners currently using Sora.\n" +
                              $"Sora has been completely reworked from the ground up. Every line of code has been rewritten.\n" +
                              $"Because of that many commands changed and some new ones arrived. I copied the most important data from Sora v1 over to v2.\n" +
                              $"This includes UserEP, Guild Prefix, starboard, punishlog and join/leave announcements.\n" +
                              $"I wasn't able to take in the rest just because the system changed so much that it wasn't compatible anymore\n" +
                              $"The data used is around 24h old tho so if any changes have been made those were not copied over!\n" +
                              $"I've written an **extremely** extensive wiki that will teach you EVERY command in Sora.\n" +
                              $"[Click here to go to the wiki](http://git.argus.moe/serenity/SoraBot-v2/wikis/home)\n\n" +
                              $"Also the Music portion of Sora saw the biggest change and is now extremely extensive,\n" +
                              $"fast and efficient and should be lag free. You can also save and share playlists/queues\n" +
                              $"(Btw he supports playlists of nearly any length now ;))\n" +
                              $"Sora might still have a couple bugs but I'm working hard on making him bugfree.\n" +
                              $"If you need any help or have further questions\n" +
                              $"[Join this Guild]({Utility.DISCORD_INVITE})\n" +
                              $"This is not the last change in Sora tho. MANY new features are currently in development ;)\n" +
                              $"Enjoy and thank you for using Sora ♥\n" +
                              $"**TL;DR:** Sora changed alot read the new wiki linked up there ^\n\n" +
                              $"PS: [You can support Sora by upvoting him here. It's really easy. You can login with Discord](https://discordbots.org/bot/270931284489011202)"
            };
            int count = 0;
            foreach (var owner in owners)
            {
                try
                {
                    var user = client.GetUser(owner);
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb);
                    count++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], $"Successfully notified {count} out of {owners.Count} Guild Owners"));
        }
    /*
        public async Task SyncAllUsers(SocketCommandContext context)
        {                
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                try
                {
                    await context.Channel.SendMessageAsync("Syncing Users...");
                    foreach (var userStruct in _userEPDict)
                    {
                        var userDb = Utility.GetOrCreateUser(userStruct.Key, soraContext);
                        if (lvlSubsriberList.Contains(userStruct.Key))
                        {
                            userDb.Notified = true;
                        }
                        userDb.Exp = userStruct.Value.Ep;
                        //Console.WriteLine($"[{DateTime.UtcNow.TimeOfDay}] Transfered User of {userStruct.Key}, {count} out of {amount}");
                    }
                    await soraContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            int amount = _userEPDict.Count;
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], $"Successfully synced {amount} old User data"));
        }

        public async Task SyncAllGuilds(SocketCommandContext context)
        {
            //sync the starboard channel, sync the announcements, sync punishlogs, sync prefix
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                int count = 0;
                int amount = _prefixDict.Count;
                try
                {
                    await context.Channel.SendMessageAsync("Syncing Prefixes...");
                    //prefixes
                    
                    foreach (var prefix in _prefixDict)
                    {
                        var guildDb = Utility.GetOrCreateGuild(prefix.Key, soraContext);

                        guildDb.Prefix = prefix.Value;
                        count++;
                        Console.WriteLine($"Transfered prefix of {prefix.Key}, {count} out of {amount}");
                    }
                    await soraContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    await context.Channel.SendMessageAsync("Syncing Starchannels...");
                    amount =_starChannelDict.Count;
                    count = 0;
                    //Starchannels
                    foreach (var starchannel in _starChannelDict)
                    {
                        var guildDb = Utility.GetOrCreateGuild(starchannel.Key, soraContext);
                        guildDb.StarChannelId = starchannel.Value;
                        count++;
                        Console.WriteLine($"Transfered Starchannel of {starchannel.Key}, {count} out of {amount}");
                    }
                    await soraContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    await context.Channel.SendMessageAsync("Syncing Announcements... ");
                    amount =_announcements.Count;
                    count = 0;
                    //sync announcements
                    foreach (var annoucementStruct in _announcements)
                    {
                        var guildDb = Utility.GetOrCreateGuild(annoucementStruct.Key, soraContext);
                        guildDb.LeaveChannelId = annoucementStruct.Value.LeaveId;
                        guildDb.LeaveMessage = annoucementStruct.Value.LeaveMsg;
                        guildDb.WelcomeChannelId = annoucementStruct.Value.ChannelId;
                        guildDb.WelcomeMessage = annoucementStruct.Value.Message;
                        count++;
                        Console.WriteLine($"Transfered Announcement of {annoucementStruct.Key}, {count} out of {amount}");
                    }
                    await soraContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    await context.Channel.SendMessageAsync("Syncing Punishlogs...");
                    amount =_punishLogs.Count;
                    count = 0;
                    //punishlogs
                    foreach (var punishData in _punishLogs)
                    {
                        var guildDb = Utility.GetOrCreateGuild(punishData.Key, soraContext);
                        guildDb.PunishLogsId = punishData.Value.channelID;
                        count++;
                        Console.WriteLine($"Transfered punishlogs of {punishData.Key}, {count} out of {amount}");
                    }
                    await soraContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully synced ALL old guild data"));

            }
            
        }
        
        private void LoadEpSubscriber()
        {
            if (File.Exists($"{_dataDict}UserEPSubscriber.json"))
            {
                using (StreamReader sr = File.OpenText($"{_dataDict}UserEPSubscriber.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var blackListguildTemp = jSerializer.Deserialize<List<ulong>>(reader);
                        if (blackListguildTemp == null)
                            return;
                        lvlSubsriberList = blackListguildTemp;
                    }
                }
            }
            else
            {
                Console.WriteLine("COULDN'T FIND SUBS!");
                throw new Exception(" SUBS");
            }
        }
        
        private void LoadUserEp()
        {
            if (File.Exists($"{_dataDict}UserEP.json"))
            {
                using (StreamReader sr = File.OpenText($"{_dataDict}UserEP.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var userEPDictTemp = jSerializer.Deserialize<ConcurrentDictionary<ulong, UserStruct>>(reader);
                        if (userEPDictTemp == null)
                            return;
                        _userEPDict = userEPDictTemp;
                    }
                }
            }
            else
            {
                Console.WriteLine("COULDN'T FIND USER EP!");
                throw new Exception("USER EP");
            }
        }
        
        private void LoadPunishlogs()
        {
            try
            {
                if (File.Exists($"{_dataDict}PunishLogs.json"))
                {
                    using (StreamReader sr = File.OpenText($"{_dataDict}PunishLogs.json"))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var temp = jSerializer.Deserialize<ConcurrentDictionary<ulong, PunishData>>(reader);
                            if (temp == null)
                                return;
                            _punishLogs = temp;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("COULDN'T FIND PUNISHLOGS!");
                    throw new Exception("PUNISH LOOOOOOOOOOOOOOOOOOOGS");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private void LoadAnnouncements()
        {
            try
            {
                if (File.Exists($"{_dataDict}AnnouncementChannels.json"))
                {
                    using (StreamReader sr = File.OpenText($"{_dataDict}AnnouncementChannels.json"))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var temp = jSerializer.Deserialize<ConcurrentDictionary<ulong, annoucementStruct>>(reader);
                            if (temp == null)
                                return;
                            _announcements = temp;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("COULDN'T FIND ANNOUNCEMENTS!");
                    throw new Exception("ANNOUNCEMENTS");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private void LoadGuildPrefixes()
        {
            if (File.Exists($"{_dataDict}guildPrefix.json"))
            {
                using (StreamReader sr = File.OpenText($"{_dataDict}guildPrefix.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        _prefixDict = jSerializer.Deserialize<ConcurrentDictionary<ulong, string>>(reader);
                    }
                }
            }
            else
            {
                Console.WriteLine("COULDN'T FIND GUILD PREFIX!");
                throw new Exception("PREFIX");
            }
        }
        
        private void LoadStarboardChannels()
        {
            try
            {
                if (File.Exists($"{_dataDict}StarBoard.json"))
                {
                    using (StreamReader sr = File.OpenText($"{_dataDict}StarBoard.json"))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var temp =
                                jSerializer.Deserialize<ConcurrentDictionary<ulong, ulong>>(reader);
                            if (temp != null)
                                _starChannelDict = temp;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("COULDN'T FIND STARBOARD!");
                    throw new Exception("STARBOARD");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }*/
    }
}