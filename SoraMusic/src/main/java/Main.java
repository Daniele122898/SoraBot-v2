import moe.argus.git.commands.CommandHandler;
import moe.argus.git.Utility.Utility;
import sx.blah.discord.api.ClientBuilder;
import sx.blah.discord.api.IDiscordClient;

public class Main {

    public static void main(String[] args){
        Utility.loadConfig();
        IDiscordClient client = new ClientBuilder()
                .withToken(Utility.properties.getProperty("token"))
                .withRecommendedShardCount()
                .build();

        CommandHandler handler = new CommandHandler();
        //handler.connectToDb();
        // Register a listener via the EventSubscriber annotation which allows for organisation and delegation of events
        client.getDispatcher().registerListener(handler);


        // Only login after all events are registered otherwise some may be missed.
        client.login();
    }
}
