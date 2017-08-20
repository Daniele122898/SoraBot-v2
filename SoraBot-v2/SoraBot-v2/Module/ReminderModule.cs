using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class ReminderModule : ModuleBase<SocketCommandContext>
    {
        private ReminderService _reminderService;

        public ReminderModule(ReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [Command("remind"), Alias("rem", "remind me"), Summary("Sets a reminder for you")]
        public async Task SetReminder([Remainder] string message)
        {
            await _reminderService.SetReminder(Context, message);
        }
        
        [Command("reminders"), Alias("rems", "remlist"), Summary("Shows your remidners")]
        public async Task ShowReminders()
        {
            await _reminderService.ShowReminders(Context);
        }

        [Command("removeremind", RunMode = RunMode.Async), Alias("rmrem", "rmremind"), Summary("Let's you remove reminders")]
        public async Task RemoveReminders()
        {
            await _reminderService.RemoveReminder(Context);
        }

    }
}