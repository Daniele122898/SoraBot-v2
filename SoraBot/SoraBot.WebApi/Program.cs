using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SoraBot.Data;

namespace SoraBot.WebApi
{
    public class Program
    {
        public static int Main(string[] args)
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
            try
            {
                Log.Information("Starting web host");
                var host = CreateHostBuilder(args).Build();

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
                        if (env.IsProduction())
                        {
                            Log.Information("In Production. Change Logger Config");
                            // Change the logger configuration To not be as verbose.     
                            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Information()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
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
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                return e.HResult;
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
                    webBuilder.UseSentry();
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}