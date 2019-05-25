using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Hosting.Internal;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class CoinService
    {
        private const int GAIN_COINS = 500;

        public async Task SendMoney(SocketCommandContext context, int amount, ulong userId)
        {
            if (context.User.Id == userId)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You can't send money to yourself... Because... Why would you?").Build());
                return;
            }
            if (amount < 1)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Amount must be greater than 1!").Build());
                return;
            }
            using (var soraContext = new SoraContext())
            {
                // get current userDb
                var userdb = Utility.OnlyGetUser(context.User.Id, soraContext);
                // check if user exists and if he has the money
                if (userdb == null || userdb.Money < amount)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You don't have enough Sora Coins!").Build());
                    return;
                }
                // get or create other userDb
                var otherDb = Utility.GetOrCreateUser(userId, soraContext);
                // transfer the money
                userdb.Money -= amount;
                otherDb.Money += amount;
                // save changes
                await soraContext.SaveChangesAsync();
            }

            var user = context.Client.GetUser(userId);
            
            // send message to other user
            try
            {
                if (user == null)
                    throw new Exception();
                
                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.PurpleEmbed,
                    Utility.SuccessLevelEmoji[4],
                    $"💰 You've received {amount} SC from {Utility.GiveUsernameDiscrimComb(context.User)} !"
                ).Build());

                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You have successfully transfered {amount} SC to {Utility.GiveUsernameDiscrimComb(user)}! They've been notified."
                ).Build());
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.YellowWarningEmbed,
                    Utility.SuccessLevelEmoji[1],
                    $"You have successfully transfered {amount} SC to {(user == null ? userId.ToString() : Utility.GiveUsernameDiscrimComb(user))}!."
                ).WithDescription("But I failed to send him/her a DM, they have probably disabled that feature. You may want to notify him/her yourself.").Build());
            }
        }
        
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
                            "Something went wrong soryy :c").Build());
                    return;
                }
                if (userdb.NextDaily.CompareTo(DateTime.UtcNow) >= 0)
                {
                    var timeRemaining = userdb.NextDaily.Subtract(DateTime.UtcNow.TimeOfDay).TimeOfDay;
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You can't earn anymore, please wait another {timeRemaining.Humanize(minUnit: TimeUnit.Second)}!").Build());
                    return;
                }
                // add 20h cooldown
                userdb.NextDaily = DateTime.UtcNow.AddHours(20);
                // give coins
                userdb.Money += GAIN_COINS;
                // save changes
                await soraContext.SaveChangesAsync();

                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You gained {GAIN_COINS} Sora Coins! You can earn again in 24h.").Build());
            }       
        }

        public int GetAmount(ulong userId)
        {
            using (var soraContext = new SoraContext())
            {
                // get user db data
                var userdb = Utility.OnlyGetUser(userId, soraContext);
                return userdb?.Money ?? 0;
            }
        }
    }
}