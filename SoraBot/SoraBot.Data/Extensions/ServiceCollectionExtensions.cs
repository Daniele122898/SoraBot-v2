using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoraBot.Data.Repositories;
using SoraBot.Data.Repositories.GuildRepos;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoraData(this IServiceCollection services, IConfiguration configs)
        {
            services.AddScoped<ITransactor<SoraContext>, SoraDbTransactor>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICoinRepository, CoinRepository>();
            services.AddScoped<IWaifuRepository, WaifuRepository>();
            services.AddScoped<IGuildRepository, GuildRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IStarboardRepository, StarboardRepository>();
            services.AddScoped<IReminderRepository, ReminderRepository>();
            
            // Use this pool in the transactor as well for improved performance
            services.AddDbContextPool<SoraContext>(op =>
            {
                op.UseLazyLoadingProxies();
                op.UseMySql(configs.GetSection("SoraBotSettings").GetValue<string>("DbConnection"));
            });

            // Inject the context factory so we use the ContextPool
            services.AddScoped<Func<SoraContext>>(provider => provider.GetRequiredService<SoraContext>);
            
            
            return services;
        }
    }
}