using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using JikanDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoraBot_v2.Services
{
    public class AnimeSearchService
    {
        private readonly InteractiveService _interactive;
        private IJikan _jikna;

        public AnimeSearchService(InteractiveService interactiveService)
        {
            _interactive = interactiveService;
            _jikna = new Jikan(true);

        }
        
        public async Task GetChar(SocketCommandContext context, string search)
        {
            var res = await _jikna.SearchCharacter(search);
            if (res.Results.Count == 0)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2],
                    "I couldn't find anything :(").Build());
                return;
            }

            int count = 0;
            var eb = new EmbedBuilder()
            {
                Title = "Search Results",
                Footer = Utility.RequestedBy(context.User),
                Color = Utility.BlueInfoEmbed
            };
            string description = "Choose character by responding with corresponding index (number)\n";
            var resList = res.Results.ToList();
            // only show 10 entries
            foreach (var result in resList)
            {
                count++;
                int charNr = count;
                description += $"\n**{charNr}.** {result.Name}";
                if (count == 10) break;
            }
            // Set description
            eb.Description = description;
            // send message
            var msg = await context.Channel.SendMessageAsync("", embed: eb.Build());
            // wait for response
            var response = await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
            await msg.DeleteAsync();
            // handle response
            if (response == null)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(context.User)} did not reply :/").Build());
                return;
            }
            if (!Int32.TryParse(response.Content, out var index))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Only send the number corresponding to the index of the character!").Build());
                return;
            }
            if (index > count || index < 1)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Index!").Build());
                return;
            }
            // now search anime using the selected one
            Character mchar = await _jikna.GetCharacter(resList[index - 1].MalId);
            string desc = "No info found";
            if (!string.IsNullOrWhiteSpace(mchar.About))
            {
                desc = mchar.About;
                if (desc.Length > 1500)
                {
                    desc = desc.Remove(1500) + "...";
                }
            }
            var resEb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = GetMalAuthor(),
                Title = mchar.Name,
                Url = mchar.LinkCanonical,
                Description = desc,
            };

            if (!string.IsNullOrWhiteSpace(mchar.ImageURL))
                resEb.ImageUrl = mchar.ImageURL;
                
            if(mchar.Mangaography.Count != 0)
                resEb.AddField(x =>
                {
                    x.Name = "Mangaography 📚";
                    x.IsInline = true;
                    x.Value = $"{String.Join(", ", mchar.Mangaography.Distinct())}";
                });
            
            if(mchar.Animeography.Count != 0)
                resEb.AddField(x =>
                {
                    x.Name = "Animeography 📺";
                    x.IsInline = true;
                    x.Value = $"{String.Join(", ", mchar.Mangaography.Distinct())}";
                });
            
            if (mchar.MemberFavorites.HasValue)
                resEb.AddField(x =>
                {
                    x.Name = "Member Favorites ⭐";
                    x.IsInline = true;
                    x.Value = $"{mchar.MemberFavorites.Value}";
                });

            if (mchar.Nicknames.Count != 0)
                resEb.AddField(x =>
                {
                    x.Name = "Nicknames 🏷️";
                    x.IsInline = true;
                    x.Value = $"{String.Join(", ", mchar.Nicknames)}";
                });
                
            await context.Channel.SendMessageAsync("", embed: resEb.Build());
        }

        public async Task GetManga(SocketCommandContext context, string search)
        {
            var res = await _jikna.SearchManga(search);
            if (res.Results.Count == 0)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2],
                    "I couldn't find anything :(").Build());
                return;
            }

            int count = 0;
            var eb = new EmbedBuilder()
            {
                Title = "Search Results",
                Footer = Utility.RequestedBy(context.User),
                Color = Utility.BlueInfoEmbed
            };
            string description = "Choose manga by responding with corresponding index (number)\n";
            var resList = res.Results.ToList();
            // only show 10 entries
            foreach (var result in resList)
            {
                count++;
                int mangaNr = count;
                description += $"\n**{mangaNr}.** {result.Title} ({result.Type})";
                if (count == 10) break;
            }
            // Set description
            eb.Description = description;
            // send message
            var msg = await context.Channel.SendMessageAsync("", embed: eb.Build());
            // wait for response
            var response = await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
            await msg.DeleteAsync();
            // handle response
            if (response == null)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(context.User)} did not reply :/").Build());
                return;
            }
            if (!Int32.TryParse(response.Content, out var index))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Only send the number corresponding to the index of the manga!").Build());
                return;
            }
            if (index > count || index < 1)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Index!").Build());
                return;
            }
            // now search anime using the selected one
            Manga manga = await _jikna.GetManga(resList[index - 1].MalId);
            string desc = "No info found";
            if (!string.IsNullOrWhiteSpace(manga.Synopsis))
            {
                desc = manga.Synopsis;
                if (desc.Length > 1500)
                {
                    desc = desc.Remove(1500) + "...";
                }
            }
            var resEb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = GetMalAuthor(),
                Title = string.IsNullOrWhiteSpace(manga.TitleEnglish) ? manga.Title : manga.TitleEnglish,
                Url = manga.LinkCanonical,
                Description = desc,
            };

            if (!string.IsNullOrWhiteSpace(manga.ImageURL))
                resEb.ImageUrl = manga.ImageURL;
                
            if (!string.IsNullOrWhiteSpace(manga.Type))
                resEb.AddField(x =>
                {
                    x.Name = "Type 📚";
                    x.IsInline = true;
                    x.Value = $"{manga.Type}";
                });
            
            if (manga.Type.Equals("Manga", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(manga.Chapters) || !string.IsNullOrWhiteSpace(manga.Volumes))
                    resEb.AddField(x =>
                    {
                        x.Name = "Chapters / Volumes 🔢";
                        x.IsInline = true;
                        x.Value = $" {(string.IsNullOrWhiteSpace(manga.Chapters) ? "" : $"{manga.Chapters} Ch / ")}{(string.IsNullOrWhiteSpace(manga.Volumes) ? "" : $"{manga.Volumes} V")}";
                    });
                
                if (!string.IsNullOrWhiteSpace(manga.Status))
                    resEb.AddField(x =>
                    {
                        x.Name = "Status ⏯️";
                        x.IsInline = true;
                        x.Value = $"{manga.Status}";
                    });
            } 
            else if (manga.Type.Contains("Novel", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(manga.Volumes))
                    resEb.AddField(x =>
                    {
                        x.Name = "Volumes 🔢";
                        x.IsInline = true;
                        x.Value = $"{manga.Volumes}";
                    });
            }
            if (manga.Genres.Count != 0)
                resEb.AddField(x =>
                {
                    x.Name = "Genres 📁";
                    x.IsInline = true;
                    x.Value = $"{String.Join(", ", manga.Genres)}";
                });
            
            if (manga.Score.HasValue)
                resEb.AddField(x =>
                {
                    x.Name = "Score ⭐";
                    x.IsInline = true;
                    x.Value = $"{manga.Score.Value} / 10{(manga.ScoredBy.HasValue ? $" ({manga.ScoredBy.Value})" : "")}";
                });
            
            if (manga.Rank.HasValue) 
                resEb.AddField(x =>
                {
                    x.Name = "Rank 📈";
                    x.IsInline = true;
                    x.Value = $"#{manga.Rank.Value}";
                });

            await context.Channel.SendMessageAsync("", embed: resEb.Build());
        }

        public async Task GetAnime(SocketCommandContext context, string search)
        {
            var res = await _jikna.SearchAnime(search);
            if (res.Results.Count == 0)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2],
                    "I couldn't find anything :(").Build());
                return;
            }

            int count = 0;
            var eb = new EmbedBuilder()
            {
                Title = "Search Results",
                Footer = Utility.RequestedBy(context.User),
                Color = Utility.BlueInfoEmbed
            };
            string description = "Choose anime by responding with corresponding index (number)\n";
            var resList = res.Results.ToList();
            // only show 10 entries
            foreach (var result in resList)
            {
                count++;
                int animeNr = count;
                description += $"\n**{animeNr}.** {result.Title}";
                if (count == 10) break;
            }
            // Set description
            eb.Description = description;
            // send message
            var msg = await context.Channel.SendMessageAsync("", embed: eb.Build());
            // wait for response
            var response = await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
            await msg.DeleteAsync();
            // handle response
            if (response == null)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(context.User)} did not reply :/").Build());
                return;
            }
            if (!Int32.TryParse(response.Content, out var index))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Only send the number corresponding to the index of the anime!").Build());
                return;
            }
            if (index > count || index < 1)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Index!").Build());
                return;
            }
            // now search anime using the selected one
            Anime anime = await _jikna.GetAnime(resList[index - 1].MalId);
            string desc = "No info found";
            if (!string.IsNullOrWhiteSpace(anime.Synopsis))
            {
                desc = anime.Synopsis;
                if (desc.Length > 1500)
                {
                    desc = desc.Remove(1500) + "...";
                }
            }
            var resEb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = GetMalAuthor(),
                Title = string.IsNullOrWhiteSpace(anime.TitleEnglish) ? anime.Title : anime.TitleEnglish,
                Url = anime.LinkCanonical,
                Description = desc,
            };

            if (!string.IsNullOrWhiteSpace(anime.ImageURL))
                resEb.ImageUrl = anime.ImageURL;
                
            if (!string.IsNullOrWhiteSpace(anime.Type))
                resEb.AddField(x =>
                {
                    x.Name = "Type 📺";
                    x.IsInline = true;
                    x.Value = $"{anime.Type}";
                });
                
            if (anime.Type.Equals("TV", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(anime.Episodes))
                {
                    resEb.AddField(x =>
                    {
                        x.Name = "Episodes 🔢";
                        x.IsInline = true;
                        x.Value = $"{anime.Episodes}";
                    });
                }

                if (!string.IsNullOrWhiteSpace(anime.Status))
                {
                    resEb.AddField(x =>
                    {
                        x.Name = "Status ⏯️";
                        x.IsInline = true;
                        x.Value = $"{anime.Status}";
                    });
                }
            } 
            else if (anime.Type.Equals("movie", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(anime.Duration))
                {
                    resEb.AddField(x =>
                    {
                        x.Name = "Duration ⏳";
                        x.IsInline = true;
                        x.Value = $"{anime.Duration}";
                    });
                }
            }
            if (anime.Genres.Count != 0)
                resEb.AddField(x =>
                {
                    x.Name = "Genres 📁";
                    x.IsInline = true;
                    x.Value = $"{String.Join(", ", anime.Genres)}";
                });
            
            if (anime.Score.HasValue)
                resEb.AddField(x =>
                {
                    x.Name = "Score ⭐";
                    x.IsInline = true;
                    x.Value = $"{anime.Score.Value} / 10{(anime.ScoredBy.HasValue ? $" ({anime.ScoredBy.Value})" : "")}";
                });
            
            if (anime.Rank.HasValue) 
                resEb.AddField(x =>
                {
                    x.Name = "Rank 📈";
                    x.IsInline = true;
                    x.Value = $"#{anime.Rank.Value}";
                });

            await context.Channel.SendMessageAsync("", embed: resEb.Build());
        }

        private EmbedAuthorBuilder GetMalAuthor() => new EmbedAuthorBuilder()
        {
            Name = "MyAnimelist",
            IconUrl = "https://cdn.myanimelist.net/img/sp/icon/apple-touch-icon-256.png"
        };


    }
}