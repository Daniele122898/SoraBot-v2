using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public static class SentryService
    {
        private static DiscordSocketClient _client;
        private static SocketUser _serenity;

        public static async Task Install(DiscordSocketClient client)
        {
            _client = client;
            _serenity = _client.GetUser(Utility.OWNER_ID);
            Console.WriteLine($"Got user {Utility.GiveUsernameDiscrimComb(_serenity)}");
        }

        public static async Task SendMessage(string message)
        {
            Console.WriteLine(message);
            string message1 = message;
            string message2 = "";
            string message3 = "";

            if (message.Length > 2000 && message.Length < 4000)
            {
                message1 = message.Remove(2000);
                message2 = message.Substring(2000);
            }
            else if (message.Length > 4000 && message.Length < 6000)
            {
                message1 = message.Remove(2000);
                message2 = message.Substring(2000, 2000);
                message3 = message.Substring(4000);
            }
            else
            {
                await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync("SOMETHING WENT BOOM AND I COULDN'T SEND IT");
                return;
            }
           
            await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync(message1);
            if (!string.IsNullOrWhiteSpace(message2))
                await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync(message2);
            if (!string.IsNullOrWhiteSpace(message3))
                await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync(message3);
        }

    }
}