using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Utils;
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

            try
            {
                var owner = await _userService.GetOrSetAndGet(clan.Some().OwnerId);
                if (!owner)
                    return;
                await (await owner.Some().GetOrCreateDMChannelAsync())
                    .SendMessageAsync(embed: SimpleEmbed(
                            Blue,
                            $"{Formatter.UsernameDiscrim(Context.User)} has accepted your invite",
                            INFO_EMOJI)
                        .Build());
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        [Command("issuedinvites"), Alias("invitelist")]
        [Summary("Shows the invite list of your clan")]
        public async Task ShowInviteList()
        {
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            var invites = await _clanRepo.GetInvitedUsers(clan.Id);

            if (!invites)
            {
                await ReplyFailureEmbed("No invites found. Invite someone!");
                return;
            }

            var eb = new EmbedBuilder()
            {
                Footer = RequestedByMe(),
                Color = Blue,
                Title = $"{INFO_EMOJI} Clan Invites",
            };
            if (!string.IsNullOrWhiteSpace(clan.AvatarUrl))
                eb.WithThumbnailUrl(clan.AvatarUrl);

            if ((~invites).Count > 22)
                eb.WithDescription("More than 22 active invites. I can only show you the 22 oldest ones. " +
                                   "Remove some to see the newer ones.");
            var invs = invites.Some();
            int count = Math.Min(invs.Count, 22);
            for (int i = 0; i < count; i++)
            {
                var invite = invs[i];
                var user = await _userService.GetOrSetAndGet(invite.Id);
                string username = user
                    ? Formatter.UsernameDiscrim(user.Some())
                    : "_Unknown_";
                eb.AddField(x =>
                {
                    x.Name = username;
                    x.IsInline = true;
                    x.Value = $"_ID: {invite.Id.ToString()}_";
                });
            }

            await ReplyEmbed(eb);
        }

        [Command("removeinvite"), Alias("rminvite")]
        [Summary("Remove the invite for a user id")]
        public async Task RemoveInviteFromUser(ulong userid)
        {
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            if (!await _clanRepo.DoesInviteExist(clan.Id, userid))
            {
                await ReplyFailureEmbed("Invite for this user does not exist");
                return;
            }

            await _clanRepo.RemoveInvite(clan.Id, userid);
            await ReplySuccessEmbed("Invite has been revoked");
        }

        [Command("leaveclan")]
        [Summary("Leave your clan. If you are the owner it will transfer ownership to the user with the highest EXP.")]
        public async Task LeaveClan()
        {
            var clan = await _clanRepo.GetClanByUserId(Context.User.Id);
            if (!clan)
            {
                await ReplyFailureEmbed("You are not in a clan");
                return;
            }

            bool owner = clan.Some().OwnerId == Context.User.Id;
            var eb = SimpleEmbed(
                Green, "Successfully left clan",
                SUCCESS_EMOJI);
            if (owner)
            {
                // Transfer ownership before we leave the clan
                // There must be members because we are one.
                var topMember = (await _clanRepo.GetClanMembers(clan.Some().Id, 2)).Some()
                    .Find(x => x.Id != Context.User.Id);
                if (topMember == null)
                {
                    // This means we are the only one in the clan so we will have to remove it
                    await _clanRepo.RemoveClan(clan.Some().Id);
                    await ReplyEmbed(eb.WithDescription("Since you where the last member the clan has been deleted"));
                    return;
                }

                // Otherwise we have a new owner
                await _clanRepo.AppointNewOwner(clan.Some().Id, topMember.Id);
                var user = await _userService.GetOrSetAndGet(topMember.Id);
                string username = user.HasValue
                    ? Formatter.UsernameDiscrim(user.Some())
                    : topMember.Id.ToString();
                eb.WithDescription(
                    $"Since you where the owner I appointed {username} as the next owner :)");

                // Send info so the new owner knows about this lol
                if (user)
                {
                    try
                    {
                        await (await user.Some().GetOrCreateDMChannelAsync())
                            .SendMessageAsync(embed: SimpleEmbed(
                                Blue,
                                $"You have been appointed as the new owner of {clan.Some().Name}",
                                INFO_EMOJI).Build());
                    }
                    catch (Exception e)
                    {
                        /* ignored */
                    }
                }
            }

            await _clanRepo.UserLeaveClan(Context.User.Id);
            await ReplyEmbed(eb);
        }

        [Command("kickuser"), Alias("clankick")]
        [Summary("Kick a user off your clan")]
        public async Task KickUser(
            [Summary("Id of the user")] ulong userId)
        {
            await KickUserImpl(userId);
        }

        [Command("kickuser"), Alias("clankick")]
        [Summary("Kick a user off your clan")]
        public async Task KickUser(
            [Summary("@mention of user")] DiscordUser user)
        {
            await KickUserImpl(user.User.Id);
        }

        private async Task KickUserImpl(ulong userId)
        {
            if (!(await GetClanIfExistsAndOwner() is Clan clan))
                return;

            // Check if user is even a member
            if (!await _clanRepo.IsUserInClan(clan.Id, userId))
            {
                await ReplyFailureEmbed("User is not a member of your clan");
                return;
            }

            // Otherwise remove him
            await _clanRepo.UserLeaveClan(userId);
            await ReplySuccessEmbed("Successfully kicked user");

            try
            {
                var user = await _userService.GetOrSetAndGet(userId);
                if (!user)
                    return;
                await (await user.Some().GetOrCreateDMChannelAsync())
                    .SendMessageAsync(embed: SimpleEmbed(
                        Blue,
                        $"You have been kicked from {clan.Name}",
                        INFO_EMOJI).Build());
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}