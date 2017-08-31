package lavaplayer;

import com.sedmelluq.discord.lavaplayer.player.AudioPlayer;
import com.sedmelluq.discord.lavaplayer.player.event.AudioEventAdapter;
import com.sedmelluq.discord.lavaplayer.track.AudioTrack;
import com.sedmelluq.discord.lavaplayer.track.AudioTrackEndReason;

import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

public class TrackScheduler {
    private final List<AudioTrack> queue;
    private final AudioPlayer player;
    private boolean repeatSong = false;
    private boolean repeatPlaylist = false;

    public TrackScheduler(AudioPlayer audioPlayer){
        // Because we will be removing from the "head" of the queue frequently, a LinkedList is a better implementation
        // since all elements won't have to be shifted after removing. Additionally, choosing to add in between the queue
        // will similarly not cause many elements to shift and wil only require a couple of node changes.
        queue = Collections.synchronizedList(new LinkedList<>());
        this.player = audioPlayer;
        // For encapsulation, keep the listener anonymous.
        player.addListener(new AudioEventAdapter() {
            @Override
            public void onTrackEnd(AudioPlayer player, AudioTrack track, AudioTrackEndReason endReason) {
                // Only start the next track if the end reason is suitable for it (FINISHED or LOAD_FAILED)
                if(endReason.mayStartNext) {
                    onNextTrack(track);
                }
            }
        });
    }

    public synchronized void shuffle(){
        Collections.shuffle(queue);
    }

    public synchronized boolean toggleRepeatSong(){
        repeatSong = !repeatSong;
        return repeatSong;
    }

    public synchronized boolean toggleRepeatPlaylist(){
        repeatPlaylist = !repeatPlaylist;
        return repeatPlaylist;
    }

    /**
     * Add the next track to queue or play right away if nothing is in the queue.
     *
     * @param track The track to play or add to queue.
     */
    public synchronized boolean queue(AudioTrack track) {
        // Calling startTrack with the noInterrupt set to true will start the track only if nothing is currently playing. If
        // something is playing, it returns false and does nothing. In that case the player was already playing so this
        // track goes to the queue instead.
        boolean playing = player.startTrack(track, true);

        if(!playing) {
            queue.add(track);
        }

        return playing;
    }

    @SuppressWarnings("Duplicates")
    public synchronized void onNextTrack(AudioTrack currentTrack){
        //Create clones bcs audiotracks arent reusable
        if(repeatSong && currentTrack != null){
            AudioTrack clone = currentTrack.makeClone();
            player.startTrack(clone, false);
            return;
        }
        AudioTrack nextTrack;
        if(repeatPlaylist && currentTrack != null){
            nextTrack= queue.remove(0);
            AudioTrack clone = currentTrack.makeClone();
            queue.add(clone);//add now finished track to the end of the queue
        }else{
            nextTrack = queue.isEmpty() ? null : queue.remove(0);
        }
        // Start the next track, regardless of if something is already playing or not. In case queue was empty, we are
        // giving null to startTrack, which is a valid argument and will simply stop the player.
        player.startTrack(nextTrack, false);

    }

    /**
     * Starts the next track, stopping the current one if it is playing.
     * @return The track that was stopped, null if there wasn't anything playing
     */
    @SuppressWarnings("Duplicates")
    public synchronized AudioTrack nextTrack() {
        AudioTrack currentTrack = player.getPlayingTrack();
        AudioTrack nextTrack;
        if(repeatPlaylist && currentTrack != null && queue.size() != 0){
            nextTrack= queue.remove(0);
            AudioTrack clone = currentTrack.makeClone();
            queue.add(clone);//add now finished track to the end of the queue
        }else{
            nextTrack = queue.isEmpty() ? null : queue.remove(0);
        }

        // Start the next track, regardless of if something is already playing or not. In case queue was empty, we are
        // giving null to startTrack, which is a valid argument and will simply stop the player.
        player.startTrack(nextTrack, false);
        return currentTrack;
    }

    /**
     * Returns the queue for this scheduler. Adding to the head of the queue (index 0) does not automatically
     * cause it to start playing immediately. The returned collection is thread-safe and can be modified.
     *
     * @apiNote To iterate over this queue, use a synchronized block. For example:
     * {@code synchronize (getQueue()) { // iteration code } }
     */
    public List<AudioTrack> getQueue() {
        return this.queue;
    }

    public AudioPlayer getPlayer() {
        return this.player;
    }
}
