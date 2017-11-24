using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class GiphyService
    {
        public async Task GetGifBySearch(SocketCommandContext context, string search)
        {
            var query = System.Net.WebUtility.UrlEncode(search);
            using (var http = new HttpClient())
            {
                var response = await http.GetStringAsync($"http://api.giphy.com/v1/gifs/search?api_key=dc6zaTOxFJmzC&q={Uri.EscapeUriString(query)}").ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<GiphyData>(response);
                var r = new Random();
                if (data.data.Count == 0)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Couldn't find any Gifs <.<"));
                    return;
                }
                var randomData = data.data[r.Next(data.data.Count)];
                int index = randomData.images.original.url.IndexOf('?');
                string url = (index == -1 ? randomData.images.original.url:randomData.images.original.url.Remove(index));
                var eb = new EmbedBuilder()
                {
                    Color = Utility.PurpleEmbed,
                    ImageUrl = url
                };
                await context.Channel.SendMessageAsync("", embed: eb);
            }
        }
    }
}