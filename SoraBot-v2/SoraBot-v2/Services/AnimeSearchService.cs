using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using ImageSharp.ColorSpaces.Conversion.Implementation.Rgb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoraBot_v2.Services
{
    public class AnimeSearchService
    {
        private const string APIURL = "https://anilist.co/api/";
        private string _anilistToken;
        private DateTime _timeToRequestAgain;
        private string _clientId = "";
        private string _clientSecret = "";
        private readonly FormUrlEncodedContent _formContent;
        private Discord.Addons.InteractiveCommands.InteractiveService _interactive;
        
        public enum AnimeType
        {
            Anime, Manga, Char
        }
        
        public AnimeSearchService(Discord.Addons.InteractiveCommands.InteractiveService interactiveService)
        {
            _interactive = interactiveService;
            _timeToRequestAgain = DateTime.UtcNow;
            _clientId = ConfigService.GetConfigData("client_id");
            _clientSecret = ConfigService.GetConfigData("client_secret");
            if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
            {
                Console.WriteLine("FAILED AINILIST DATA");
            }
            
            var headers = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
            };
            _formContent = new FormUrlEncodedContent(headers);
            RequestAuth();
        }

        private async Task RequestAuth()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Clear();
                var response = await http.PostAsync("https://anilist.co/api/auth/access_token", _formContent)
                    .ConfigureAwait(false);
                var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _anilistToken = JObject.Parse(stringContent)["access_token"].ToString();
            }
            _timeToRequestAgain = DateTime.UtcNow.AddMinutes(59);
            Console.WriteLine("ANILIST AUTH DONE");
        }

        private async Task CheckAndUpdateAuth()
        {
            if (DateTime.UtcNow.CompareTo(_timeToRequestAgain) == 1)
            {
                await RequestAuth();
            }
        }

        public async Task GetInfo(SocketCommandContext context, string searchStr, AnimeType type)
        {
            await CheckAndUpdateAuth();
            var search = System.Net.WebUtility.UrlEncode(searchStr);
            string link = "";
            switch (type)
            {
                case AnimeType.Anime:
                    link = $"{APIURL}anime/";
                    break;
                case AnimeType.Char:
                    link = $"{APIURL}character/";
                    break;
                case AnimeType.Manga:
                    link = $"{APIURL}manga/";
                    break;
            }
            try
            {
                using (var http = new HttpClient())
                {
                    var resultString = await http.GetStringAsync($"{link}search/{Uri.EscapeUriString(search)}?access_token={_anilistToken}").ConfigureAwait(false);
                    var results = JArray.Parse(resultString);
                    if (!results.HasValues || results.Count == 0)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "I couldn't find anything. Sorry"));
                        return;
                    }
                    int index;
                    if (results.Count > 1)
                    {
                        string choose = "";
                        var ebC = new EmbedBuilder()
                        {
                            Color = Utility.PurpleEmbed,
                            Title = "Just enter the index for more info"
                        };
                        for (int i = 0; i < results.Count; i++)
                        {
                            choose += $"**{i+1}.** {((type == AnimeType.Anime || type == AnimeType.Manga) ? results[i]["title_english"] : $"{results[i]["name_first"]} {results[i]["name_last"]}")}\n";
                        }
                        ebC.Description = choose;
                        var msg = await context.Channel.SendMessageAsync("", embed: ebC);
                        var response =
                            await _interactive.WaitForMessage(context.User, context.Channel, TimeSpan.FromSeconds(45));
                        await msg.DeleteAsync();
                        if (response == null)
                        {
                            await context.Channel.SendMessageAsync("",
                                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(context.User)} did not reply :/"));
                            return;
                        }
    
                        if (!Int32.TryParse(response.Content, out index))
                        {
                            await context.Channel.SendMessageAsync("",
                                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Only send the Index!"));
                            return;
                        }
                        if (index > (results.Count) || index < 1)
                        {
                            await context.Channel.SendMessageAsync("",
                                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Index!"));
                            return;
                        }
                    }
                    else
                    {
                        index = 1;
                    }
                    var tempObj = results[index - 1];
                    var aniData = await http.GetStringAsync($"{link}{tempObj["id"]}?access_token={_anilistToken}")
                        .ConfigureAwait(false);
                    switch (type)
                    {
                        case AnimeType.Anime:
                            var animeData = JsonConvert.DeserializeObject<AnimeResult>(aniData);
                            await context.Channel.SendMessageAsync("",
                                embed: animeData.GetEmbed().WithFooter(Utility.RequestedBy(context.User)));
                            break;
                        case AnimeType.Char:
                            var charData = JsonConvert.DeserializeObject<CharacterResult>(aniData);
                            await context.Channel.SendMessageAsync("",
                                embed: charData.GetEmbed().WithFooter(Utility.RequestedBy(context.User)));
                            break;
                        case AnimeType.Manga:
                            var mangaData = JsonConvert.DeserializeObject<MangaResult>(aniData);
                            await context.Channel.SendMessageAsync("",
                                embed: mangaData.GetEmbed().WithFooter(Utility.RequestedBy(context.User)));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find anything. Sorry"));
            }
        }

    }
    
    public class CharacterResult
    {
        public int id { get; set; }
        public string name_first { get; set; }
        public string name_last { get; set; }
        public string name_japanese { get; set; }
        public string name_alt { get; set; }
        public string info { get; set; }
        public bool favorite { get; set; }
        public string image_url_lge { get; set; }
        public string image_url_med { get; set; }
        public string Link => "http://anilist.co/character/" + id;
        public string Synopsis => info?.Substring(0, info.Length > 1900 ? 1900 : info.Length) + (info.Length > 1900 ? "..." : "");

        public EmbedBuilder GetEmbed()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Anilist",
                    IconUrl = "https://anilist.co/img/logo_al.png"
                }, 
                Title = $"{(string.IsNullOrWhiteSpace(name_last) ? "": $"{name_last}, ")}{name_first}",
                Url = Link,
                Description = $"{(String.IsNullOrWhiteSpace(Synopsis)? "No Info found!": "")}" + Synopsis.Replace("<br>", Environment.NewLine),
            };
            if (!string.IsNullOrWhiteSpace(image_url_lge))
            {
                eb.WithImageUrl(image_url_lge);
            }
            if (!String.IsNullOrWhiteSpace(name_japanese))
            {
                eb.AddField((x) =>
                {
                    x.Name = "Japanese Name";
                    x.IsInline = true;
                    x.Value = name_japanese;
                });
            }

            if (!String.IsNullOrWhiteSpace(name_alt))
            {
                eb.AddField((x) =>
                {
                    x.Name = "Alt Name";
                    x.IsInline = true;
                    x.Value = name_alt;
                });
            }
            return eb;
        }

    }

    public class MangaResult
    {
        public int id;
        public string publishing_status;
        public string image_url_lge;
        public string title_english;
        public int total_chapters;
        public int total_volumes;
        public string description;
        public string start_date;
        public string end_date;
        public string[] Genres;
        public string average_score;
        public string Link => "http://anilist.co/manga/" + id;
        public string Synopsis => description?.Substring(0, description.Length > 1900 ? 1900 : description.Length) + (description.Length > 1900 ? "..." : "");
        
        public EmbedBuilder GetEmbed()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Anilist",
                    IconUrl = "https://anilist.co/img/logo_al.png"
                }, 
                Title = $"{title_english}",
                Url = Link,
                Description = $"{(String.IsNullOrWhiteSpace(Synopsis)? "No Info found!": "")}" + Synopsis.Replace("<br>", Environment.NewLine),
            };
            if (!string.IsNullOrWhiteSpace(image_url_lge))
            {
                eb.WithImageUrl(image_url_lge);
            }
            if(!string.IsNullOrWhiteSpace(total_chapters.ToString()))
                eb.AddField(efb => efb.WithName("🔢 Chapters").WithValue(total_chapters == 0 ? "-" : total_chapters.ToString()).WithIsInline(true));
            if(!string.IsNullOrWhiteSpace(publishing_status))
                eb.AddField(efb => efb.WithName("📺 Status").WithValue(publishing_status).WithIsInline(true));
            if(!string.IsNullOrWhiteSpace(String.Join(", ", Genres)))
                eb.AddField(efb => efb.WithName("📁 Genres").WithValue(String.Join(", ", Genres).Remove(String.Join(", ", Genres).Length-2)).WithIsInline(true));
            eb.AddField(efb => efb.WithName("⭐ Score").WithValue((average_score ?? "-") + " / 100").WithIsInline(true));
            if(!string.IsNullOrWhiteSpace(start_date))
                eb.AddField(efb => efb.WithName("🗓️ Published").WithValue($"{start_date.Remove(10)} - {(String.IsNullOrWhiteSpace(end_date) ? "Ongoing" : $"{end_date.Remove(10)}")}").WithIsInline(true));
            
            return eb;
        }
        
       }

    public class AnimeResult
    {
        public int id;
        public string AiringStatus => airing_status.ToLowerInvariant();
        public string airing_status;
        public string title_english;
        public int total_episodes;
        public string description;
        public string image_url_lge;
        public string start_date;
        public string end_date;
        public string[] Genres;
        public string average_score;

        public string Link => "http://anilist.co/anime/" + id;
        public string Synopsis => description?.Substring(0, description.Length > 1900 ? 1900 : description.Length) + (description.Length > 1900 ? "..." : "");

        public EmbedBuilder GetEmbed()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Anilist",
                    IconUrl = "https://anilist.co/img/logo_al.png"
                },
                Title = $"{title_english}",
                Url = Link,
                Description = $"{(String.IsNullOrWhiteSpace(Synopsis) ? "No Info found!" : "")}" +
                              Synopsis.Replace("<br>", Environment.NewLine),
            };
            if (!string.IsNullOrWhiteSpace(image_url_lge))
            {
                eb.WithImageUrl(image_url_lge);
            }
            eb.AddField(efb => efb.WithName("🔢 Episodes").WithValue(total_episodes.ToString() ?? "-").WithIsInline(true));
            eb.AddField(efb => efb.WithName("📺 Status").WithValue(string.IsNullOrWhiteSpace(AiringStatus) ? "-" : AiringStatus.Humanize()).WithIsInline(true));
            eb.AddField(efb => efb.WithName("📁 Genres").WithValue(string.IsNullOrWhiteSpace(String.Join(", ", Genres)) ? "-" : String.Join(", ", Genres).Remove(String.Join(", ", Genres).Length-2)).WithIsInline(true));
            eb.AddField(efb => efb.WithName("⭐ Score").WithValue((average_score ?? "-") + " / 100").WithIsInline(true));
            if(!string.IsNullOrWhiteSpace(start_date))
                eb.AddField(efb => efb.WithName("🗓️ Aired").WithValue($"{start_date.Remove(10)} - {(String.IsNullOrWhiteSpace(end_date) ? "Ongoing": $"{end_date.Remove(10)}")}").WithIsInline(true));
            
            return eb;
        }
    }
}