using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Newtonsoft.Json;

namespace SoraBot_v2.Services
{
    public class ImdbService
    {
        private readonly InteractiveService _interactive;

        public ImdbService(InteractiveService interactiveService)
        {
            _interactive = interactiveService;
        }
        
        public async Task GetImdb(SocketCommandContext context, string target)
        {
            try
            {
                await context.Channel.TriggerTypingAsync();

                var movieSimple = await TheMovieDbProvider.FindMovie(target);
                
                if(movieSimple == null || movieSimple.Length < 1)
                {
                    await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find movie/series"));
                    return;
                }

                int index;
                
                if (movieSimple.Length > 1)
                {
                    string choose = "";
                    var ebC = new EmbedBuilder()
                    {
                        Color = Utility.PurpleEmbed,
                        Title = "Enter the Index of the Movie you want more info about."
                    };
                    int count = 1;
                    foreach (var movie in movieSimple)
                    {   
                        choose += $"**{count}.** {movie.title} ({(movie.release_date.Length > 4 ?  movie.release_date.Remove(4) : (string.IsNullOrWhiteSpace(movie.release_date)? "NoDate": movie.release_date))})\n";
                        count++;
                    }
                    ebC.Description = choose;
                    var msg = await context.Channel.SendMessageAsync("", embed: ebC);
                    var response =
                        await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
                    await msg.DeleteAsync();
                    if (response == null)
                    {
                        await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(context.User)} didn't answer in time (≧д≦ヾ)"));
                        return;
                    }
                    if (!Int32.TryParse(response.Content, out index))
                    {
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Only send the Index!"));
                        return;
                    }
                    if (index > (movieSimple.Length) || index < 1)
                    {
                        await context.Channel.SendMessageAsync("", embed:
                            Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Invalid Index!"));
                        return;
                    }
                }
                else
                {
                    index = 1;
                }                
                var finalMovie = TheMovieDbProvider.FindFinalMovie(movieSimple[index-1].id.ToString()).Result;
                
                

                var eb = finalMovie.GetEmbed();
                eb.WithFooter(Utility.RequestedBy(context.User));
                await context.Channel.SendMessageAsync("", embed: eb);

            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find TheMovieDb entry."));
            }
        }
    }

    public static class TheMovieDbProvider
    {
        private const string simpleQuery = "https://api.themoviedb.org/3/search/movie?api_key={0}&query={1}";
        private const string precQuery = "https://api.themoviedb.org/3/movie/{0}?api_key={1}";
        private static string apiKey = "";

        static TheMovieDbProvider()
        {
            apiKey = ConfigService.GetConfigData("movieDB");
        }

        public static async Task<Results[]> FindMovie(string name)
        {
            using (var http = new HttpClient())
            {
                var search = System.Net.WebUtility.UrlEncode(name);

                
                
                var res = await http.GetStringAsync(String.Format(simpleQuery, apiKey, search)).ConfigureAwait(false);
                var tempMovie = JsonConvert.DeserializeObject<TheMovieDbTempData>(res);

                return tempMovie.results;
            }
        }

        public static async Task<TheMovieDb> FindFinalMovie(string ID)
        {
            using (var http = new HttpClient())
            {   
                
                var res = await http.GetStringAsync(String.Format(precQuery, ID, apiKey)).ConfigureAwait(false);
                
                var movie = JsonConvert.DeserializeObject<TheMovieDb>(res);

                return movie;
            }
        }
    }

    public class TheMovieDbTempData
    {
        public int page { get; set; }
        public int total_results{ get; set; }
        public int total_pages { get; set; }
        public Results[] results{ get; set; }
    }

    public class Results
    {
        public int id { get; set; }
        public string title { get; set; }
        public string release_date { get; set; }
    }

    public class TheMovieDb
    {
        public string budget { get; set; }
        public MovieGenres[] genres { get; set; }
        public string imdb_id { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public string release_date { get; set; }
        public string title { get; set; }
        public string vote_average { get; set; }
        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(Utility.PurpleEmbed)
            .WithAuthor(x => { x.Name = "TheMovieDb"; x.IconUrl =  ("http://i.imgur.com/odbmxoz.jpg"); })
            .WithTitle(title)
            .WithUrl( ($"http://www.imdb.com/title/{imdb_id}/"))
            .WithDescription(overview)
            .AddField(x => x.WithName("⭐ Rating").WithValue(vote_average).WithIsInline(true))
            .AddField(x => x.WithName("📁 Genre").WithValue(GetGenres().Remove(GetGenres().Length-2)).WithIsInline(true))
            .AddField(x => x.WithName("🗓️ Released").WithValue(release_date).WithIsInline(true))
            .AddField(x => x.WithName("💸 Budget").WithValue(budget+"$").WithIsInline(true))
            .WithImageUrl(($"https://image.tmdb.org/t/p/w500{poster_path}"));

        private string GetGenres()
        {
            string gnrs = "";
            var genreCount = 0;
            foreach (var genre in genres)
            {
                genreCount++;
                gnrs += $"{genre.name}, {(genreCount %3 == 0 ? "\n" : "")}";
            }
            return gnrs;
        }
    }

    public class MovieGenres
    {
        public int id { get; set; }
        public string name{ get; set; }
    }
}