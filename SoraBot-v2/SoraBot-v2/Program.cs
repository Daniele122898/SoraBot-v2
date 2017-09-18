using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using Microsoft.EntityFrameworkCore;
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
        //private CommandHandler _commands;
        //private SoraContext _soraContext;
        private InteractiveService _interactive;
        private Discord.Addons.InteractiveCommands.InteractiveService _interactiveCommands;
        private string _connectionString;
        #endregion

        public async Task MainAsync(string[] args)
        {
            //setup discord client
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Warning            
            });

            _client.Log += Log;
            
            //Setup config
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();
            
            //setup DB
            if (!ConfigService.GetConfig().TryGetValue("connectionString", out _connectionString))
            {
                throw new IOException
                {
                    Source = "COULDNT FIND CONNECTION STRING FOR DB!"
                };
            }

            
            //_soraContext = new SoraContext(_connectionString);
            //await _soraContext.Database.EnsureCreatedAsync();
            
            //Setup Services
            ProfileImageProcessing.Initialize();
            _interactive = new InteractiveService(_client);
            _interactiveCommands = new Discord.Addons.InteractiveCommands.InteractiveService(_client);
            //Create dummy commandHandler for dependency Injection
            //_commands = new CommandHandler();
            //Instantiate the dependency map and add our services and client to it
            var serviceProvider = ConfigureServices();
            
            //setup command handler
            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync(serviceProvider);
            //_commands.ConfigureCommandHandler(serviceProvider);
            //await _commands.InstallAsync();
            
            //SETUP other dependency injection services
            serviceProvider.GetRequiredService<ReminderService>().Initialize(serviceProvider);
            serviceProvider.GetRequiredService<EpService>().Initialize(serviceProvider);
            serviceProvider.GetRequiredService<MarriageService>().Initialize(serviceProvider);
            serviceProvider.GetRequiredService<MusicShareService>().Initialize(serviceProvider);
            await serviceProvider.GetRequiredService<StarboardService>().InitializeAsync(serviceProvider);
            serviceProvider.GetRequiredService<SelfAssignableRolesService>().Initialize(serviceProvider);
            serviceProvider.GetRequiredService<AnnouncementService>().Initialize(serviceProvider);
            serviceProvider.GetRequiredService<ModService>().Initialize(serviceProvider);
            await serviceProvider.GetRequiredService<WeebService>().InitializeAsync();
            serviceProvider.GetRequiredService<RatelimitingService>().SetTimer();

            
            //Set up an event handler to execute some state-reliant startup tasks
            _client.Ready += async () =>
            {
                SentryService.Install(_client);
            };
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
            services.AddSingleton<CommandService>();
            services.AddDbContext<SoraContext>(options => options.UseMySql(_connectionString),ServiceLifetime.Transient);//, ServiceLifetime.Transient
            services.AddSingleton<CommandHandler>();
            services.AddSingleton(_interactive);
            services.AddSingleton(_interactiveCommands);
            services.AddSingleton<InteractionsService>();
            services.AddSingleton<AfkService>();
            services.AddSingleton<DynamicPrefixService>();
            services.AddSingleton<RatelimitingService>();
            services.AddSingleton<MusicShareService>();
            services.AddSingleton<SelfAssignableRolesService>();
            services.AddSingleton<WeatherService>();
            services.AddSingleton<AnnouncementService>();
            services.AddSingleton<MarriageService>();
            services.AddSingleton<StarboardService>();
            services.AddSingleton<GiphyService>();
            services.AddSingleton<WeebService>();
            services.AddSingleton<ModService>();
            services.AddSingleton<ReminderService>();
            services.AddSingleton<GuildCountUpdaterService>();
            services.AddSingleton<UbService>();
            services.AddSingleton(new ImdbService(_interactiveCommands));
            services.AddSingleton<EpService>();
            services.AddSingleton<TagService>();
            services.AddSingleton(new AnimeSearchService(_interactiveCommands));

            
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