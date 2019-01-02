using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace SoraBot_v2.Services
{
    public class UbService
    {
        public async Task GetUbDef(SocketCommandContext context, string urban)
        {
            try
            {
                using(var http = new HttpClient()){
                    var search = System.Net.WebUtility.UrlEncode(urban);
                    string req = await http.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + Uri.EscapeUriString(search));
                    var ub = JsonConvert.DeserializeObject<UbContainer>(req);
                    if (ub == null)
                    {
                        await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find Urban Dictionary entry.").Build());
                        return;
                    }
                    var eb = ub.GetEmbed();
                    eb.WithFooter(Utility.RequestedBy(context.User));
                    await context.Channel.SendMessageAsync("", embed: eb.Build());
                }
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find Urban Dictionary entry.").Build());
            }
        }
    }
    
    public class UbContainer
    {
        public UbDef[] list;

        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
                .WithColor(Utility.PurpleEmbed)
                .WithAuthor(x =>
                {
                    x.Name = "Urban Dictionary";
                    x.IconUrl =
                        ("https://lh5.ggpht.com/oJ67p2f1o35dzQQ9fVMdGRtA7jKQdxUFSQ7vYstyqTp-Xh-H5BAN4T5_abmev3kz55GH=w300"
                        );
                })
                .WithTitle($"Definition of {list[0].word}")
                .WithDescription(list[0].definition.Replace("[", "").Replace("]",""))
                .WithUrl((list[0].permalink))
                .AddField(x => x.WithName("Examples").WithValue(list[0].example.Replace("[", "").Replace("]","")).WithIsInline(false))
                .AddField(x => x.WithName("Author").WithValue(list[0].author).WithIsInline(true))
                .AddField(x =>
                    x.WithName("Stats").WithValue($"{list[0].thumbs_up} :thumbsup:\t{list[0].thumbs_down} :thumbsdown:")
                        .WithIsInline(true));
    }

    public class UbDef
    {
        public string definition { get; set; }
        public string example { get; set; }
        public string word { get; set; }
        public string author { get; set; }
        public string thumbs_up { get; set; }
        public string thumbs_down { get; set; }
        public string permalink { get; set; }
    }
}