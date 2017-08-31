package lavaplayer;

import com.sedmelluq.discord.lavaplayer.player.AudioPlayer;
import com.sedmelluq.discord.lavaplayer.player.AudioPlayerManager;
import com.sedmelluq.discord.lavaplayer.player.event.AudioEventListener;
import moe.argus.git.Utility.VideoSelection;

import java.util.HashMap;
import java.util.Map;

public class GuildMusicManager {
    private final AudioPlayer player;
    private final AudioProvider provider;
    private final TrackScheduler scheduler;

    public final Map<Long, VideoSelection> selections = new HashMap<>();

    /**
     * Creates a player and a track scheduler.
     * @param manager Audio player manager to use for creating the player.
     */
    public GuildMusicManager(AudioPlayerManager manager) {
        player = manager.createPlayer();
        provider = new AudioProvider(player);
        scheduler = new TrackScheduler(player);
    }

    /**
     * Adds a listener to be registered for audio events.
     */
    public void addAudioListener(AudioEventListener listener) {
        player.addListener(listener);
    }

    /**
     * Removes a listener that was registered for audio events.
     */
    public void removeAudioListener(AudioEventListener listener) {
        player.removeListener(listener);
    }

    /**
     * @return The scheduler for AudioTracks.
     */
    public TrackScheduler getScheduler() {
        return this.scheduler;
    }

    /**
     * @return Wrapper around AudioPlayer to use it as an AudioSendHandler.
     */
    public AudioProvider getAudioProvider() {
        return provider;
    }
}
