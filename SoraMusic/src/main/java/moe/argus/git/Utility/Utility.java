package moe.argus.git.Utility;


import org.apache.commons.lang3.StringUtils;
import sx.blah.discord.api.internal.json.objects.EmbedObject;
import sx.blah.discord.handle.obj.IChannel;
import sx.blah.discord.util.DiscordException;
import sx.blah.discord.util.EmbedBuilder;
import sx.blah.discord.util.RequestBuffer;

import java.awt.*;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Properties;

public class Utility {

    public static String BOT_PREFIX = ">";
    public static String DJ_ROLE_NAME = "Sora-DJ";

    public static Color PurpleEmbed = new Color(109,41,103);
    public static Color YellowWarningEmbed = new Color(255,204,77);
    public static Color GreenSuccessEmbed = new Color(119,178,85);
    public static Color RedFailiureEmbed= new Color(221,46,68);
    public static Color BlueInfoEmbed = new Color(59,136,195);

    public static String StandardDiscordAvatar = "http://i.imgur.com/tcpgezi.jpg";

    public static long StartTime;


    public static Properties properties = new Properties();

    public static String[] SuccessLevelEmoji = new String[]
            {
                    "✅","⚠","❌","ℹ","\uD83C\uDFB5", ""
            };

    public static void sendMessage(IChannel channel, String message, EmbedObject eb ){

        RequestBuffer.request(()->{
            try {
                if(StringUtils.isBlank(message) && eb == null)
                    return;
                else if(eb==null)
                    channel.sendMessage(message);
                else if(StringUtils.isBlank(message))
                    channel.sendMessage(eb);
                else
                    channel.sendMessage(message,eb);
            } catch (DiscordException e){
                System.err.println("Message could not be send with error: ");
                e.printStackTrace();
            }
        });
    }

    public static void loadConfig(){
        try (FileInputStream fis = new FileInputStream("config.properties")){
            properties.load(fis);
        } catch (IOException e) {
            e.printStackTrace();
            System.out.println("COULDN'T FIND config.properties FILE!");
            System.exit(-1);
        }
    }

    public static boolean isBetween(int x, int lower, int upper){
        return lower<= x && x<= upper;
    }

    public static boolean tryParseInt(String value){
        try {
            Integer.parseInt(value);
            return true;
        }catch (NumberFormatException e){
            return false;
        }
    }

    public static EmbedBuilder ResultFeedback(Color color, String symbol, String text){

        EmbedBuilder builder = new EmbedBuilder();
        builder.withColor(color);
        builder.withTitle(String.format("%s %s", symbol, text));
        return builder;
    }
}
