using System;
using System.Linq;
using System.Threading;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;
using SoraBot.Services.Utils;

namespace SoraBot.Services.Reminder
{
    public class ReminderService : IReminderService
    {
        public const int TIMER_INTERVAL_MINS = 1;

        private readonly ILogger<ReminderService> _log;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUserService _userService;
        private readonly Timer _timer;
        
        public ReminderService(
            ILogger<ReminderService> log, 
            IServiceScopeFactory scopeFactory,
            IUserService userService)
        {
            _log = log;
            _scopeFactory = scopeFactory;
            _userService = userService;
            // We only want the timer to run on shard 0. ONLY shard 0 shall actually handle reminders.
            // All other shards shall just add to the reminders!
            if (GlobalConstants.ShardId == 0)
            {
                _timer = new Timer(CheckReminders, null, TimeSpan.FromMinutes(TIMER_INTERVAL_MINS), 
                    TimeSpan.FromMinutes(TIMER_INTERVAL_MINS));
            }
        }

        private async void CheckReminders(object _)
        {
            // Change it since this might take a while or introduce some kind of lag
            // so we dont accidentally want to have multiple instances running at the same time
            _timer.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminderRepo = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
                var reminders = await reminderRepo.GetAllRemindersThatAreDue().ConfigureAwait(false);
                if (reminders == null || reminders.Count == 0) return;
                // Otherwise work through the reminders
                // User a for loop because it's faster than a foreach :)
                for (int i = 0; i < reminders.Count; i++)
                {
                    var reminder = reminders[i];
                    var user = await _userService.GetOrSetAndGet(reminder.UserId).ConfigureAwait(false);
                    if (!user.HasValue) continue;
                    try
                    {
                        var channel = await user.Value.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                        if (channel == null) return;
                        var eb = new EmbedBuilder()
                        {
                            Color = SoraSocketCommandModule.Purple,
                            Title = "⏰ Reminder!",
                            Description = reminder.Message
                        };
                        await channel.SendMessageAsync(embed: eb.Build()).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // This is probably due to the user not accepting DMs.
                        // not worth handling or doing anything about
                        // Like to have it here to be more explicit when reading the code. Just preference :)
                        // ReSharper disable once RedundantJumpStatement
                        continue;
                    }
                }
                
                // remove all the reminders
                var reminderIds = reminders.Select(x => x.Id).ToList();
                await reminderRepo.RemoveReminders(reminderIds).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to send or manipulate reminders!");
            }
            finally
            {
                _timer.Change(TimeSpan.FromMinutes(TIMER_INTERVAL_MINS),
                    TimeSpan.FromMinutes(TIMER_INTERVAL_MINS));
            }
        }
    }
}