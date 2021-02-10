using System.Threading.Tasks;
using Discord.Commands;

namespace SoraBot.Bot.Modules.ClanModule
{
    public partial class ClanModule
    {
        [Command("clancreate"), Alias("createclan")]
        [Summary("Create a clan if you're not already in one")]
        public async Task CreateClan(
            [Summary("Clan name"), Remainder] string name)
        {
            // Check if clan name is too long
            if (name.Length > 25 || name.Length < 2)
            {
                await ReplyFailureEmbed("Name can be no longer than 25 chars and must be more than 2 char long");
                return;
            }

            if (name.Contains("<"))
            {
                await ReplyFailureEmbed("You are not allowed to use '<' in the clan name");
                return;
            }
            
            // Check if user already is in clan
            if (await _clanRepo.IsUserInAClan(Context.User.Id))
            {
                await ReplyFailureEmbed("You already are in a clan. You" +
                                        " must first leave that clan before you can create a new one.");
                return;
            }
            
            // Check if clan with that name already exists
            if (await _clanRepo.DoesClanExistByName(name))
            {
                await ReplyFailureEmbed("Clan with that name already exists. Choose another one!");
                return;
            }
            
            // Otherwise we can create the clan
            await _clanRepo.CreateClan(name, Context.User.Id);
            await ReplySuccessEmbed("Successfully created clan!");
        } 
    }
}