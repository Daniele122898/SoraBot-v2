package moe.argus.git.commands;

import com.mashape.unirest.http.HttpResponse;
import com.mashape.unirest.http.JsonNode;
import com.mashape.unirest.http.Unirest;
import com.mashape.unirest.http.exceptions.UnirestException;
import com.sedmelluq.discord.lavaplayer.player.AudioLoadResultHandler;
import com.sedmelluq.discord.lavaplayer.player.AudioPlayer;
import com.sedmelluq.discord.lavaplayer.player.AudioPlayerManager;
import com.sedmelluq.discord.lavaplayer.player.DefaultAudioPlayerManager;
import com.sedmelluq.discord.lavaplayer.source.AudioSourceManagers;
import com.sedmelluq.discord.lavaplayer.tools.FriendlyException;
import com.sedmelluq.discord.lavaplayer.track.AudioPlaylist;
import com.sedmelluq.discord.lavaplayer.track.AudioTrack;
import lavaplayer.GuildMusicManager;
import lavaplayer.TrackScheduler;
import moe.argus.git.Utility.SearchUtil;
import moe.argus.git.Utility.Utility;
import moe.argus.git.Utility.VideoSelection;
import moe.argus.git.database.Database;
import org.apache.commons.lang3.time.DurationFormatUtils;
import org.json.JSONException;
import sx.blah.discord.api.events.EventSubscriber;
import sx.blah.discord.handle.impl.events.guild.channel.message.MessageReceivedEvent;
import sx.blah.discord.handle.impl.events.guild.voice.user.UserVoiceChannelLeaveEvent;
import sx.blah.discord.handle.impl.events.guild.voice.user.UserVoiceChannelMoveEvent;
import sx.blah.discord.handle.obj.*;
import sx.blah.discord.util.EmbedBuilder;

import java.sql.Connection;
import java.util.*;

public class CommandHandler {

    // A static map of commands mapping from command string to the functional impl
    private static Map<CommandKey, Command> commandMap= new HashMap<>();

    private static final AudioPlayerManager playerManager = new DefaultAudioPlayerManager();
    private static Map<Long, GuildMusicManager> musicManagers = new HashMap<>();
    private static Connection connection;

    /*public void connectToDb(){
        try {
            connection = DriverManager.getConnection("");
        } catch (SQLException e) {
            System.out.println("FAILED TO CONNECT TO DB");
            e.printStackTrace();
        }
    }*/

    private static Command joinVc = (event, args) -> {
        IVoiceChannel userVoiceChannel = event.getAuthor().getVoiceStateForGuild(event.getGuild()).getChannel();

        if(userVoiceChannel == null) {
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not connected to a voice channel!").build());
            return;
        }

        userVoiceChannel.join();
        getGuildAudioPlayer(event.getGuild());//add new audiomanager to Map but dont actually save it anywhere else rn.
    };



    private static Command queueList = (event, args) -> {
        GuildMusicManager musicManager = getGuildAudioPlayer(event.getGuild());
        AudioPlayer player = musicManager.getScheduler().getPlayer();
        List<AudioTrack> queue = musicManager.getScheduler().getQueue();
        if(queue.size() == 0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "No songs in playlist").build());
            return;
        }

        EmbedBuilder builder = new EmbedBuilder();
        builder.withColor(Utility.PurpleEmbed);
        builder.withTitle(Utility.SuccessLevelEmoji[4]+" Queue");
        long npDur =player.getPlayingTrack().getDuration()/1000;
        long npMins = npDur/60;
        long npSec = npDur%60;

        builder.appendField("Now playing by "+player.getPlayingTrack().getInfo().author, "["+String.format("%d:%02d", npMins, npSec)+"] - **[" +player.getPlayingTrack().getInfo().title+"]("+player.getPlayingTrack().getInfo().uri+")**" , false);

        for (int i = 0; i <(queue.size() < 10 ? queue.size(): 10); i++){
            long newDur =queue.get(i).getDuration()/1000;
            long newMins = newDur/60;
            long newSecs = newDur%60;
            builder.appendField("#"+(i+1)+" by "+queue.get(i).getInfo().author, "["+String.format("%d:%02d", newMins, newSecs)+"] - **[" +queue.get(i).getInfo().title+"]("+queue.get(i).getInfo().uri+")**" , false);
        }
        long totalMillis = 0;
        for (AudioTrack track: queue) {
            totalMillis += track.getDuration();
        }
        String hms = DurationFormatUtils.formatDurationHMS(totalMillis);
        hms = hms.substring(0, hms.indexOf('.'));
        builder.appendField(queue.size()+" songs in queue", "["+hms+"] total playtime", false);

        Utility.sendMessage(event.getChannel(),"", builder.build());
    };

    private static Command soraSystem = ((event, args) -> {
        Runtime runtime = Runtime.getRuntime();
        float allocatedRamUse = Math.round((runtime.totalMemory() - runtime.freeMemory()) / 1048576F); //java programm itself is using

        String uptime = DurationFormatUtils.formatDurationHMS(System.currentTimeMillis() - Utility.StartTime);
        uptime = uptime.substring(0, uptime.indexOf('.'));

        EmbedBuilder builder = new EmbedBuilder();
        builder.withTitle(Utility.SuccessLevelEmoji[3]+" Sora Music Stats");
        builder.withColor(Utility.BlueInfoEmbed);
        builder.withThumbnail(event.getClient().getApplicationIconURL());
        int playingFor = 0;
        int queueLength = 0;
        for (GuildMusicManager manager:
             musicManagers.values()) {
            if(!manager.getScheduler().getPlayer().isPaused() && manager.getScheduler().getPlayer().getPlayingTrack()!= null)
                playingFor++;
            queueLength += manager.getScheduler().getQueue().size();
            if(manager.getScheduler().getPlayer().getPlayingTrack() != null)
                queueLength++;
        }
        builder.appendField("Uptime", uptime, true);
        builder.appendField("Used RAM", allocatedRamUse + " mB", true);
        builder.appendField("Playing music for",playingFor+" "+(playingFor == 1 ? "guild" : "guilds"), true);
        builder.appendField("Total queue length",queueLength+"", true);

        Utility.sendMessage(event.getChannel(), "" ,builder.build());
        });

    private static Command nowPlaying = (event, args)->{
        GuildMusicManager musicManager = getGuildAudioPlayer(event.getGuild());
        AudioPlayer player = musicManager.getScheduler().getPlayer();

        if(player.getPlayingTrack() == null){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "Nothing is playing").build());
            return;
        }

        if(!player.getPlayingTrack().getInfo().isStream) {
            long npDur = player.getPlayingTrack().getDuration() / 1000;
            long npMins = npDur / 60;
            long npSec = npDur % 60;

            float percentageDone = (100.0f / player.getPlayingTrack().getDuration()) * player.getPlayingTrack().getPosition();
            int rounded = (int) Math.floor(percentageDone / 10);
            String progressStatus = "";
            for (int i = 0; i < 10; i++) {
                if (i == rounded) {
                    progressStatus += " :red_circle: ";
                    continue;
                }
                progressStatus += "▬";
            }
            long posDur = player.getPlayingTrack().getPosition() / 1000;
            long posMins = posDur / 60;
            long posSec = posDur % 60;

            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                            "Currently playing by " + player.getPlayingTrack().getInfo().author)
                            .withDescription("**[" + player.getPlayingTrack().getInfo().title + "](" + player.getPlayingTrack().getInfo().uri + ")**")
                            .appendField("Progress", "[" + String.format("%d:%02d", posMins, posSec) + "] " + progressStatus + " [" + String.format("%d:%02d", npMins, npSec) + "]", false).build());
        }else{
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                            "Currently playing by " + player.getPlayingTrack().getInfo().author)
                            .withDescription("**[" + player.getPlayingTrack().getInfo().title + "](" + player.getPlayingTrack().getInfo().uri + ")**")
                            .appendField("Progress", "Live...", false).build());
        }
    };

    private static Command pausePlayer = ((event, args) -> {
        if(!userIsInBotChannel(event)){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must be in the same VC as me!").build());
            return;
        }
        GuildMusicManager musicManager = getGuildAudioPlayer(event.getGuild());
        AudioPlayer player = musicManager.getScheduler().getPlayer();
        if(player.isPaused()){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Is already paused or not playing anything!").build());
            return;
        }
        player.setPaused(true);
        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Paused player").build());
    });

    private static Command playPlayer = ((event, args) -> {
        if(!userIsInBotChannel(event)){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must be in the same VC as me!").build());
            return;
        }
        GuildMusicManager musicManager = getGuildAudioPlayer(event.getGuild());
        AudioPlayer player = musicManager.getScheduler().getPlayer();
        if(!player.isPaused()){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Player isn't paused").build());
            return;
        }
        player.setPaused(false);
        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Player unpaused!").build());
    });

    private static Command clearList = (event, args) -> {
        if(!userIsInBotChannel(event)){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must be in the same VC as me!").build());
            return;
        }
        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        scheduler.getQueue().clear();
        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Cleared Queue").build());
    };

    private static Command leaveVc = (event, args) -> {

        if(!checkBotAndSame(event))
            return;
        IVoiceChannel botVoiceChannel = event.getClient().getOurUser().getVoiceStateForGuild(event.getGuild()).getChannel();

        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        scheduler.getQueue().clear();
        scheduler.nextTrack();

        botVoiceChannel.leave();
    };

    private static Command exportPlaylist = (event, args) -> {
        GuildMusicManager musicManager = getGuildAudioPlayer(event.getGuild());

        if(musicManager.getScheduler().getQueue().size() == 0)
        {
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "This guild has no Queue yet! Add songs!").build());
            return;
        }
        String Urls = "";
        if(musicManager.getScheduler().getPlayer().getPlayingTrack()!= null)
            Urls=musicManager.getScheduler().getPlayer().getPlayingTrack().getInfo().uri+"\n";

        for (AudioTrack track:musicManager.getScheduler().getQueue()) {
            Urls+= track.getInfo().uri+"\n";
        }

        try {
            HttpResponse<JsonNode> response = Unirest.post("https://hastebin.com/documents").body(Urls).asJson();
            String key = response.getBody().getObject().get("key").toString();
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Click here to get playlist").withUrl("https://hastebin.com/"+key+".sora").withDescription("You can later import this queue using the import command and give it this link").build());
        } catch (UnirestException e) {
            e.printStackTrace();
        }
    };

    private static Command importPlaylist = (event, args) -> {
        if(args.size() == 0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Add the hastebin URL!").build());
            return;
        }

        if(args.size() > 1){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Only add 1 hastebin URL!").build());
            return;
        }
        if(!checkBotAndSame(event))
            return;

        String url = args.get(0);
        if(!url.startsWith("https://hastebin.com/")){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Must be a valid hastebin URL!").build());
            return;
        }
        if(!url.endsWith(".sora") && !url.endsWith(".fredboat")){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Must be a playlist exported by Sora or Fredboat to minimize errors!").build());
            return;
        }

        url = url.replace("https://hastebin.com/", "https://hastebin.com/raw/");
        try {
            HttpResponse<String> response = Unirest.get(url).asString();
            if(response.getBody().contains("Document not found.")){
                Utility.sendMessage(event.getChannel(), "",
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to get playlist! Make sure its a valid link!").build());
                return;
            }
            String[] urlsFromUser = response.getBody().split("\n");
            for (String urlFromUser:urlsFromUser) {
                importSongs(event.getChannel(), urlFromUser);
            }

            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Importing playlist. This will take a while.").build());

        } catch (UnirestException e) {
            e.printStackTrace();
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to get playlist! Make sure its a valid link!").build());
            return;
        }
    };

    private static Command listenMoe = (event, args) -> {
        if(!checkBotAndSame(event))
            return;
        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        scheduler.getQueue().clear();
        scheduler.nextTrack();
        loadAndPlay(event.getChannel(), "https://listen.moe/stream");
        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Successfully started listen.moe stream").build());
        return;
    };

    private static Command changeVolume = (event, args) -> {
        if(!checkBotAndSame(event))
            return;

        if(args.size() ==0 || args.size() > 1){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Please only add one number from 0-100").build());
            return;
        }

        if(!Utility.tryParseInt(args.get(0))){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Must be a whole number from 0-100").build());
            return;
        }

        int volume = Integer.parseInt(args.get(0));

        if(volume > 100)
            volume = 100;
        if(volume <1)
            volume = 1;

        AudioPlayer player =  getGuildAudioPlayer(event.getGuild()).getScheduler().getPlayer();
        player.setVolume(volume);

        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Set volume to "+volume).build());
    };

    private static Command playAndLoad = (event, args) -> {

        if(!checkBotAndSame(event))
            return;

        if(args.size() ==0 || args.size() > 1){
            if(args.size() ==0 && getGuildAudioPlayer(event.getGuild()).getScheduler().getPlayer().isPaused()){
                Utility.sendMessage(event.getChannel(), "",
                        Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Unpaused player.").build());
                return;
            }
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Add only 1 URL to play from!").build());
            return;
        }


        // Turn the args back into a string separated by space
        String searchStr = String.join(" ", args);

        loadAndPlay(event.getChannel(), searchStr);
    };

    private static Command shufflePlaylist = (event, args) -> {
        if(!checkBotAndSame(event))
            return;

        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        scheduler.shuffle();

        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Shuffled entire Queue").build());
    };

    private static Command repeatSong = (event, args) -> {
        if(!checkBotAndSame(event))
            return;
        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        boolean repeat = scheduler.toggleRepeatSong();

        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Repeating current song is "+(repeat ? "ON": "OFF")).build());
    };

    private static Command select = (event, args) -> {
        if(!checkBotAndSame(event))
            return;

        if(args.size() != 1){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Add whole number of which track to select!").build());
            return;
        }

        if(!Utility.tryParseInt(args.get(0))){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Add whole number of which track to select!").build());
            return;
        }
        GuildMusicManager manager = getGuildAudioPlayer(event.getGuild());

        if(!manager.selections.containsKey(event.getAuthor().getLongID())){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "There is no selection for you!").build());
            return;
        }

        int chosen = Integer.parseInt(args.get(0));
        int index = chosen-1;

        VideoSelection videoSel = manager.selections.get(event.getAuthor().getLongID());

        List<AudioTrack> selection = videoSel.getChoices();

        if(index >selection.size()-1 || index < 0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "The index you provided doesn't correspond to any possible selection!").build());
            return;
        }

        AudioTrack track = selection.get(index);


        manager.getScheduler().queue(track);

        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "Added to Queue").withDescription("**["+track.getInfo().title+"]("+track.getInfo().uri+")**").build());

        videoSel.getMsg().delete();
        manager.selections.remove(event.getAuthor().getLongID());
    };

    private static Command soundcloudSearch = (event, args) -> {
        if(!checkBotAndSame(event))
            return;

        if(args.size() ==0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You have to add the name of the Song you are searching...").build());
            return;
        }

        String search = String.join(" ", args);
        searchForVideos(event, search, "scsearch:");
    };

    private static Command ytSearch = (event, args) -> {
        if(!checkBotAndSame(event))
            return;

        if(args.size() ==0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You have to add the name of the Song you are searching...").build());
            return;
        }

        String search = String.join(" ", args);
        searchForVideos(event, search, "ytsearch:");
    };

    private static Command repeatPlaylist = (event, args) -> {
        if(!checkBotAndSame(event))
            return;
        TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
        boolean repeat = scheduler.toggleRepeatPlaylist();

        Utility.sendMessage(event.getChannel(), "",
                Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Repeating playlist is "+(repeat ? "ON": "OFF")).build());
    };

    private static Command skipSong = (event, args) -> {

        if(!checkBotAndSame(event))
            return;

        if(args.size() == 0) {
            skipTrack(event.getChannel(), 1);
            return;
        }
        if(args.size() > 1)
        {
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Only add 1 whole number of how many tracks to skip!").build());
            return;
        }

        if(!Utility.tryParseInt(args.get(0))){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Only add a whole number of how many tracks to skip!").build());
            return;
        }
        int skip = Integer.parseInt(args.get(0));
        skipTrack(event.getChannel(), skip);
    };

    static {

        AudioSourceManagers.registerRemoteSources(playerManager);
        AudioSourceManagers.registerLocalSource(playerManager);

        commandMap.put(new CommandKey(new String[]{"join", "joinvoice"}, true),joinVc);
        commandMap.put(new CommandKey(new String[]{"leave", "leavevoice"}, true),leaveVc);
        commandMap.put(new CommandKey(new String[]{"play", "playsong", "add"}, true),playAndLoad);
        commandMap.put(new CommandKey(new String[]{"skip", "next"}, true),skipSong);
        commandMap.put(new CommandKey(new String[]{"np"}, false),nowPlaying);
        commandMap.put(new CommandKey(new String[]{"musicsys", "msys"}, false),soraSystem);
        commandMap.put(new CommandKey(new String[]{"queue", "list"}, false),queueList);
        commandMap.put(new CommandKey(new String[]{"pause"}, true),pausePlayer);
        commandMap.put(new CommandKey(new String[]{"continue"}, true),playPlayer);
        commandMap.put(new CommandKey(new String[]{"export"}, false),exportPlaylist);
        commandMap.put(new CommandKey(new String[]{"import"}, true),importPlaylist);
        commandMap.put(new CommandKey(new String[]{"clear"}, true),clearList);
        commandMap.put(new CommandKey(new String[]{"listen", "listenmoe", "listen.moe","moe"}, true),listenMoe);
        commandMap.put(new CommandKey(new String[]{"shuffle"}, true),shufflePlaylist);
        commandMap.put(new CommandKey(new String[]{"repeatsong", "replaysong", "rsong"}, true),repeatSong);
        commandMap.put(new CommandKey(new String[]{"repeatplaylist", "replayplaylist", "repeatqueue","rqueue","rplaylist"}, true),repeatPlaylist);
        commandMap.put(new CommandKey(new String[]{"setvolume", "volume", "vol"}, true),changeVolume);
        commandMap.put(new CommandKey(new String[]{"yt", "youtube"}, true),ytSearch);
        commandMap.put(new CommandKey(new String[]{"sc", "soundcloud"}, true),soundcloudSearch);
        commandMap.put(new CommandKey(new String[]{"select"}, true),select);
    }

    private static boolean checkIfDj(MessageReceivedEvent event){
        //IF HE IS ADMIN JUST PROCEED ANYWAY
        if(event.getAuthor().getPermissionsForGuild(event.getGuild()).contains(Permissions.ADMINISTRATOR))
            return true; //do this before even looking at the DB.
        try {
            if(!Database.getInstance().isInDjMode(event.getGuild()))
                return true;
            List<IRole> roles= event.getAuthor().getRolesForGuild(event.getGuild());
            for (IRole role:roles) {
                if(role.getName().equals(Utility.DJ_ROLE_NAME))
                    return true;
            }
        } catch (Exception e){
            e.printStackTrace();
        }

        Utility.sendMessage(event.getChannel(), "" ,
                Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "This guild locked the music commands. You need the "+Utility.DJ_ROLE_NAME + " role to use them!").build());
        return false;
    }

    private static boolean checkBotAndSame(MessageReceivedEvent event){
        IVoiceChannel botVoiceChannel = event.getClient().getOurUser().getVoiceStateForGuild(event.getGuild()).getChannel();

        if(botVoiceChannel == null) {
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Bot is not connected to any voice channel!").build());
            return false;
        }

        if(!userIsInBotChannel(event)){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must be in the same VC as me!").build());
            return false;
        }
        return true;
    }

    private static void importSongs(final IChannel channel, final String trackUrl){
        GuildMusicManager musicManager = getGuildAudioPlayer(channel.getGuild());
        playerManager.loadItemOrdered(musicManager, trackUrl, new AudioLoadResultHandler() {
            @Override
            public void trackLoaded(AudioTrack audioTrack) {
                play(musicManager, audioTrack);
            }

            @Override
            public void playlistLoaded(AudioPlaylist audioPlaylist) {
                for (AudioTrack track:audioPlaylist.getTracks()) {
                    play(musicManager, track);
                }
            }

            @Override
            public void noMatches() {

            }

            @Override
            public void loadFailed(FriendlyException e) {
            }
        });
    }

    private static boolean userIsInBotChannel(MessageReceivedEvent event){
        IVoiceChannel botVoiceChannel = event.getClient().getOurUser().getVoiceStateForGuild(event.getGuild()).getChannel();
        IVoiceChannel userVoiceChannel = event.getAuthor().getVoiceStateForGuild(event.getGuild()).getChannel();
        if(botVoiceChannel == null || userVoiceChannel == null)
            return false;
        if(botVoiceChannel.getLongID() == userVoiceChannel.getLongID())
            return true;
        return false;
    }

    private static void loadAndPlay(final IChannel channel, final String trackUrl) {
        GuildMusicManager musicManager = getGuildAudioPlayer(channel.getGuild());

        playerManager.loadItemOrdered(musicManager, trackUrl, new AudioLoadResultHandler() {
            @Override
            public void trackLoaded(AudioTrack track) {
                if(!trackUrl.equals("https://listen.moe/stream")) {
                    Utility.sendMessage(channel, "",
                            Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "Added to queue ").withDescription(track.getInfo().title).build());
                }

                play(musicManager, track);
            }

            @Override
            public void playlistLoaded(AudioPlaylist playlist) {
                /*
                for (int i = 0; i< (playlist.getTracks().size() > 100 ? 100 :playlist.getTracks().size()); i++){
                    play(musicManager, playlist.getTracks().get(i));
                }*/

                for (AudioTrack track:playlist.getTracks()) {
                    play(musicManager, track);
                }

                Utility.sendMessage(channel, "",
                        Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "Added playlist").withDescription(playlist.getTracks().size()+" songs were added to the queue!").build());

            }

            @Override
            public void noMatches() {
                Utility.sendMessage(channel, "",
                        Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1], "Nothing found by " + trackUrl).build());

            }

            @Override
            public void loadFailed(FriendlyException exception) {
                Utility.sendMessage(channel, "",
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Could not play: ").withDescription(exception.getMessage()).build());
            }
        });
    }

    private static void skipTrack(IChannel channel, int amount) {
        GuildMusicManager musicManager = getGuildAudioPlayer(channel.getGuild());
        for (int i = 0; i <amount; i++) {
            musicManager.getScheduler().nextTrack();
        }
        if(musicManager.getScheduler().getPlayer().getPlayingTrack() != null) {
            Utility.sendMessage(channel, "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                            "Skipped "+amount+" "+(amount == 1 ? "track":"tracks")).withDescription("Next: "+musicManager.getScheduler().getPlayer().getPlayingTrack().getInfo().title).build());
        }else{
            Utility.sendMessage(channel, "",
                    Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4],
                            "Skipped "+amount+(amount == 1 ? " track":" tracks")+" and paused since there are no further tracks").build());
        }
    }

    private static void play(GuildMusicManager musicManager, AudioTrack track) {

        musicManager.getScheduler().queue(track);
    }

    private static synchronized GuildMusicManager getGuildAudioPlayer(IGuild guild) {
        long guildId = guild.getLongID();
        GuildMusicManager musicManager = musicManagers.get(guildId);

        if (musicManager == null) {
            musicManager = new GuildMusicManager(playerManager);
            musicManagers.put(guildId, musicManager);
        }

        guild.getAudioManager().setAudioProvider(musicManager.getAudioProvider());

        return musicManager;
    }

    private static void searchForVideos(MessageReceivedEvent event, String search, String prefix){
        //Matcher m = Pattern.compile("\\S+\\s+(.*)").matcher(search);
        //m.find();
        //String query = m.group(1);
        String query = search;
        //now remove all punctuation
        query = query.replaceAll("[.,/#!$%\\^&*;:{}=\\-_`~()]", "");
        AudioPlaylist list;
        try {
            list = SearchUtil.searchForTracks(query, prefix);
        }catch (JSONException e){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Something went terribly wrong ;_;").build());
            System.out.println("Search exception \n"+ e);
            return;
        }

        if(list == null || list.getTracks().size() == 0){
            Utility.sendMessage(event.getChannel(), "",
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find anything.").build());
        } else {
            //Get at most 10 tracks
            List<AudioTrack> selectable = list.getTracks().subList(0, Math.min(10,list.getTracks().size()));
            GuildMusicManager manager = getGuildAudioPlayer(event.getGuild());

            if(manager.selections.containsKey(event.getAuthor().getLongID())){
                manager.selections.remove(event.getAuthor().getLongID());
            }



            EmbedBuilder builder = new EmbedBuilder();
            builder.withColor(Utility.BlueInfoEmbed);
            builder.withAuthorIcon((event.getAuthor().getAvatarURL() == null ?  Utility.StandardDiscordAvatar : event.getAuthor().getAvatarURL()));
            builder.withAuthorName(event.getAuthor().getName()+"#"+event.getAuthor().getDiscriminator());
            builder.withTitle("Search Results");
            builder.withDescription("Use `"+Database.getInstance().getPrefix(event.getGuild())+"select IndexOfSong` to select");

            int i = 1;
            for (AudioTrack track:selectable) {
                long npDur = track.getDuration() / 1000;
                long npMins = npDur / 60;
                long npSec = npDur % 60;

                builder.appendField("#"+i+" by "+track.getInfo().author, "["+String.format("%d:%02d", npMins, npSec)+"] **["+track.getInfo().title+"]("+track.getInfo().uri+")**", false);
                i++;
            }

            IMessage msg = event.getChannel().sendMessage("", builder.build());
            manager.selections.put(event.getAuthor().getLongID(), new VideoSelection(selectable, msg));
        }
    }

    @SuppressWarnings("Duplicates")
    @EventSubscriber
    public void onUserVoiceChannelMoveEvent(UserVoiceChannelMoveEvent event){
        IVoiceChannel botVoiceChannel = event.getClient().getOurUser().getVoiceStateForGuild(event.getGuild()).getChannel();

        if(botVoiceChannel == null) {
            return;
        }

        if(event.getOldChannel().getLongID() != botVoiceChannel.getLongID())
            return;


        List<IUser> connectedUsers = event.getOldChannel().getConnectedUsers();

        if(connectedUsers.size() == 1){
            TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
            scheduler.getQueue().clear();
            scheduler.nextTrack();

            botVoiceChannel.leave();
        }
        boolean allBots = true;

        for (IUser user:connectedUsers) {
            if(!user.isBot()) {
                allBots = false;
                break;
            }

        }
        if(allBots){
            TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
            scheduler.getQueue().clear();
            scheduler.nextTrack();

            botVoiceChannel.leave();
        }
    }

    @SuppressWarnings("Duplicates")
    @EventSubscriber
    public void onUserVoiceChannelLeaveEvent(UserVoiceChannelLeaveEvent event){

        IVoiceChannel botVoiceChannel = event.getClient().getOurUser().getVoiceStateForGuild(event.getGuild()).getChannel();

        if(botVoiceChannel == null) {
            return;
        }
        IVoiceChannel userVoiceChannel = event.getVoiceChannel();

        if(botVoiceChannel.getLongID() != userVoiceChannel.getLongID())
            return;

        List<IUser> connectedUsers = userVoiceChannel.getConnectedUsers();

        if(connectedUsers.size() == 1){
            TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
            scheduler.getQueue().clear();
            scheduler.nextTrack();

            botVoiceChannel.leave();
        }
        boolean allBots = true;

        for (IUser user:connectedUsers) {
            if(!user.isBot()) {
                allBots = false;
                break;
            }

        }
        if(allBots){
            TrackScheduler scheduler = getGuildAudioPlayer(event.getGuild()).getScheduler();
            scheduler.getQueue().clear();
            scheduler.nextTrack();

            botVoiceChannel.leave();
        }

    }


    @EventSubscriber
    public void onMessageReceived(MessageReceivedEvent event){
        // Note for error handling, you'll probably want to log failed commands with a logger or sout
        // In most cases it's not advised to annoy the user with a reply incase they didn't intend to trigger a
        // command anyway, such as a user typing ?notacommand, the bot should not say "notacommand" doesn't exist in
        // most situations. It's partially good practise and partially developer preference

        String[] argArray = event.getMessage().getContent().split(" ");

        if(argArray.length == 0)
            return;

        String prefix = Database.getInstance().getPrefix(event.getGuild());

        if(!argArray[0].startsWith(prefix))
            return;

        String commandStr = argArray[0].substring(prefix.length()).toLowerCase();

        List<String> argList = new ArrayList<>(Arrays.asList(argArray));
        argList.remove(0);//remove the command

        // Instead of delegating the work to a switch, automatically do it via calling the mapping if it exists
        boolean found = false;
        for(Map.Entry<CommandKey, Command> command : commandMap.entrySet()){
            for (String name:command.getKey().getNames()) {
                //check if the name amtches. Use an if not to reduce code indentation
                if(!name.equalsIgnoreCase(commandStr)){
                    continue;
                }
                //make sure found == true to not keep iterating
                found = true;
                //Name matches. Check if it is a DJ command
                if(command.getKey().isDjRestricted()){
                    //it is a DJ command
                    //if the user has no access just exit the loop (causing found to stay false)
                    if(!checkIfDj(event)){
                        break;
                    }
                }
                //try to run the command
                try {
                    command.getValue().runCommand(event, argList);
                } catch (Exception e){
                    e.printStackTrace();
                }
                //exit out of the loop since we finished business
                break;
            }
            if(found){
                break;
            }
        }
        //OLD
        /*
        if(commandMap.containsKey(commandStr)) {
            try {
                commandMap.get(commandStr).runCommand(event, argList);
            }catch (Exception e){
                e.printStackTrace();
            }
        }*/
    }

}
