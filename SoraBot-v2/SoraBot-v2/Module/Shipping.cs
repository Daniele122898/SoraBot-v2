using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Extensions;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class Shipping : ModuleBase<SocketCommandContext>
    {
        private float multiplier = 100f / 18;
        //TODO MAKE MORE COMPLEX MAYBE ID "you could multiply both user id's and then divide them by 2 until the number is below 100"
        private async Task GetAvatar(SocketUser user)
        {
            Uri requestUri = new Uri(user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar);
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (Stream contentStream = await(await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream($"Shipping/{user.Id}Avatar.png", FileMode.Create, FileAccess.Write,
                    FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
                await contentStream.FlushAsync();
                contentStream.Dispose();
                await stream.FlushAsync();
                stream.Dispose();
            }

        }

        [Command("ship", RunMode = RunMode.Async), Summary("Ship two people")]
        public async Task Ship(SocketUser user1, SocketUser user2 = null)
        {
            try
            {
                user2 = user2 ?? Context.User;
                await GetAvatar(user1);
                await GetAvatar(user2);
                ProfileImageGeneration.GenerateShipping($"Shipping/{user1.Id}Avatar.png",
                    $"Shipping/{user2.Id}Avatar.png", $"Shipping/ship{user1.Id}{user2.Id}.png");

                int distance = LevenshteinDistance.Compute(Utility.GiveUsernameDiscrimComb(user1),
                    Utility.GiveUsernameDiscrimComb(user2));
            
                await Context.Channel.SendFileAsync($"Shipping/ship{user1.Id}{user2.Id}.png",$"💕 Probability: {Math.Round(Math.Min(100, distance*multiplier),2)}%");
            }
            catch (Exception e)
            {
                await SentryService.SendMessage(e.ToString());
            }
            if (File.Exists($"Shipping/{user1.Id}Avatar.png"))
            {
                File.Delete($"Shipping/{user1.Id}Avatar.png");
            }
            if (File.Exists($"Shipping/{user2.Id}Avatar.png"))
            {
                File.Delete($"Shipping/{user2.Id}Avatar.png");
            }
            if (File.Exists($"Shipping/ship{user1.Id}{user2.Id}.png"))
            {
                File.Delete($"Shipping/ship{user1.Id}{user2.Id}.png");
            }
            
        }
    }
}