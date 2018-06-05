using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class LyricsModule : InteractiveBase<SocketCommandContext>
    {
        [Command("lyrics", RunMode = RunMode.Async), Alias("searchlyrics"), Summary("Search lyrics")]
        public async Task SearchLyrics([Remainder] string search)
        {
            var query = System.Net.WebUtility.UrlEncode(search);
            string response = "";
            using (var client = new HttpClient())
            {
                response = await client.GetStringAsync($"https://api.genius.com/search?q={query.Trim()}&access_token={ConfigService.GetConfigData("geniusToken")}").ConfigureAwait(false);
            }
            var data = JsonConvert.DeserializeObject<LyricsData>(response);
            if (data.meta.status != 200 || data.response.hits.Count == 0)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to get Lyrics"));
                return;
            }
            Result res = null;
            // check if a song is in it and take first
            foreach (var hit in data.response.hits)
            {
                if (hit.type == "song")
                {
                    res = hit.result;
                    break;
                }
            }
            if (res == null || !res.lyrics_state.Equals("complete"))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to get Lyrics"));
                return;
            }
            // got a hit
            // start scraping website
            var web = new HtmlWeb();
            var html = await web.LoadFromWebAsync(res.url);
            var doc = html.DocumentNode;

            var lyrics = doc.QuerySelector(".lyrics");

            var text = lyrics.InnerText.Trim();

            // can be sent in ONE embed
            if (text.Length < 2048)
            {
                var eb = new EmbedBuilder()
                {
                    Title = res.full_title,
                    ThumbnailUrl = res.song_art_image_thumbnail_url,
                    Footer = Utility.RequestedBy(Context.User),
                    Description = text,
                    Color = Utility.PurpleEmbed
                };

                await Context.Channel.SendMessageAsync("", embed: eb);
            }
            else
            {
                List<string> lyricsList = new List<string>();
                int pageAmount = (int) Math.Ceiling(text.Length / 2048.0);
                int startIndex = 0;
                for (int i = 0; i < pageAmount; i++)
                {
                    var first = text.Substring(startIndex);
                    if (first.Length < 2048)
                    {
                        lyricsList.Add(first);
                        break;
                    }
                    if (i == (pageAmount - 1))
                    {
                        pageAmount++;
                    }
                    var copy = first.Remove(2048);
                    int lastBreak = copy.LastIndexOf("\n", StringComparison.Ordinal);
                    var page = copy.Substring(0, lastBreak);
                    startIndex = lastBreak;
                    lyricsList.Add(page);
                }
                
                var pmsg = new PaginatedMessage()
                {
                    Color = Utility.PurpleEmbed,
                    Title = res.full_title,
                    Options = new PaginatedAppearanceOptions()
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(30),
                        InfoTimeout = TimeSpan.FromSeconds(30),
                    },
                    Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                    Pages = lyricsList
                };
                await PagedReplyAsync(pmsg);
            }
        }
    }
}