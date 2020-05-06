using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using SoraBot.Services.Utils;

namespace SoraBot.Services.Reminder
{
    public class ReminderService : IReminderService
    {
        private readonly ILogger<ReminderService> _log;
        public const int TIMER_INTERVAL_MINS = 1;
        
        private readonly Timer _timer;
        
        public ReminderService(ILogger<ReminderService> log)
        {
            _log = log;
            // We only want the timer to run on shard 0. ONLY shard 0 shall actually handle reminders.
            // All other shards shall just add to the reminders!
            if (GlobalConstants.ShardId == 0)
            {
                _timer = new Timer(CheckReminders, null, TimeSpan.FromMinutes(TIMER_INTERVAL_MINS), 
                    TimeSpan.FromMinutes(TIMER_INTERVAL_MINS));
            }
        }

        private void CheckReminders(object _)
        {
            // Change it since this might take a while or introduce some kind of lag
            // so we dont accidentally want to have multiple instances running at the same time
            _timer.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            try
            {

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