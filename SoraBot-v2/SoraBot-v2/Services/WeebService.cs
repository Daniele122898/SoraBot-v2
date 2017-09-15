using System.Threading.Tasks;
using Discord.Commands;
using Weeb.net;

namespace SoraBot_v2.Services
{
    public class WeebService
    {
        private readonly WeebClient _weebClient;
        private readonly string _token;

        public WeebService()
        {
            _token = ConfigService.GetConfigData("weebToken");
            _weebClient = new WeebClient();
        }
        
        public async Task InitializeAsync()
        {
            await _weebClient.Authenticate(_token);
        }

        public async Task GetTypes(SocketCommandContext context)
        {
            var result = await _weebClient.GetTypesAsync();
            string types = "";
            foreach (var resultType in result.Types)
            {
                types += $"{resultType}, ";
            }
            await context.Channel.SendMessageAsync($"```\n{types}\n```");
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

        public async Task GetImages(SocketCommandContext context, string type, string[] tags)
        {
            var result = await _weebClient.GetRandomAsync(type, tags);

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