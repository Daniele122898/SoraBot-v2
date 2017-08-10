using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class TagService
    {

        public async Task RemoveTag(SocketCommandContext context, SoraContext soraContext, string name, bool admin)
        {
            var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);
            //Check if tag exists
            var result = guildDb.Tags.FirstOrDefault(x => x.Name == name);
            if (result == null)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], $"Tag \"{name}\" could not be found!"));
                return;
            }
            //Check if creator or admin
            if (!admin && result.CreatorId != context.User.Id)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are neither Administrator nor the Creator of the Tag!"));
                return;
            }
            //delete Tag
            guildDb.Tags.Remove(result);
            soraContext.SaveChanges();
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Tag was successfully removed!"));
        }
        
        public async Task CreateTag(SocketCommandContext context, SoraContext soraContext, string name, string value, bool forceEmbed)
        {
            var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);
            //Check if guild restricted tag creation and if the user has the admin role!
            if (guildDb.RestrictTags)
            {
                if (Utility.CheckIfSoraAdminExists(context.Guild))
                {
                    var adminRole = (context.User as SocketGuildUser)?.Roles.FirstOrDefault(x=> x.Name == Utility.SORA_ADMIN_ROLE_NAME);
                    if (adminRole == null)
                    {
                        //HE MISSES THE ROLE SO HE CANT ADD SHIT :D
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You don't have the {Utility.SORA_ADMIN_ROLE_NAME} role and thus can't create Tags!"));
                        return;
                    }
                }
            }
            
            //Check if already exists
            if (guildDb.Tags.Any(x => x.Name == name))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "A tag with that name already exists in this guild!"));
                return;
            }
            
            //Check if contents are valid
            if (string.IsNullOrWhiteSpace(name))// || string.IsNullOrWhiteSpace(value)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "The Name cannot be Empty or White Space!"));
                return;
            }
            //Also take the attachment of the message if there is one
            string attachmentUrls="";
            bool attachMent = false;
            bool picAttachment = false;
            string picAttach = "";
            
            if (context.Message.Attachments.Count > 0)
            {
                attachMent = true;
                if (context.Message.Attachments.Count == 1)
                {
                    var url = context.Message.Attachments.ToArray()[0].Url;
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
                    foreach (var messageAttachment in context.Message.Attachments)
                    {
                        attachmentUrls += $"{messageAttachment.Url} \n";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!attachMent && !picAttachment)
                {
                    //NEEDS ATLEAST VALUE OR ATTACHMENT/PIC
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "The Value of a Tag cannot be empty! Either add text or an Attachment to the message!"));
                    return;
                }
            }
            
            //CHECK FOR 1 IMAGE WITHIN THE VALUE
            if (!attachMent && !picAttachment)
            {
                var mc = Regex.Matches(value, @"(https://[^ \s]+|http://[^ \s]+)([\s]|$)");
                if (mc.Count == 1)
                {
                    var link = mc[0].Value;
                    if (link.EndsWith(".png") || link.EndsWith(".jpg") || link.EndsWith(".gif"))
                    {
                        picAttachment = true;
                        picAttach = link;
                        value = value.Remove(value.IndexOf(link, StringComparison.Ordinal), link.Length);
                    }
                }
            }
            
            //If not ADD
            guildDb.Tags.Add(new Tags(){Name = name, Value = (attachMent ? $"{value}\n{attachmentUrls}":value??""), CreatorId = context.User.Id, PictureAttachment = picAttachment, AttachmentString = (picAttachment ? picAttach: ""), ForceEmbed = forceEmbed});
            soraContext.SaveChanges();
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Tag was successfully created!"));
        }

        public async Task FindAndDisplayTag(SocketCommandContext context, SoraContext soraContext, string name)
        {
            var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);

            var result = guildDb.Tags.FirstOrDefault(x => x.Name == name);
            if (result == null)
            {
                //didn't find the tag ;(
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], $"No tag was found with the name \"{name}\""));
                return;
            }
            //show tag
            //CHECK IF THERE IS A LINK TO BE EMBEDED 
            //if yes dissolve the embed

            if (!result.ForceEmbed && (result.Value.Contains("http://") || result.Value.Contains("https://")))
            {
                await context.Channel.SendMessageAsync(result.Value);
                return;
            }
            
            var creator = context.Client.GetUser(result.CreatorId);
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Description = result.Value,
            };
            if (creator != null)
            {
                eb.Author = new EmbedAuthorBuilder()
                {
                    IconUrl = creator.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Name = Utility.GiveUsernameDiscrimComb(creator)
                };
            }

            if (result.PictureAttachment)
            {
                eb.ImageUrl = result.AttachmentString;
            }

            await context.Channel.SendMessageAsync("", embed: eb);
        }
    }
}