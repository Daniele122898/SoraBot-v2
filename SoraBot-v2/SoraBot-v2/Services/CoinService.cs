using System;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Hosting.Internal;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class CoinService
    {
        private const int GAIN_COINS = 500;

        public async Task DoDaily(SocketCommandContext context )
        {
            using (var soraContext = new SoraContext())
            {
                // get user db data
                var userdb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                // Check if he can gain again or userdb is null for some odd reason
                if (userdb == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Something went wrong soryy :c"));
                    return;
                }
                if (userdb.NextDaily.CompareTo(DateTime.UtcNow) >= 0)
                {
                    var timeRemaining = userdb.NextDaily.Subtract(DateTime.UtcNow.TimeOfDay).TimeOfDay;
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You can't earn anymore, please wait another {timeRemaining.Humanize(minUnit: TimeUnit.Second)}!"));
                    return;
                }
                // add 24h cooldown
                userdb.NextDaily = DateTime.UtcNow.AddHours(24);
                // give coins
                userdb.Money += GAIN_COINS;
                // save changes
                await soraContext.SaveChangesAsync();

                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You gained {GAIN_COINS} Sora Coins! You can earn again in 24h."));
            }       
        }
    }
}