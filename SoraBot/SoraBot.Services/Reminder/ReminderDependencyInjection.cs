using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Reminder
{
    public static class ReminderDependencyInjection
    {
        public static IServiceCollection AddReminderService(this IServiceCollection services)
            => services.AddSingleton<IReminderService, ReminderService>();
    }
}