package moe.argus.git.Utility;

import com.sedmelluq.discord.lavaplayer.player.AudioLoadResultHandler;
import com.sedmelluq.discord.lavaplayer.player.AudioPlayerManager;
import com.sedmelluq.discord.lavaplayer.player.DefaultAudioPlayerManager;
import com.sedmelluq.discord.lavaplayer.source.soundcloud.SoundCloudAudioSourceManager;
import com.sedmelluq.discord.lavaplayer.source.youtube.YoutubeAudioSourceManager;
import com.sedmelluq.discord.lavaplayer.tools.FriendlyException;
import com.sedmelluq.discord.lavaplayer.track.AudioPlaylist;
import com.sedmelluq.discord.lavaplayer.track.AudioTrack;

public class SearchUtil {

    private static final AudioPlayerManager PLAYER_MANAGER = initPlayerManager();
    private static final int DEFAULT_TIMEOUT = 3000;

    private static AudioPlayerManager initPlayerManager(){
        DefaultAudioPlayerManager manager = new DefaultAudioPlayerManager();
        manager.registerSourceManager(new YoutubeAudioSourceManager());
        manager.registerSourceManager(new SoundCloudAudioSourceManager());
        return manager;
    }

    public static AudioPlaylist searchForTracks(String query, String prefix) {
        return searchForTracks(query,prefix, DEFAULT_TIMEOUT);
    }

    public static AudioPlaylist searchForTracks(String query, String prefix, int timeout) {
        return new SearchResultHandler().searchSync(query, prefix, timeout);
    }

    static class SearchResultHandler implements AudioLoadResultHandler {

        Throwable throwable;
        AudioPlaylist result;
        final Object toBeNotified = new Object();

        AudioPlaylist searchSync(String query, String prefix, int timeout) {
            try {
                synchronized (toBeNotified) {
                    PLAYER_MANAGER.loadItem(prefix + query, this);//TODO MAKE CHANGEABLE TO SOUNDCLOUD
                    toBeNotified.wait(timeout);
                }
            } catch (InterruptedException e) {
                throw new RuntimeException("Was interrupted while searching", e);
            }

            if (throwable != null) {
                throw new RuntimeException("Failed to search!", throwable);
            }

            return result;
        }

        @Override
        public void trackLoaded(AudioTrack audioTrack) {
            throwable = new UnsupportedOperationException("Can't load a single track when we are expecting a playlist!");
            synchronized (toBeNotified) {
                toBeNotified.notify();
            }
        }

        @Override
        public void playlistLoaded(AudioPlaylist audioPlaylist) {
            result = audioPlaylist;
            synchronized (toBeNotified) {
                toBeNotified.notify();
            }

        }

        @Override
        public void noMatches() {
            synchronized (toBeNotified) {
                toBeNotified.notify();
            }
        }

        @Override
        public void loadFailed(FriendlyException e) {
            throwable = e;
            synchronized (toBeNotified) {
                toBeNotified.notify();
            }
        }
    }
}
