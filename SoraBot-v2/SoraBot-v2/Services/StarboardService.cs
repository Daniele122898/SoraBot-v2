using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class StarboardService
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public StarboardService(DiscordSocketClient client)
        {
            _client = client;
        }
        
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
        }
        
        private readonly List<string> _acceptedStars = new List<string>()
        {
            "⭐", "🌟", "🌠"
        };
        
        public async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel socketMessageChannel, SocketReaction reaction)
        {
            try
            {
            //Reaction doesn't match a star
            if(!_acceptedStars.Contains(reaction.Emote.Name))
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
                //get Message
                var msg = await cacheable.GetOrDownloadAsync();
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
                    await PostStarMessage(starChannel, msg);
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

        private async Task PostStarMessage(SocketTextChannel starChannel, IUserMessage msg)
        {
            try
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
    
                await starChannel.SendMessageAsync($"⭐ in <#{msg.Channel.Id}> \n", embed: eb);
            }
            catch (Exception e)
            {
                await SentryService.SendMessage(e.ToString());
            }
            
        }
    }
}