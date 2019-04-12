using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;

namespace SoraBot_v2.Services
{
    public partial class AudioService
    {
        public void Initialize(LavaSocketClient lavaSocketClient, LavaRestClient lavaRestClient, ulong soraId)
        {
            _soraId = soraId;
            _lavaSocketClient = lavaSocketClient;
            _lavaRestClient = lavaRestClient;
            // lavanode events
            _lavaSocketClient.OnTrackStuck += OnTrackStuck;
            _lavaSocketClient.OnTrackFinished += OnTrackFinished;
            _lavaSocketClient.OnTrackException += OnTrackException;
            _lavaSocketClient.OnServerStats  += StatsUpdated;
            //node.PlayerUpdated = PlayerUpdated;
        }
        
        private Task StatsUpdated(ServerStats stats)
        {
            _serverStats = stats;
            return Task.CompletedTask;
        }
        
        private async Task OnTrackException(LavaPlayer player, LavaTrack track, string arg3)
        {
            RemoveVotes(player.VoiceChannel.GuildId);
            player.Queue.Remove(track);
            await player.TextChannel.SendMessageAsync(
                "",embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {track.Title} threw an exception. Track has been removed.")
                    .WithDescription(string.IsNullOrWhiteSpace(arg3) ? "Unknown Exception" : arg3)
                    .Build());
        }
        
        private async Task OnTrackStuck(LavaPlayer player, LavaTrack lavaTrack, long arg3)
        {
            RemoveVotes(player.VoiceChannel.GuildId);
            player.Queue.Remove(lavaTrack);
            await player.TextChannel.SendMessageAsync(
                "", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {lavaTrack.Title} got stuck: {arg3}. Track has been removed.")
                    .WithUrl(lavaTrack.Uri.ToString())
                    .Build());
        }
        
        private async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (player == null)
                return;
            if (!reason.ShouldPlayNext())
                return;
            
            // player.Remove(track);

            ulong guildId = player.VoiceChannel.GuildId;

            _options.TryGetValue(guildId, out var options);

            LavaTrack nextTrack = null;

            if (options != null && options.RepeatTrack)
                nextTrack = await RepeatTrackPlay(track.Uri.ToString());
            else
            {
                nextTrack = player.Queue.Count == 0 ? null : player.Queue.Dequeue() as LavaTrack;
            }            
            
            RemoveVotes(guildId);
            if (nextTrack == null)
            {
                await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        "Queue Completed!")
                    .Build());
                return;
            }

            await player.PlayAsync(nextTrack);
            await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    $"Now Playing: {nextTrack.Title}")
                .WithUrl(nextTrack.Uri.ToString())
                .Build());
        }
    }
}