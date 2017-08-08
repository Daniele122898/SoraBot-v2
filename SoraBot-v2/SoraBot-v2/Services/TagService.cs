using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class TagService
    {
        public async Task CreateTag(SocketCommandContext context, SoraContext soraContext, string name, string value)
        {
            //TODO CHECK PERMS
            
            //Find 
            
            //Check if already exists
            var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);

            if (guildDb.Tags.Any(x => x.Name == name))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "A tag with that name already exists in this guild!"));
            }
            
            //
        }
    }
}