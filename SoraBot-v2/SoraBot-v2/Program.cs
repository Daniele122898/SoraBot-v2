using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.Shapes;
using SoraBot_v2.Services;

namespace SoraBot_v2
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        #region Private Fields

        private DiscordSocketClient _client;
        private CommandHandler _commands;
        #endregion

        public async Task MainAsync(string[] args)
        {
            //setup discord client
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 75
            });

            _client.Log += Log;
            
            //Setup copfig
            
            //Setup Services
            
            //Instantiate the dependency map and add our services and client to it
            var serviceProvider = ConfigureServices();
            
            //setup command handler
            _commands = new CommandHandler(serviceProvider);
            await _commands.InstallAsync();
            
            //Set up an event handler to execute some state-reliant startup tasks
            /*_client.Ready += async () =>
            {

            };*/
            string token = "";
            ConfigService.GetConfig().TryGetValue("token2", out token);
            
            //Connect to Discord
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            //Hang indefinitely
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {    var services = new ServiceCollection();
            services.AddSingleton(_client);
            services.AddSingleton(new CommandService());

            return new DefaultServiceProviderFactory().CreateServiceProvider(services);
        }

        private Task Log(LogMessage m)
        {
            switch (m.Severity)
            {
                case LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogSeverity.Critical: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                case LogSeverity.Verbose: Console.ForegroundColor = ConsoleColor.White; break;
            }
            
            Console.WriteLine(m.ToString());
            if(m.Exception != null)
                Console.WriteLine(m.Exception);
            Console.ForegroundColor = ConsoleColor.Gray;

            return Task.CompletedTask;
        }
        
    }
}