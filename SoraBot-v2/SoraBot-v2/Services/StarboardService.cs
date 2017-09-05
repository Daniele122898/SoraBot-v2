using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class StarboardService
    {
        /*private readonly List<string> _acceptedStars = new List<string>()
        {
            "⭐", "🌟", "🌠"
        };*/
        
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private Timer _timer;
        
        
        public StarboardService(DiscordSocketClient client)
        {
            _client = client;
        }
        
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
            Task.Factory.StartNew(() => { _timer = new Timer(UpdateStarCounts, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(10)); });
        }

        private async void UpdateStarCounts(Object objectInfo)
        {
            try
            {
                using (SoraContext soraContext = _services.GetService<SoraContext>())
                {
                    //TODO BUILD CACHE TO MINIMIZE GET CALLS!
                    var starMessages = soraContext.StarMessages.ToList();
                    foreach (var starMessage in starMessages)
                    {
                        var guild = _client.GetGuild(starMessage.GuildForeignId);
                        if (guild == null)
                            continue;
                        var guildDb = Utility.GetOrCreateGuild(guild, soraContext);
                        if(guildDb.StarChannelId == 0)
                            continue;
                        var starChannel = guild.GetTextChannel(guildDb.StarChannelId);
                        if (starChannel == null)
                        {
                            guildDb.StarChannelId = 0;
                            continue;
                        }
                        var starMsg = (IUserMessage)await starChannel.GetMessageAsync(starMessage.PostedMsgId);
                        if (starMsg == null)
                        {
                            starMessage.IsPosted = false;
                            starMessage.StarCount = 0;
                            starMessage.HitZeroCount++;
                            continue;
                        }
                        int amount;
                        if(!int.TryParse(starMsg.Content.Substring(0,starMsg.Content.IndexOf(" ", StringComparison.Ordinal)).Replace("**", ""), out amount))
                            continue;
                        if(amount == starMessage.StarCount)
                            continue;
                        await starMsg.ModifyAsync(x =>
                        {
                            x.Content = $"**{starMessage.StarCount}**{starMsg.Content.Substring(starMsg.Content.IndexOf(" ", StringComparison.Ordinal))}";
                        });
                    }
                    await soraContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await SentryService.SendMessage(e.ToString());
            }
        }
        
        public async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel socketMessageChannel, SocketReaction reaction)
        {
            try
            {
                //Reaction doesn't match a star
                if(!reaction.Emote.Name.Equals("⭐"))
                    return;
                //get Message
                var msg = await cacheable.GetOrDownloadAsync();
                //Dont do anything if the msg originates from a bot
                if(msg.Author.IsBot)
                    return;
                //Reaction was a star
                using (SoraContext soraContext = _services.GetService<SoraContext>())
                {
                    var guild = ((SocketGuildChannel) socketMessageChannel).Guild;   
                    var guildDb = Utility.GetOrCreateGuild(guild, soraContext);
                    //Either the starboard wasn't set up or the channel doesnt exist anymore.
                    if(guildDb.StarChannelId == 0)
                        return;
                    var starChannel = guild.GetTextChannel(guildDb.StarChannelId);
                    if (starChannel == null)
                    {
                        guildDb.StarChannelId = 0; //Reset the channelID to 0 so in the future we dont have to save anything anymore :D
                        await soraContext.SaveChangesAsync();
                        return;
                    }
                    //Check if reaction is from author
                    if(msg.Author.Id == reaction.UserId)
                        return;
                    //check if it was added once before and if it was added too many times!
                    var starMsg = guildDb.StarMessages.FirstOrDefault(x=> x.MessageId == msg.Id);
                    if(starMsg != null &&  starMsg.HitZeroCount >=3)
                    {
                        return;
                    }
                    //if it was null create a new one otherwise keep the old one
                    bool wasNull = false;
                    if (starMsg == null)
                    {
                        starMsg = new StarMessage()
                        {
                            GuildForeignId = guild.Id,
                            HitZeroCount = 0,
                            MessageId = msg.Id,
                            StarCount = 0,
                            IsPosted = false
                        };
                        wasNull = true;
                    }
                   
                    //Add star
                    starMsg.StarCount++;
                    //Check if its enough to post
                    if (starMsg.StarCount >= guildDb.StarMinimum && !starMsg.IsPosted)
                    {
                        //POST
                        starMsg.PostedMsgId =  await PostStarMessage(starChannel, msg);
                        if (starMsg.PostedMsgId == 0)
                        {
                            await socketMessageChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                                Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Something failed. Can't add msg to starboard. Serenity#0783 has been notified"));
                            return;
                        }
                        starMsg.IsPosted = true;
                    }
                    //save changes made
                    if(wasNull)
                        guildDb.StarMessages.Add(starMsg);
                    await soraContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await SentryService.SendMessage(e.ToString());
            }
        }
        
        
        public async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel socketMessageChannel, SocketReaction reaction)
        {
            //Reaction doesn't match a star
            if(!reaction.Emote.Name.Equals("⭐"))
                return;
            //get Message
            var msg = await cacheable.GetOrDownloadAsync();
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guild = ((SocketGuildChannel) socketMessageChannel).Guild;
                var guildDb = Utility.GetOrCreateGuild(guild, soraContext);
                //Either the starboard wasn't set up or the channel doesnt exist anymore.
                if (guildDb.StarChannelId == 0)
                    return;
                var starChannel = guild.GetTextChannel(guildDb.StarChannelId);
                if (starChannel == null)
                {
                    guildDb.StarChannelId =0; //Reset the channelID to 0 so in the future we dont have to save anything anymore :D
                    await soraContext.SaveChangesAsync();
                    return;
                }
                //Check if reaction is from author
                if (msg.Author.Id == reaction.UserId)
                    return;
                //check if the starmessage exists in the DB
                var starMsg = guildDb.StarMessages.FirstOrDefault(x => x.MessageId == msg.Id);
                if (starMsg == null)
                    return;
                //Reduce starcount
                starMsg.StarCount--;
                //starcount hit 0 or went below magically :thonk: => DELETE
                if (starMsg.StarCount < 1)
                {
                    starMsg.HitZeroCount++; //If it reaches 3 or beyond it wont get added anymore.

                    var postedStarMsg = await starChannel.GetMessageAsync(starMsg.PostedMsgId);
                    //delete the msg if not null otherwise proceed as if we deleted it :P
                    if (postedStarMsg != null)
                        await postedStarMsg.DeleteAsync();
                    //make sure to reset is posted so it can get posted again
                    starMsg.IsPosted = false;
                }
                await soraContext.SaveChangesAsync();
            }
        }

        private async Task<ulong> PostStarMessage(SocketTextChannel starChannel, IUserMessage msg)
        {

            string attachmentUrls = "";
            bool attachMent = false;
            bool picAttachment = false;
            string picAttach = "";

            if (msg.Attachments.Count > 0)
            {
                attachMent = true;
                if (msg.Attachments.Count == 1)
                {
                    var url = msg.Attachments.ToArray()[0].Url;
                    if (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".gif"))
                    {
                        attachMent = false;
                        picAttachment = true;
                        picAttach = url;
                    }
                    else
                    {
                        attachmentUrls = url;
                    }
                }
                else
                {
                    foreach (var messageAttachment in msg.Attachments)
                    {
                        attachmentUrls += $"{messageAttachment.Url} \n";
                    }
                }
            }
            string messageContent = msg.Content ?? "";
            //CHECK FOR 1 IMAGE WITHIN THE VALUE
            if (!attachMent && !picAttachment)
            {
                var mc = Regex.Matches(messageContent, @"(https://[^ \s]+|http://[^ \s]+)([\s]|$)");
                if (mc.Count == 1)
                {
                    var link = mc[0].Value;
                    if (link.EndsWith(".png") || link.EndsWith(".jpg") || link.EndsWith(".gif"))
                    {
                        picAttachment = true;
                        picAttach = link;
                        messageContent = messageContent.Remove(messageContent.IndexOf(link, StringComparison.Ordinal), link.Length);
                    }
                }
            }
            //Finally ADD
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = msg.Author.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Name = Utility.GiveUsernameDiscrimComb(msg.Author as SocketUser)
                },
                Timestamp = DateTime.Now,
                Description = (attachMent ? $"{messageContent}\n{attachmentUrls}": messageContent)
            };
            if (picAttachment)
            {
                eb.ImageUrl = picAttach;
            }
            try
            {
                var postedMsg = await starChannel.SendMessageAsync($"**1** ⭐ in <#{msg.Channel.Id}> \n", embed: eb);
                return postedMsg.Id;
            }
            catch (Exception e)
            {
                await SentryService.SendMessage(e.ToString());
            }
            return 0;
        }
    }
}