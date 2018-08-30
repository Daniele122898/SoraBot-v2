using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Weeb.net;
using Weeb.net.Data;
using TokenType = Weeb.net.TokenType;

namespace SoraBot_v2.Services
{
    public class WeebService
    {
        private readonly WeebClient _weebClient;
        private readonly string _token;


        public WeebService()
        {
            _token = ConfigService.GetConfigData("weebToken");
            _weebClient = new WeebClient("Sora", Utility.SORA_VERSION);
        }
                public async Task InitializeAsync()
        {
            try
            {
                await _weebClient.Authenticate(_token, TokenType.Bearer);

            }
            catch (Exception e)
            {
                await SentryService.SendMessage("COULND'T CONNECT TO WEEB.SH SERVICES");
            }
        }

        public async Task<TypesData> GetTypesRaw()
        {
            try
            {
                return await _weebClient.GetTypesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public async Task GetTypes(SocketCommandContext context)
        {
            var result = await _weebClient.GetTypesAsync();
            string types = "";
            foreach (var resultType in result.Types)
            {
                types += $"{resultType}, ";
            }
            var eb = new EmbedBuilder()
            {
                Description = $"These are all available Interactions:\n```\n{types}\n```",
                Color = Utility.PurpleEmbed,
                Title = "Available Types",
                Footer = Utility.RequestedBy(context.User),
                ThumbnailUrl = context.Client.CurrentUser.GetAvatarUrl()
            };
            await context.Channel.SendMessageAsync("", embed: eb.Build());
        }

        public async Task GetTags(SocketCommandContext context)
        {
            var result = await _weebClient.GetTagsAsync();
            string tags = "";
            foreach (var tag in result.Tags)
            {
                tags += $"{tag}, ";
            }
            await context.Channel.SendMessageAsync($"```\n{tags}\n```");
        }

        public async Task<RandomData> GetRandImage(string type, string[] tags, FileType fileType, NsfwSearch nsfw)
        {
            return await _weebClient.GetRandomAsync(type, tags, fileType, false, nsfw);
        }

        public async Task GetImages(SocketCommandContext context, string type, string[] tags)
        {
            var result = await _weebClient.GetRandomAsync(type, tags, FileType.Gif);

            if (result == null)
            {
                await context.Channel.SendMessageAsync("No image found with query.");
                return;
            }
            
            await context.Channel.SendMessageAsync($"Base type: {result.BaseType}\n" +
                                                   $"File type: {result.FileType}\n" +
                                                   $"{result.Url}");
        }

    }
}