using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class TagModule : InteractiveBase<SocketCommandContext>, IDisposable
    {
        private TagService _tagService;
        private SoraContext _soraContext;
        
        public TagModule(TagService tagService, SoraContext soraContext)
        {
            _tagService = tagService;
            _soraContext = soraContext;
        }
        
        [Command("taglist", RunMode = RunMode.Async), Alias("tl"), Summary("Shows all the tags that exist in the guild")]
        public async Task SearchTag()
        {
            try
            {
                var guildDb = Utility.GetOrCreateGuild(Context.Guild.Id, _soraContext);
                
                if (guildDb.Tags.Count < 1)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], "Your Guild has no Tags yet!"));
                    return;
                }
                
                List<string> tagList = new List<string>();
                int pageAmount = (int) Math.Ceiling(guildDb.Tags.Count /15.0);
                int addToJ = 0;
                int amountLeft = guildDb.Tags.Count;
                for (int i = 0; i < pageAmount; i++)
                {
                    string addToList = "";
                    for (int j = 0; j < (amountLeft > 15? 15: amountLeft); j++)
                    {
                        addToList += $"{guildDb.Tags[j+addToJ].Name}\n";
                    }
                    tagList.Add(addToList);
                    amountLeft -= 15;
                    addToJ += 15;
                }
                if (pageAmount > 1)
                {
                    var pmsg = new PaginatedMessage()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = Context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = $"Taglist of {Context.Guild.Name}",
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(30),
                            InfoTimeout = TimeSpan.FromSeconds(30),
                        },
                        Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                        Pages = tagList
                    };
                    await PagedReplyAsync(pmsg);
                }
                else
                {
                    var eb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = Context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = $"Taglist of {Context.Guild.Name}",
                        Description = tagList[0],
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "Page 1/1"
                        }
                    };
                    await Context.Channel.SendMessageAsync("", embed: eb);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        [Command("restricttag"), Alias("tagrestrict", "trestrict", "tr", "rt"),
         Summary("Restricts the Tagusage to Sora-Admin only!")]
        public async Task RestrictTagCreation()
        {
            var invoker = (SocketGuildUser)Context.User;
            if (!invoker.GuildPermissions.Has(GuildPermission.Administrator) && !Utility.IsSoraAdmin(invoker))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need Administrator permissions or the {Utility.SORA_ADMIN_ROLE_NAME} role to change these settings!"));
                return;
            }
            var guildDb = Utility.GetOrCreateGuild(Context.Guild.Id, _soraContext);
            //Check if the sora admin role even exists!
            if (!Utility.CheckIfSoraAdminExists(Context.Guild)&& !guildDb.RestrictTags)
            {
                await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"The {Utility.SORA_ADMIN_ROLE_NAME} Role does not exist! Please create it manually or use \"{Utility.GetGuildPrefix(Context.Guild, _soraContext)}createAdmin\""));
                return;
            }
            
            
            guildDb.RestrictTags = !guildDb.RestrictTags;
            await _soraContext.SaveChangesAsync();
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"{(guildDb.RestrictTags ? $"Set the Tag Restriction to TRUE!\n=> Users need the {Utility.SORA_ADMIN_ROLE_NAME} Role to create Tags":$"Set the Tag Restriction to FALSE!\n=> Everyone can create Tags")}"));
        }

        [Command("createtag"), Alias("addtag", "at", "ct"), Summary("Adds a new Tag to the taglist of your guild")]
        public async Task CreateTag([Remainder] string tag)
        {
            int index = tag.IndexOf('|');
            bool foundAttachment = false;
            if (index < 1)
            {
                if (Context.Message.Attachments.Count > 0)
                {
                    foundAttachment = true;
                }
                else
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Failed to add Tag! Make sure the format is \"tagName | tagValue\"").WithDescription("You can add attachments to your message which Sora will " +
                                                                                                                 "embed aswell!\n(if you add attachments you don't need to specify " +
                                                                                                                 "a value, you can still can tho.\nif you add no value you can simply " +
                                                                                                                 "leave the \"|\" away and only give the tag name)"));
                    return;
                }
            }
            bool forceEmbed = false;
            tag = tag.TrimStart();
            if (tag.StartsWith("-fe"))
            {
                forceEmbed = true;
                tag = tag.Replace("-fe", "");
                index = tag.IndexOf('|');
            }
            await _tagService.CreateTag(Context, _soraContext, (foundAttachment ? tag.ToLower().Trim(): tag.Remove(index).ToLower().Trim()), (foundAttachment ? "" : tag.Substring(index+1).Trim()), forceEmbed);
        }

        [Command("tag"), Alias("t"), Summary("Searches and displays the tag with the specified name")]
        public async Task SearchTag([Remainder] string name)
        {
            await _tagService.FindAndDisplayTag(Context, _soraContext, name.ToLower().Trim());
        }

        [Command("removetag"), Alias("deletetag", "dt", "rt"), Summary("Deletes the specified tag!")]
        public async Task RemoveTag([Remainder]string tag)
        {
            var user = (SocketGuildUser) Context.User;
            bool admin = (user.GuildPermissions.Has(GuildPermission.Administrator) || Utility.IsSoraAdmin(user));
            await _tagService.RemoveTag(Context, _soraContext, tag, admin);
        }

        public void Dispose()
        {
            _soraContext?.Dispose();
        }
    }
}