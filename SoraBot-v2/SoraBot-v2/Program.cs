using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq.Parsing;
using SixLabors.Shapes;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        #region Private Fields

        private DiscordSocketClient _client;
        private CommandHandler _commands;
        private SoraContext _soraContext;
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
            
            //Setup config
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();
            
            //setup DB
            string connectionString;
            if (!ConfigService.GetConfig().TryGetValue("connectionString", out connectionString))
            {
                throw new IOException
                {
                    Source = "COULDNT FIND CONNECTION STRING FOR DB!"
                };
            }
            _soraContext = new SoraContext(connectionString);
            await _soraContext.Database.EnsureCreatedAsync();
            
            //Setup Services
            ProfileImageProcessing.Initialize();
            //Create dummy commandHandler for dependency Injection
            _commands = new CommandHandler();
            //Instantiate the dependency map and add our services and client to it
            var serviceProvider = ConfigureServices();
            
            //setup command handler
            _commands.ConfigureCommandHandler(serviceProvider);
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
            services.AddSingleton(_soraContext);
            services.AddSingleton(_commands);
            services.AddSingleton(new InteractionsService());
            services.AddSingleton(new AfkService());
            services.AddSingleton(new DynamicPrefixService());
            services.AddSingleton(new CommandService());
            services.AddSingleton(new EpService(_client, _soraContext));
            services.AddSingleton(new TagService());
            services.AddSingleton<InteractiveService>();
            
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