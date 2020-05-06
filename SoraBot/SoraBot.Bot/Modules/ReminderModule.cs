using System;
using System.Linq;
using System.Text.RegularExpressions;
using ArgonautCore.Maybe;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    [Name("Reminders")]
    [Summary("A collection of commands to set up reminders for things you do not want to forget :D")]
    public class ReminderModule : SoraSocketCommandModule
    {
        private readonly IReminderRepository _remindRepo;

        public ReminderModule(IReminderRepository remindRepo)
        {
            _remindRepo = remindRepo;
        }
        
        
        
        /// <summary>
        /// Tries to properly parse the time or if it cant returns a Zero Maybe
        /// No silent failing. If smth isn't exactly right we completely fail the entire parsing!
        /// </summary>
        private Maybe<DateTime> ParseTime(string message)
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
                if (uint.TryParse(captured[1].Value, out var time))
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