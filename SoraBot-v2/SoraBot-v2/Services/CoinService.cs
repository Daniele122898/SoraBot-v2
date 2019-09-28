using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class CoinService
    {
        private const int GAIN_COINS = 500;
        private const int DAILY_COOLDOWN = 20;

        public const int LOCK_TIMOUT_MSECONDS = 10000;
        private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _coinLocks = new ConcurrentDictionary<ulong, SemaphoreSlim>();
        private readonly DiscordRestClient _restClient;
        public CoinService(DiscordRestClient restClient)
        {
            _restClient = restClient;
        }
        
        public SemaphoreSlim GetOrCreateLock(ulong id)
        {
            if (_coinLocks.TryGetValue(id, out var key)) return key;
            key = new SemaphoreSlim(1, 1);
            _coinLocks.TryAdd(id, key);

            return key;
        }

        public async Task LockingErrorMessage(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Locking error. Please try again.").Build());
        }

        public void GetSortedLocks(ulong id1, ulong id2, out SemaphoreSlim lock1, out SemaphoreSlim lock2)
        {
            ulong first, second;
            if (id1 > id2)
            {
                first = id1;
                second = id2;
            }
            else
            {
                first = id2;
                second = id1;
            }

            lock1 = GetOrCreateLock(first);
            lock2 = GetOrCreateLock(second);
        }

        public async Task SendMoney(SocketCommandContext context, int amount, ulong userId, bool check = false)
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

            if (check)
            {
                var restUser = await _restClient.GetUserAsync(userId);
                if (restUser == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "This user does not exist or I am not connected to him.").Build());
                    return;
                }
            }
            
            using (var soraContext = new SoraContext())
            {
                GetSortedLocks(context.User.Id, userId, out var lock1, out var lock2);
                try
                {
                    if (!await lock1.WaitAsync(LOCK_TIMOUT_MSECONDS))
                    {
                        await LockingErrorMessage(context.Channel);
                        return;
                    }

                    if (!await lock2.WaitAsync(LOCK_TIMOUT_MSECONDS))
                    {
                        await LockingErrorMessage(context.Channel);
                        return;
                    }
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
                finally
                {
                    lock1.Release();
                    lock2.Release();
                }
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
                var lck = GetOrCreateLock(context.User.Id);
                try
                {
                    if (!await lck.WaitAsync(LOCK_TIMOUT_MSECONDS))
                    {
                        await LockingErrorMessage(context.Channel);
                        return;
                    }
                    // get user db data
                    var userdb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                    // Check if he can gain again or userdb is null for some odd reason
                    if (userdb == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                "Something went wrong sorry :c").Build());
                        return;
                    }

                    if (userdb.NextDaily.CompareTo(DateTime.UtcNow) >= 0)
                    {
                        var timeRemaining = userdb.NextDaily.Subtract(DateTime.UtcNow.TimeOfDay).TimeOfDay;
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                                Utility.RedFailiureEmbed,
                                Utility.SuccessLevelEmoji[2],
                                $"You can't earn anymore, please wait another {timeRemaining.Humanize(minUnit: TimeUnit.Second, precision: 2)}!")
                            .Build());
                        return;
                    }

                    // add 20h cooldown
                    userdb.NextDaily = DateTime.UtcNow.AddHours(DAILY_COOLDOWN);
                    // give coins
                    userdb.Money += GAIN_COINS;
                    // save changes
                    await soraContext.SaveChangesAsync();

                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed,
                        Utility.SuccessLevelEmoji[0],
                        $"You gained {GAIN_COINS} Sora Coins! You can earn again in {DAILY_COOLDOWN}h.").Build());
                }
                
                finally
                {
                    lck.Release();
                }
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