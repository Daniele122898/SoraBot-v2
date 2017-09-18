using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class MusicShareService
    {
        private InteractiveService _interactive;
        private Discord.Addons.InteractiveCommands.InteractiveService _interactiveCommands;
        private IServiceProvider _services;

        private const int MIN_LEVEL = 7;
        private const int NEED_FOR_EXTRA_PLAYLIST = 2;
        
        public MusicShareService(InteractiveService interactiveService, Discord.Addons.InteractiveCommands.InteractiveService interactiveCommands)
        {
            _interactive = interactiveService;
            _interactiveCommands = interactiveCommands;
        }

        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
        }
        
        //using (var soraContext = _services.GetService<SoraContext>())

        public struct SearchStruct
        {
            public ShareCentral SharedPlaylist;
            public int Matches;
        }

        public async Task SearchPlaylistsByName(SocketCommandContext context, string name)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var resultsQueryable = from soraContextShareCentral in soraContext.ShareCentrals
                    where EF.Functions.Like(soraContextShareCentral.Titel, $"%{name}%") && soraContextShareCentral.IsPrivate == false
                    select soraContextShareCentral;

                var results = resultsQueryable.ToList();
                
                if (results.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Nothing found with entered Title").WithDescription("You can use SQL to search => Where you don't know what to write add `%`"));
                    return;  
                }

                var orderedList = results.OrderByDescending(x=> (x.Upvotes- x.Downvotes)).ToList();
                
                await PaginateResult(context, "🔍 Search Results", orderedList);
            }
        }

        public async Task GetAllPlaylists(SocketCommandContext context)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var orderedList = soraContext.ShareCentrals.Where(x=> x.IsPrivate == false).OrderByDescending(x => (x.Upvotes - x.Downvotes)).ToList();
                if (orderedList.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "There are no shared playlists yet! Add one!"));
                    return;  
                }

                await PaginateResult(context, "Best Playlists by voting", orderedList);
            }
        }

        private async Task PaginateResult(SocketCommandContext context, string title, List<ShareCentral> orderedList)
        {
            List<string> playlistsString = new List<string>();
                int pageAmount = (int) Math.Ceiling(orderedList.Count/7.0);
                int addToJ = 0;
                int amountLeft = orderedList.Count;
                for (int i = 0; i < pageAmount; i++)
                {
                    string addToList = "";
                    for (int j = 0; j < (amountLeft > 7? 7: amountLeft); j++)
                    {
                        var sharedPlaylist = orderedList[j + addToJ];
                        addToList += $"{(sharedPlaylist.IsPrivate ? "[P] " : "")}**[{sharedPlaylist.Titel}]({sharedPlaylist.ShareLink})**\nVotes: {sharedPlaylist.Upvotes} / {sharedPlaylist.Downvotes}  \tTags: {sharedPlaylist.Tags.Replace(";", " - ")}\n\n";
                    }
                    playlistsString.Add(addToList);
                    amountLeft -= 7;
                    addToJ += 7;
                }
                if (pageAmount > 1)
                {
                    var pmsg = new PaginatedMessage()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = title,
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(60),
                            InfoTimeout = TimeSpan.FromSeconds(60)
                        },
                        Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                        Pages = playlistsString
                    };
                    
                    Criteria<SocketReaction> criteria = new Criteria<SocketReaction>();
                    criteria.AddCriterion(new EnsureReactionFromSourceUserCriterionMod());

                    await _interactive.SendPaginatedMessageAsync(context, pmsg, criteria);
                }
                else
                {
                    var eb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = title,
                        Description = playlistsString[0],
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "Page 1/1"
                        }
                    };
                    await context.Channel.SendMessageAsync("", embed: eb);
                }
        }

        public async Task ShowAllMySharedPlaylists(SocketCommandContext context)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null || userDb.ShareCentrals.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You have no shared playlists!"));
                    return;
                }

                var orderedList = userDb.ShareCentrals.OrderByDescending(x => (x.Upvotes - x.Downvotes)).ToList();
                await PaginateResult(context, "Your Shared Playlists ordered by vote", orderedList);
            }
        }

        public async Task RemovePlaylist(SocketCommandContext context, string url)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null || userDb.ShareCentrals.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You have no shared playlists!"));
                    return;
                }

                var result = userDb.ShareCentrals.FirstOrDefault(x => x.ShareLink == url);
                if (result == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "URL not found in your shared playlists"));
                    return;
                }

                userDb.ShareCentrals.Remove(result);
                var votes = soraContext.Votings.Where(x => x.ShareLink == result.ShareLink).ToList();
                foreach (var voting in votes)
                {
                    soraContext.Votings.Remove(voting);
                }
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed:
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "Successfully removed playlist"));
        }

        public async Task VotePlaylist(SocketCommandContext context, string url, bool vote)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null || EpService.CalculateLevel(userDb.Exp) < MIN_LEVEL)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You need to be at least lvl {MIN_LEVEL} to vote on playlists!"));
                    return;
                }

                var playlistDb = soraContext.ShareCentrals.FirstOrDefault(x => x.ShareLink == url);
                
                if (playlistDb == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "There is no shared playlist with that Url!"));
                    return;
                }
                
                //First check if it was ever voted on
                var voteDb = soraContext.Votings.FirstOrDefault(x => x.ShareLink == url && x.VoterId == context.User.Id);
                if (voteDb == null)
                {

                    userDb.Votings.Add(new Voting()
                    {
                        ShareLink = playlistDb.ShareLink,
                        UpOrDown = vote,
                        VoterId = context.User.Id
                    });

                    if (vote) //UPVOTED
                    {
                        playlistDb.Upvotes++;
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                                "Successfully UPVOTED playlist"));
                    }
                    else //DOWNVOTED
                    {
                        playlistDb.Downvotes++;
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                                "Successfully DOWNVOTED playlist"));
                    }

                    await soraContext.SaveChangesAsync();
                }
                else
                {
                    if (voteDb.UpOrDown == vote)
                    {
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                "You already voted this playlist with this vote! You can change your vote though!"));
                        return;
                    }
                    voteDb.UpOrDown = vote;
                    if (vote) //UPVOTE
                    {
                        playlistDb.Downvotes--;
                        playlistDb.Upvotes++;
                        
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                                "Successfully updated vote to UPVOTED!"));
                    }
                    else//DOWNVOTE
                    {
                        playlistDb.Downvotes++;
                        playlistDb.Upvotes--;
                        
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                                "Successfully updated vote to DOWNVOTED!"));
                    }
                    await soraContext.SaveChangesAsync();
                }
            }
        }
        
        public async Task SearchPlaylistByTags(SocketCommandContext context, string tags)
        {
            string[] seperatedTags;
            if (tags.IndexOf(";", StringComparison.Ordinal) < 1)
            {
                seperatedTags = new[] {tags};
            }
            else
            {
                seperatedTags = tags.Split(";");
            }
            using (var soraContext = _services.GetService<SoraContext>())
            {
                List<SearchStruct> searchResult = new List<SearchStruct>();
                foreach (var playlist in soraContext.ShareCentrals.Where(x=> x.IsPrivate == false))
                {
                    int matches = 0;
                    string[] playlistTags = playlist.Tags.Split(";");
                    foreach (var tag in seperatedTags)
                    {
                        foreach (var playlistTag in playlistTags)
                        {
                            if (tag.Trim().Equals(playlistTag.Trim(), StringComparison.OrdinalIgnoreCase))
                                matches++;
                        }
                    }
                    if (matches > 0)
                    {
                        searchResult.Add(new SearchStruct()
                        {
                            Matches = matches,
                            SharedPlaylist = playlist
                        });
                    }
                }

                if (searchResult.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Nothing found with entered tags"));
                    return;  
                }

                var orderedList = searchResult.OrderByDescending(x => x.Matches).ThenByDescending(x=> (x.SharedPlaylist.Upvotes - x.SharedPlaylist.Downvotes)).ToList(); 
                List<string> playlistsString = new List<string>();
                int pageAmount = (int) Math.Ceiling(orderedList.Count/7.0);
                int addToJ = 0;
                int amountLeft = orderedList.Count;
                for (int i = 0; i < pageAmount; i++)
                {
                    string addToList = "";
                    for (int j = 0; j < (amountLeft > 7? 7: amountLeft); j++)
                    {
                        var sharedPlaylist = orderedList[j + addToJ].SharedPlaylist;
                        addToList += $"**[{sharedPlaylist.Titel}]({sharedPlaylist.ShareLink})**\nVotes: {sharedPlaylist.Upvotes} / {sharedPlaylist.Downvotes}  \tTags: {sharedPlaylist.Tags.Replace(";", " - ")}\n\n";
                    }
                    playlistsString.Add(addToList);
                    amountLeft -= 7;
                    addToJ += 7;
                }
                if (pageAmount > 1)
                {
                    var pmsg = new PaginatedMessage()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = $"🔍 Search Results",
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(30),
                            InfoTimeout = TimeSpan.FromSeconds(30)
                        },
                        Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                        Pages = playlistsString
                    };
                    
                    Criteria<SocketReaction> criteria = new Criteria<SocketReaction>();
                    criteria.AddCriterion(new EnsureReactionFromSourceUserCriterionMod());

                    await _interactive.SendPaginatedMessageAsync(context, pmsg, criteria);
                }
                else
                {
                    var eb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = $"Search Results",
                        Description = playlistsString[0],
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "Page 1/1"
                        }
                    };
                    await context.Channel.SendMessageAsync("", embed: eb);
                }
            }
        }

        public async Task SetPrivate(SocketCommandContext context, string url)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if(userDb == null || userDb.ShareCentrals.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You have no playlists"));
                    return;  
                }
                var shareResult = userDb.ShareCentrals.FirstOrDefault(x => x.ShareLink == url);
                if (shareResult == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "No playlist found with that URL!"));
                    return;  
                }
                if (shareResult.IsPrivate)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Playlist already is set to PRIVATE"));
                    return;
                }
                
                shareResult.IsPrivate = true;
                
                await soraContext.SaveChangesAsync();

                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Playlist is now PRIVATE")); 
  
            }
        }

        public async Task SetPublic(SocketCommandContext context, string url)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if(userDb == null || userDb.ShareCentrals.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You have no playlists"));
                    return;  
                }
                var shareResult = userDb.ShareCentrals.FirstOrDefault(x => x.ShareLink == url);
                if (shareResult == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "No playlist found with that URL!"));
                    return;  
                }
                if (!shareResult.IsPrivate)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Playlist already is set to PUBLIC"));
                    return;
                }
                
                shareResult.IsPrivate = false;
                
                await soraContext.SaveChangesAsync();

                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Playlist is now PUBLIC")); 
            }
        }

       
        public async Task UpdateEntry(SocketCommandContext context, string shareUrl, string title, string tags)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if(userDb == null || userDb.ShareCentrals.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You have no playlists"));
                    return;  
                }
                var shareResult = userDb.ShareCentrals.FirstOrDefault(x => x.ShareLink == shareUrl);
                if (shareResult == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "No playlist found with that URL!"));
                    return;  
                }
                
                string[] seperatedTags;
                if (tags.IndexOf(";", StringComparison.Ordinal) < 1)
                {
                    seperatedTags = new[] {tags};
                }
                else
                {
                    seperatedTags = tags.Split(";");
                }
                List<string> betterTags = new List<string>();
                foreach (var tag in seperatedTags)
                {
                    string finalTag = tag;
                    if(finalTag.Contains(";"))
                        finalTag =finalTag.Replace(";", "");
                    if(!string.IsNullOrWhiteSpace(finalTag))
                        betterTags.Add(finalTag.Trim());
                }
                if (betterTags.Count < 1)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Add at least one Tag!").WithDescription("Tags must be added like this: `trap;edm;chill music;other`"));
                    return;
                }
                if (betterTags.Count > 10)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Please dont exceed 10 tags!"));
                    return;
                }
                
                string joinedTags = String.Join(";", betterTags);

                var eb = new EmbedBuilder()
                {
                    Color = Utility.BlueInfoEmbed,
                    Title = $"{Utility.SuccessLevelEmoji[3]} Are you sure you want Update this? y/n",
                    Description = $"{shareUrl}",
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                        Name = Utility.GiveUsernameDiscrimComb(context.User)
                    }
                };
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Title";
                    x.Value = title;
                });
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Tags";
                    x.Value = joinedTags.Replace(";", " - ");
                });
                var msg = await context.Channel.SendMessageAsync("", embed: eb);

                var response =
                    await _interactiveCommands.WaitForMessage(context.User, context.Channel, TimeSpan.FromSeconds(45));

                await msg.DeleteAsync();
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer in time ;_;"));
                    return;
                }

                if (response.Content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    response.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    shareResult.Tags = joinedTags;
                    shareResult.Titel = title;
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully updated playlist (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧"));
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer with y or yes! Discarded changes"));
                }
                
            }
        }

        public async Task<bool> CanAddNewPlaylist(SocketCommandContext context, User userDb)
        {
            if (context.User.Id == Utility.OWNER_ID)//backdoor so i can add as many as i want
                return true;
            
            int level = EpService.CalculateLevel(userDb.Exp);
            int amountGranted = (int) Math.Floor((double) ((level - (MIN_LEVEL - NEED_FOR_EXTRA_PLAYLIST)) / NEED_FOR_EXTRA_PLAYLIST));
            int amountLeft = amountGranted - userDb.ShareCentrals.Count;
            
            if (amountLeft > 0)
                return true;
            
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You reached your playlist limit. You gain another slot every 2 levels!"));
            return false;
        }

        
        public async Task SharePlaylist(SocketCommandContext context, string shareUrl, string title, string tags, bool isPrivate)
        {
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null || EpService.CalculateLevel(userDb.Exp) < MIN_LEVEL)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need to be at least lvl {MIN_LEVEL} to share playlists!"));
                    return;  
                }
                
                if (await CanAddNewPlaylist(context, userDb) == false)
                    return;
                    
                if (!shareUrl.StartsWith("https://hastebin.com/"))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "The link must be a valid hastebin link!"));
                    return;
                }
            
                if (!shareUrl.EndsWith(".sora") && !shareUrl.EndsWith(".fredboat"))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                "Must be an originaly exported sora or fredboat playlist!")
                            .WithDescription(
                                $"Use `{Utility.GetGuildPrefix(context.Guild, soraContext)}export` when you have a Queue! This is to minimize errors."));
                    return;
                }
                if (shareUrl.EndsWith(".fredboat"))
                {
                    shareUrl = shareUrl.Replace(".fredboat", ".sora");
                }

                if (soraContext.ShareCentrals.Any(x => x.ShareLink == shareUrl))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Playlist already exists!"));
                    return;
                }
                string[] seperatedTags;
                if (tags.IndexOf(";", StringComparison.Ordinal) < 1)
                {
                    seperatedTags = new[] {tags};
                }
                else
                {
                    seperatedTags = tags.Split(";");
                }
                List<string> betterTags = new List<string>();
                foreach (var tag in seperatedTags)
                {
                    string finalTag = tag;
                    if(finalTag.Contains(";"))
                        finalTag =finalTag.Replace(";", "");
                    if(!string.IsNullOrWhiteSpace(finalTag))
                        betterTags.Add(finalTag.Trim());
                }
                if (betterTags.Count < 1)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Add at least one Tag!").WithDescription("Tags must be added like this: `trap;edm;chill music;other`"));
                    return;
                }
                if (betterTags.Count > 10)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Please dont exceed 10 tags!"));
                    return;
                }
                string joinedTags = String.Join(";", betterTags);

                var eb = new EmbedBuilder()
                {
                    Color = Utility.BlueInfoEmbed,
                    Title = $"{Utility.SuccessLevelEmoji[3]} Are you sure you want share this? y/n",
                    Description = $"{shareUrl}",
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                        Name = Utility.GiveUsernameDiscrimComb(context.User)
                    }
                };
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Title";
                    x.Value = title;
                });
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Tags";
                    x.Value = joinedTags.Replace(";", " - ");
                });
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Is Private?";
                    x.Value = $"{(isPrivate ? "Yes": "No")}";
                });

                var msg = await context.Channel.SendMessageAsync("", embed: eb);

                var response =
                    await _interactiveCommands.WaitForMessage(context.User, context.Channel, TimeSpan.FromSeconds(45));

                await msg.DeleteAsync();
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer in time ;_;"));
                    return;
                }

                if (response.Content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    response.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    userDb.ShareCentrals.Add(new ShareCentral()
                    {
                        CreatorId = context.User.Id,
                        Downvotes = 0,
                        Upvotes = 0,
                        ShareLink = shareUrl,
                        Titel = title,
                        IsPrivate = isPrivate,
                        Tags = joinedTags
                    });
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully {(isPrivate ? "saved": "shared")} playlist (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧"));
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer with y or yes! Discarded changes"));
                }
            }
        }
    }
}