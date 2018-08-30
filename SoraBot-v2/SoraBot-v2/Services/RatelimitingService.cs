using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class RatelimitingService
    {
        private const int INITIAL_FILL= 12;
        private const int MAX_FILL = 24;
        private const int SIZE_PER_DROP = 1;
        private const int INITIAL_DELAY = 20;
        private const int BUCKET_DROP_INTERVAL = 10;
        private const int ADDITIONAL_DELETION = 2;

        private DiscordSocketClient _client;
        
        private ConcurrentDictionary<ulong, Bucket> _bucketDict = new ConcurrentDictionary<ulong, Bucket>();

        private Timer _timer;

        public RatelimitingService(DiscordSocketClient client)
        {
            _client = client;
        }
            
        public void SetTimer()
        {
            _timer = new Timer(FillBuckets, null, TimeSpan.FromSeconds(INITIAL_DELAY),
            TimeSpan.FromSeconds(BUCKET_DROP_INTERVAL));
        }

        private void FillBuckets(Object stateInfo)
        {
            var temp = _bucketDict.ToArray();
            foreach (var bucket in temp)
            {
                if (bucket.Value.Fill < MAX_FILL)
                {
                    bucket.Value.Fill += SIZE_PER_DROP;
                    if (bucket.Value.Fill > 0 && bucket.Value.SendMessage)
                        bucket.Value.SendMessage = false;
                    _bucketDict.TryUpdate(bucket.Key, bucket.Value);
                }
                    
            }
        }

        private void CreateBucketIfItDoesntExist(ulong userId)
        {
            if (!_bucketDict.ContainsKey(userId))
            {
                Bucket bucket = new Bucket()
                {
                    Fill = INITIAL_FILL
                };
                _bucketDict.TryAdd(userId, bucket);
            }
        }

        private async Task SendMessage(ulong userId)
        {
            var user = _client.GetUser(userId);
            if(user == null)
                return;
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(
            Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], "You have been ratelimited by Sora!").WithDescription(
                "Please refrain from spamming Sora. Please use him normally or a permanent ban will be issued if repeated often.\n" +
                $"If you think you did not spam then join [this guild and open a ratelimit appeal]({Utility.DISCORD_INVITE})!").Build());
            await SentryService.SendMessage(
                $"**USER RATELIMITED**\nUser: {Utility.GiveUsernameDiscrimComb(user)} ({userId})");
        }

        public async Task<bool> IsRatelimited(ulong userId)
        {
            CreateBucketIfItDoesntExist(userId);
            Bucket bucket;
            _bucketDict.TryGetValue(userId, out bucket);

            if (bucket?.Fill < 1)
            {
                if (!bucket.SendMessage)
                {
                    //Send Message
                    await SendMessage(userId);
                    bucket.SendMessage = true;
                    _bucketDict.TryUpdate(userId, bucket);
                }
                return true;
            }
            return false;
        }

        public void RateLimitMain(ulong userId)
        {
            CreateBucketIfItDoesntExist(userId);
            Bucket bucket;
            _bucketDict.TryGetValue(userId, out bucket);
            if(bucket == null)
                return;
            bucket.Fill--;
            if (bucket.Fill == 0)
                bucket.Fill -= ADDITIONAL_DELETION;
            _bucketDict.TryUpdate(userId, bucket);
        }
        
    }


    internal class Bucket
    {
        public sbyte Fill { get; set; }
        public bool SendMessage { get; set; } = false;
    }
}