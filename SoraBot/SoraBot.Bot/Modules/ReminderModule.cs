using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using SoraBot.Bot.Extensions.Interactive;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    [Name("Reminders")]
    [Summary("A collection of commands to set up reminders for things you do not want to forget :D")]
    public class ReminderModule : SoraSocketCommandModule
    {
        private readonly IReminderRepository _remindRepo;
        private readonly InteractiveService _interactiveService;
        private const short _MAX_USER_REMINDERS = 10;

        public ReminderModule(IReminderRepository remindRepo, InteractiveService interactiveService)
        {
            _remindRepo = remindRepo;
            _interactiveService = interactiveService;
        }

        [Command("removereminders"), Alias("remove reminders", "rmrem", "delrem")]
        [Summary("Select which reminder to remove from a list of all of your reminders")]
        public async Task RemoveReminder()
        {
            var rems = await _remindRepo.GetUserReminders(Context.User.Id).ConfigureAwait(false);
            if (!rems.HasValue)
            {
                await ReplyFailureEmbed("You don't have any reminders.");
                return;
            }
            
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                ThumbnailUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                Title = "⏰ Remove a Reminder",
                Description = "Answer with a number indicating the ID of the reminder you'd like to remove.",
                Footer = RequestedByMe()
            };
            
            rems.Value.Sort((r1, r2) => r1.DueDateUtc.CompareTo(r2.DueDateUtc));

            for (int i = 0; i < rems.Value.Count; i++)
            {
                var rem = rems.Value[i];
                var remindIn = rem.DueDateUtc.Subtract(DateTime.UtcNow);
                int num = i + 1;
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name =
                        $"**{num.ToString()}#** Due in {remindIn.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 4)}";
                    x.Value = $"{rem.Message}\n_On {rem.DueDateUtc.Date.ToString("dd/MM/yyyy")}_";
                });
            }

            await ReplyEmbed(eb);
            var criteria = InteractiveServiceExtensions.CreateEnsureFromUserInChannelCriteria(Context.User.Id, Context.Channel.Id);
            var resp = await _interactiveService.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(45));
            if (resp == null)
            {
                await ReplyFailureEmbed("Failed to answer in time >.<");
                return;
            }

            if (!int.TryParse(resp.Content, out var removeId))
            {
                await ReplyFailureEmbed("Please respond with the ID of the reminder to remove e.g. `1` or `7`.");
                return;
            }
            removeId--;
            if (removeId < 0 || removeId >= rems.Value.Count)
            {
                await ReplyFailureEmbed($"Not a valid ID! Please choose a reminder between 1 and {rems.Value.Count.ToString()}");
                return;               
            }

            await _remindRepo.RemoveReminder(rems.Value[removeId].Id);
            await ReplySuccessEmbed("Successfully removed reminder.");
        }

        [Command("reminders"), Alias("rems", "remlist", "reminderlist")]
        [Summary("Shows you all your reminders and when they go off")]
        public async Task AllReminders()
        {
            var rems = await _remindRepo.GetUserReminders(Context.User.Id).ConfigureAwait(false);
            if (!rems.HasValue)
            {
                await ReplyFailureEmbed("You don't have any reminders.");
                return;
            }
                            
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                ThumbnailUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                Title = "⏰ Reminders",
                Footer = RequestedByMe()
            };
            
            rems.Value.Sort((r1, r2) => r1.DueDateUtc.CompareTo(r2.DueDateUtc));

            for (int i = 0; i < rems.Value.Count; i++)
            {
                var rem = rems.Value[i];
                var remindIn = rem.DueDateUtc.Subtract(DateTime.UtcNow);
                int num = i + 1;
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name =
                        $"{num.ToString()}# Due in {remindIn.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 4)}";
                    x.Value = $"{rem.Message}\n_On {rem.DueDateUtc.Date.ToString("dd/MM/yyyy")}_";
                });
            }

            await ReplyEmbed(eb);
        }

        [Command("remind"), Alias("rm", "remind me")]
        [Summary("Set's a reminder for you. It must be of the form: " +
                 "`what to remind in 3 hours 2 min`. The **in** is very important!")]
        public async Task RemindMe(
            [Summary("The reminder query"), Remainder]
            string remind)
        {
            var dueDate = ParseTime(remind);
            if (!dueDate.HasValue)
            {
                await ReplyFailureEmbedExtended(
                    "Reminder was not correctly formatted!",
                    "Make sure the reminder is of the format: `<what to remind you> in <when>`\n" +
                    "Where in the first space you just add the text that Sora should remind you of and then the **in** is very important. " +
                    "After it you should add when he should remind you.\n" +
                    "For example: `3 hours` or `10 hours and 5 minutes.`");
                return;
            }
            
            // Check that there's actually a message and not just timer
            string msg = remind
                .Substring(0, remind.LastIndexOf(" in ", StringComparison.InvariantCultureIgnoreCase))
                .Trim();

            if (string.IsNullOrWhiteSpace(msg))
            {
                await ReplyFailureEmbedExtended(
                    "Reminder was not correctly formatted!",
                    "Make sure the reminder is of the format: `<what to remind you> in <when>`\n" +
                    "Where in the first space you just add the text that Sora should remind you of and then the **in** is very important. " +
                    "After it you should add when he should remind you.\n" +
                    "For example: `3 hours` or `10 hours and 5 minutes.`");
                return;
            }
            
            // Otherwise see how many reminders we already have. We don't want to allow users to spam with reminders!
            var userRemCount = await _remindRepo.GetUserReminderCount(Context.User.Id).ConfigureAwait(false);
            if (userRemCount >= _MAX_USER_REMINDERS)
            {
                await ReplyFailureEmbed(
                    $"You already have the maximum amount of {_MAX_USER_REMINDERS.ToString()} reminders!");
                return;
            }
            
            // Just add the reminder to the user :D
            await _remindRepo.AddReminderToUser(Context.User.Id, msg, dueDate.Value).ConfigureAwait(false);
            var remindIn = dueDate.Value.Subtract(DateTime.UtcNow);
            await ReplySuccessEmbedExtended(
                "Successfully set reminder",
                $"I will remind you to `{msg}` in {remindIn.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 10)}");
        }
        
        /// <summary>
        /// Tries to properly parse the time or if it cant returns a Zero Maybe
        /// No silent failing. If smth isn't exactly right we completely fail the entire parsing!
        /// </summary>
        private static Maybe<DateTime> ParseTime(string message)
        {
            if (!message.Contains(" in "))
                return Maybe.Zero<DateTime>();

            string timeString = message.Split(" in ").LastOrDefault();
            if (string.IsNullOrWhiteSpace(timeString))
                return Maybe.Zero<DateTime>();

            var regex = Regex.Matches(timeString, @"(\d+)\s{0,1}([a-zA-Z]*)");
            double timeToAdd = 0;
            for (int i = 0; i < regex.Count; i++)
            {
                var captured = regex[i].Groups;

                if (captured.Count != 3)
                    return Maybe.Zero<DateTime>();
                if (!uint.TryParse(captured[1].Value, out var time))
                    return Maybe.Zero<DateTime>();

                string type = captured[2].Value;

                switch (type)
                {
                    case ("weeks"):
                    case ("week"):
                    case ("w"):
                        timeToAdd += time * 604800;
                        break;
                    case ("day"):
                    case ("days"):
                    case ("d"):
                        timeToAdd += time * 86400;
                        break;
                    case ("hours"):
                    case ("hour"):
                    case ("h"):
                        timeToAdd += time * 3600;
                        break;
                    case ("minutes"):
                    case ("minute"):
                    case ("m"):
                    case ("min"):
                    case ("mins"):
                        timeToAdd += time * 60;
                        break;
                    case ("seconds"):
                    case ("second"):
                    case ("secs"):
                    case ("sec"):
                    case ("s"):
                        timeToAdd += time;
                        break;
                    default:
                        return Maybe.Zero<DateTime>();
                }
            }

            return Maybe.FromVal<DateTime>(DateTime.UtcNow.AddSeconds(timeToAdd));
        }
    }
}