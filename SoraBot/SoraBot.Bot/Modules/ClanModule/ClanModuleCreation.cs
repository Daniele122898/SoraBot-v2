using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Bot.Modules.ClanModule
{
    public partial class ClanModule
    {
        private const int _LEVEL_UP_COST = 7500;
        
        [Command("clanrename"), Alias("renameclan")]
        [Summary("Rename your clan")]
        public async Task RenameClan(
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
            
            // Check if clan with that name already exists
            if (await _clanRepo.DoesClanExistByName(name))
            {
                await ReplyFailureEmbed("Clan with that name already exists. Choose another one!");
                return;
            }
            
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;
            
            await _clanRepo.ChangeClanName(clan.Id, name);
            await ReplySuccessEmbed("Successfully renamed clan!");
        } 
        
        [Command("clanlevelup"), Alias("levelupcaln", "1up")]
        [Summary("Level up clan by paying 7'500 SC.")]
        public async Task LevelUpClan()
        {
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            if (_coinRepository.GetCoins(Context.User.Id) < _LEVEL_UP_COST)
            {
                await ReplyFailureEmbed($"You do not have enough Sora coins. " +
                                        $"Upgrading costs {_LEVEL_UP_COST.ToString()} SC");
                return;
            }

            await _clanRepo.LevelUp(clan.Id);
            await ReplySuccessEmbed("Successfully leveled up clan");
        }
        
        [Command("clanavatar"), Alias("clanicon")]
        [Summary("Set the clan description")]
        public async Task SetClanAvatar(
            [Summary("Clan avatar. Leave blank to remove it"), Remainder]
            string avatar = null)
        {
            if (avatar != null && (
                !Helper.UrlValidUri(avatar) || Helper.LinkIsNoImage(avatar)))
            {
                await ReplyFailureEmbed("Avatar must point to an image!");
                return;  
            } 
            
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            await _clanRepo.SetClanAvatar(clan.Id, avatar);
            await ReplySuccessEmbed("Successfully set avatar");
        }
        
        [Command("clandescription"), Alias("clandesc")]
        [Summary("Set the clan description")]
        public async Task SetClanDescription(
            [Summary("Clan description. Leave blank to remove it"), Remainder]
            string desc = null)
        {
            if (desc?.Length > 250)
            {
                await ReplyFailureEmbed("Clan description cannot be longer than 250 characters");
                return;  
            } 
            
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            await _clanRepo.SetClanDescription(clan.Id, desc);
            await ReplySuccessEmbed("Successfully set description");
        }

        private async Task<Clan> GetClanIfExistsAndOwner()
        {
            var clan = await _clanRepo.GetClanByUserId(Context.User.Id);
            if (!clan)
            {
                await ReplyFailureEmbed("You are not in a clan");
                return null;
            }

            if (clan.Some().OwnerId != Context.User.Id)
            {
                await ReplyFailureEmbed("You are not the clan owner");
                return null;
            }

            return ~clan;
        }
        
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