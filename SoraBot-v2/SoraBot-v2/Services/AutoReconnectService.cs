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
        private string _token;

        public AutoReconnectService(DiscordSocketClient client, string token)
        {
            _client = client;
            _token = token;
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
            var connect = _client.LoginAsync(TokenType.Bot, _token);
            var task = await Task.WhenAny(timeout, connect);

             if (task == timeout){
                Console.WriteLine("Client reset timed out. Retrying...");
            }
            else if (connect.IsFaulted){
                Console.WriteLine("Client login failed. Retrying...", connect.Exception);
            }
            else if (connect.IsCompletedSuccessfully){
                //run start
                try{
                    await _client.StartAsync();
                } catch(Exception e){
                    Console.WriteLine("Client start failed. Retrying...\n"+e.ToString());
                    return;
                }
                Console.WriteLine("Reconnect Successfull...");
            }

        }

    }
}