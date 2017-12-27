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

        //// Disabled by Catherine Renelle - Memory Leak Fix
        ////private string _connectionString;
        #endregion

        public async Task MainAsync(string[] args)
        {
            Console.WriteLine(args.Join(", "));
            int shardId;
            if (!int.TryParse(args[0], out shardId))
            {
                throw new Exception("INVALID SHARD ARGUMENT");
            }

            //Setup config
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();

            if (!int.TryParse(ConfigService.GetConfigData("shardCount"), out Utility.TOTAL_SHARDS))
            {
                throw new Exception("INVALID SHARD COUNT");
            }

            //setup discord client
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = false,
                MessageCacheSize = 0,
                TotalShards = Utility.TOTAL_SHARDS,
                ShardId = shardId
            });

            _client.Log += Log;



            //setup DB

            /****
             * Disabled by Catherine Renelle - Memory leak fix
            if (!ConfigService.GetConfig().TryGetValue("connectionString", out _connectionString))
            {
                throw new IOException
                {
                    Source = "COULDNT FIND CONNECTION STRING FOR DB!"
                };
            }****/

            Utility.SORA_VERSION = ConfigService.GetConfigData("version");


            //_soraContext = new SoraContext(_connectionString);
            //await _soraContext.Database.EnsureCreatedAsync();

            //Setup Services
            ProfileImageProcessing.Initialize();
            _interactive = new InteractiveService(_client);
            //Create dummy commandHandler for dependency Injection
            //_commands = new CommandHandler();
            //Instantiate the dependency map and add our services and client to it
            var serviceProvider = ConfigureServices();

            //setup command handler
            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync(serviceProvider);
            //_commands.ConfigureCommandHandler(serviceProvider);
            //await _commands.InstallAsync();

            //SETUP other dependency injection services
            serviceProvider.GetRequiredService<ReminderService>().Initialize();
            await serviceProvider.GetRequiredService<WeebService>().InitializeAsync();
            serviceProvider.GetRequiredService<StarboardService>().Initialize(); 
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

            //INITIALIZE CACHE
            CacheService.Initialize();

            //Hang indefinitely
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_client);
            services.AddSingleton<CommandService>();

            //// Disabled by Catherine Renelle - Memory leak fix
            ////services.AddDbContext<SoraContext>(options => options.UseMySql(_connectionString),ServiceLifetime.Transient);//, ServiceLifetime.Transient

            services.AddSingleton<CommandHandler>();
            services.AddSingleton(_interactive);
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
            services.AddSingleton<ImdbService>();
            services.AddSingleton<EpService>();
            services.AddSingleton<TagService>();
            services.AddSingleton<AnimeSearchService>();


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
            if (m.Exception != null)
                Console.WriteLine(m.Exception);
            Console.ForegroundColor = ConsoleColor.Gray;

            return Task.CompletedTask;
        }

    }
}