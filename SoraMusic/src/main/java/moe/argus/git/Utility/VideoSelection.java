package moe.argus.git.Utility;

import com.sedmelluq.discord.lavaplayer.track.AudioTrack;
import sx.blah.discord.handle.obj.IMessage;

import java.util.List;

public class VideoSelection {

    private final List<AudioTrack> choices;
    private final IMessage msg;

    public VideoSelection(List<AudioTrack> choices, IMessage msg){
        this.choices = choices;
        this.msg = msg;
    }

    public List<AudioTrack> getChoices(){
        return choices;
    }

    public IMessage getMsg(){
        return msg;
    }

}
