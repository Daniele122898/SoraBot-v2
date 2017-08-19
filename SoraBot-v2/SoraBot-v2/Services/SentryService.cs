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
            _serenity = _client.GetUser(192750776005689344);
            Console.WriteLine($"Got user {Utility.GiveUsernameDiscrimComb(_serenity)}");
        }

        public static async Task SendMessage(string message)
        {
            string message1 = message;
            string message2 = "";
            if (message.Length > 2000)
            {
                message1 = message.Remove(2000);
                message2 = message.Substring(2000);
            }
            await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync(message1);
            if (!string.IsNullOrWhiteSpace(message2))
                await (await _serenity.GetOrCreateDMChannelAsync()).SendMessageAsync(message2);
        }

    }
}