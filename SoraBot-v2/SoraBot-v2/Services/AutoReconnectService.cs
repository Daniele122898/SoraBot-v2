using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System;

namespace SoraBot_v2.Services
{
    public class AutoReconnectService
    {
        private DiscordSocketClient _client;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        public AutoReconnectService(DiscordSocketClient client)
        {
            _client = client;
            //start reconnection loop
            Task.Factory.StartNew(()=>{ AutoReconnect(); }, TaskCreationOptions.LongRunning);
        }

        private async Task AutoReconnect(){
            while(true){
                if(_client.ConnectionState == ConnectionState.Disconnected){
                    //this could happen while discord.net is still trying to auto reconnect. 
                    //Thus lets wait for a minute and see if this changed.
                    await Task.Delay(60000);
                    //Now recheck the condition
                    if(_client.ConnectionState != ConnectionState.Disconnected) { 
                        //To prevent further indention                        
                        continue;
                    }
                    //Start reconnection process
                    await Reconnect();
                }
                //wait with check again.
                await Task.Delay(60000);
            }
        }

        private async Task Reconnect(){
            //reconnect
            var timeout = Task.Delay(_timeout);
            var connect = _client.StartAsync();
            var task = await Task.WhenAny(timeout, connect);

             if (task == timeout){
                Console.WriteLine("Client reset timed out. Retrying...");
            }
            else if (connect.IsFaulted){
                Console.WriteLine("Client reset faulted. Retrying...", connect.Exception);
            }
            else if (connect.IsCompletedSuccessfully)
                Console.WriteLine("Client reset succesfully!");
        }

    }
}