using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SoraBot.Data;
using SoraBot.Services.Utils;

namespace SoraBot.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                // Change the logger configuration To not be as verbose.     
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("SoraBot.Bot.Extensions.DiscordSerilogAdapter", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug)
                    .WriteTo.Sentry(o =>
                    {
                        o.MinimumBreadcrumbLevel = LogEventLevel.Warning;
                        o.MinimumEventLevel = LogEventLevel.Error;
                        o.AttachStacktrace = true;
                        o.SendDefaultPii = true;
                    })
                    .CreateLogger();

                GlobalConstants.Production = true;
                Log.Information("In Production. Change Logger Config");
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("SoraBot.Bot.Extensions.DiscordSerilogAdapter", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug)
                    .WriteTo.Sentry(o =>
                    {
                        o.MinimumBreadcrumbLevel = LogEventLevel.Warning;
                        o.MinimumEventLevel = LogEventLevel.Error;
                        o.AttachStacktrace = true;
                        o.SendDefaultPii = true;
                    })
                    .CreateLogger();
            }

            // Set the shardId to be used across the app
            if (args.Length == 0 || !int.TryParse(args[0], out int shardId))
            {
                shardId = 0;
            }

            GlobalConstants.SetShardId(shardId);
            Log.Logger.Information($"Using Shard ID: {shardId.ToString()}");

            // Parse and set up port
            if (args.Length < 2 || !int.TryParse(args[1], out int port))
            {
                port = 9100;
            }

            port += shardId;
            GlobalConstants.SetPort(port);
            Log.Logger.Information($"Binding Port: {port.ToString()}");

            try
            {
                Log.Information("Starting web host");
                using var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        // Before we do anything else make sure the DB is setup correctly
                        Log.Information("Applying migrations if needed");
                        using var context = services.GetRequiredService<SoraContext>();
                        context.Database.Migrate();

                        IWebHostEnvironment env = services.GetRequiredService<IWebHostEnvironment>();

                        if (env.IsDevelopment())
                        {
                            // This is where seeding happens if needed
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Fatal(e, "Could not setup services or migrations properly. Shutting down...");
                        throw;
                    }
                }

                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    int p = GlobalConstants.Port;
                    webBuilder.UseSentry();
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://*:{p.ToString()}");
                })
                .UseSerilog();
    }
}