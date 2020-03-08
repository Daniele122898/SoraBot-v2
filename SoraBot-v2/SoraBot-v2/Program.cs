using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Services;
using Victoria;

namespace SoraBot_v2
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        #region Private Fields

        private DiscordSocketClient _client;
        private InteractiveService _interactive;
        private AutoReconnectService _autoReconnectService;
        private BanService _banService;
        private DiscordRestClient _restClient;
        
        //// Disabled by Catherine Renelle - Memory Leak Fix
        ////private string _connectionString;
        #endregion

        public async Task MainAsync(string[] args)
        {
            Console.WriteLine(args.Join());
            int shardId;
            if (!int.TryParse(args[0], out shardId))
            {
                throw new Exception("INVALID SHARD ARGUMENT");
            }
            
            _restClient = new DiscordRestClient();

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

            Utility.SHARD_ID = shardId;

            _client.Log += Log;
            
            string token;
            ConfigService.GetConfig().TryGetValue("token2", out token);

            await _restClient.LoginAsync(TokenType.Bot, token);
            Console.WriteLine($"CONNECTED REST CLIENT {_restClient.LoginState}");

            //setup DB

            Utility.SORA_VERSION = ConfigService.GetConfigData("version");

            // setup banservice
            _banService = new BanService();
            
            //Setup Services
            ProfileImageGeneration.Initialize();
            _interactive = new InteractiveService(_client);
            //Instantiate the dependency map and add our services and client to it
            var serviceProvider = ConfigureServices();
            
            // first setup weebservice
            await serviceProvider.GetRequiredService<WeebService>().InitializeAsync();

            //setup command handler
            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync(serviceProvider);

            //SETUP other dependency injection services
            serviceProvider.GetRequiredService<ReminderService>().Initialize();
            serviceProvider.GetRequiredService<WaifuService>().Initialize();
            serviceProvider.GetRequiredService<ProfileService>().Initialize();
            serviceProvider.GetRequiredService<StarboardService>().Initialize(); 
            serviceProvider.GetRequiredService<SelfAssignableRolesService>().Initialize(); 
            serviceProvider.GetRequiredService<RatelimitingService>().SetTimer();


            //Connect to Discord
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // initialize Autoreconnect Feature
            _autoReconnectService = new AutoReconnectService(_client, LogPretty);
            
            // setup ban users
            _banService.FetchBannedUsers();
            
            //INITIALIZE CACHE
            CacheService.Initialize();
            
            //build webserver and inject service
            try
            {
                int port = int.Parse(ConfigService.GetConfigData("port"));
                var host = new WebHostBuilder()
                    .UseKestrel() // MVC webserver is called Kestrel when self hosting
                    .UseUrls("http://localhost:" + (port+shardId)) // Bind to localhost:port to allow http:// calls. TODO ADD WEBPORT
                    .UseContentRoot(Directory.GetCurrentDirectory() + @"/web/") // Required to be set and exist. Create web folder in the folder the bot runs from. Folder can be empty.
                    .UseWebRoot(Directory.GetCurrentDirectory() + @"/web/") // Same as above.
                    .UseStartup<Startup>() // Use Startup class in Startup.cs
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(_client);     // Injected Discord client
                        services.AddSingleton(_banService); // Injected Ban service
                        services.AddSingleton(_restClient); // inject rest Client
                        services.AddCors(options =>
                        {
                            options.AddPolicy("AllowLocal", builder => builder.WithOrigins("localhost")); // Enable CORS to only allow calls from localhost
                        });
                        services.AddMvc().AddJsonOptions( options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore ); // Fixes JSON Recursion issues in API response.
                    })
                    .Build(); // Actually creates the webhost
                Console.WriteLine($"WEB API STARTED ON PORT: {port+shardId}");
                await host.RunAsync(); // Run in tandem to client
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendMessage(e.ToString());
            }

            //Hang indefinitely
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_client);
            services.AddSingleton(_restClient);
            services.AddSingleton<CommandService>();
            services.AddSingleton<ClanService>();

            //// Disabled by Catherine Renelle - Memory leak fix
            ////services.AddDbContext<SoraContext>(options => options.UseMySql(_connectionString),ServiceLifetime.Transient);//, ServiceLifetime.Transient

            services.AddSingleton<LavaSocketClient>();
            services.AddSingleton<AudioService>();
            services.AddSingleton<CommandHandler>();
            services.AddSingleton(_interactive);
            services.AddSingleton(_banService);
            services.AddSingleton<InteractionsService>();
            services.AddSingleton<OwnerService>();
            services.AddSingleton<LogService>();
            services.AddSingleton<CoinService>();
            services.AddSingleton<AfkService>();
            services.AddSingleton<DynamicPrefixService>();
            services.AddSingleton<RatelimitingService>();
            services.AddSingleton<MusicShareService>();
            services.AddSingleton<WaifuService>();
            services.AddSingleton<SelfAssignableRolesService>();
            services.AddSingleton<WeatherService>();
            services.AddSingleton<AnnouncementService>();
            services.AddSingleton<MarriageService>();
            services.AddSingleton<StarboardService>();
            services.AddSingleton<GiphyService>();
            services.AddSingleton<WeebService>();
            services.AddSingleton<GuildLevelRoleService>();
            services.AddSingleton<ModService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<ReminderService>();
            services.AddSingleton<GuildCountUpdaterService>();
            services.AddSingleton<UbService>();
            services.AddSingleton<ImdbService>();
            services.AddSingleton<ExpService>();
            services.AddSingleton<TagService>();
            services.AddSingleton<AnimeSearchService>();


            return new DefaultServiceProviderFactory().CreateServiceProvider(services);
        }

        // Example of a logging handler. This can be re-used by addons
    // that ask for a Func<LogMessage, Task>.
    private static Task LogPretty(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();
        
        // If you get an error saying 'CompletedTask' doesn't exist,
        // your project is targeting .NET 4.5.2 or lower. You'll need
        // to adjust your project's target framework to 4.6 or higher
        // (instructions for this are easily Googled).
        // If you *need* to run on .NET 4.5 for compat/other reasons,
        // the alternative is to 'return Task.Delay(0);' instead.
        return Task.CompletedTask;
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