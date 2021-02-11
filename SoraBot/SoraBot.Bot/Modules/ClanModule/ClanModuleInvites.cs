using System;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Bot.Modules.ClanModule
{
    public partial class ClanModule
    {
        [Command("claninvite"), Alias("cinvite")]
        [Summary("Invite the @mentioned user into the clan")]
        public async Task InviteUser(
            [Summary("@user to ivnite")] DiscordUser user)
        {
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            if (await _clanRepo.DoesInviteExist(clan.Id, user.User.Id))
            {
                await ReplyFailureEmbed("User has already been invited to the clan");
                return;
            }

            // Otherwise we can invite him
            await _clanRepo.InviteUser(clan.Id, user.User.Id);
            await ReplySuccessEmbed("Successfully invited user");

            try
            {
                // Try and DM the user. This is in a try block bcs he could not accept DMs
                await (await user.User.GetOrCreateDMChannelAsync())
                    .SendMessageAsync(embed: SimpleEmbed(
                            Blue,
                            $"You have been invited to join {clan.Name}",
                            INFO_EMOJI).WithDescription(
                            $"Use `acceptinvite {clan.Name}` command to accept")
                        .Build()
                    );
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        [Command("acceptinvite"), Alias("accinv")]
        [Summary("Accept an invite from a clan")]
        public async Task AcceptInvite(
            [Summary("Name of clan"), Remainder] string name)
        {
            var clan = await _clanRepo.GetClanByName(name);
            if (!clan)
            {
                await ReplyFailureEmbed("Clan does not exist. " +
                                        "Make sure to spell it correctly");
                return;
            }
            
            // Clan exists. Check if invite exists
            if (!await _clanRepo.DoesInviteExist(clan.Some().Id, Context.User.Id))
            {
                await ReplyFailureEmbed("You do not have an invite from this clan.");
                return;
            }
            
            // Check if user is already in a clan
            if (await _clanRepo.IsUserInAClan(Context.User.Id))
            {
                await ReplyFailureEmbed("You already are in a clan. You must first leave to join another one.");
                return;
            }
            
            // Invite exists and user is in no clan so accept and join
            await _clanRepo.RemoveInvite(clan.Some().Id, Context.User.Id);
            await _clanRepo.UserJoinClan(clan.Some().Id, Context.User.Id);
            await ReplySuccessEmbed("Successfully joined clan");
        }
    }
}