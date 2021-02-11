using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Extensions;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class ClanRepository : IClanRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public ClanRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Option<Clan>> GetClanById(int clanId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId).ConfigureAwait(false);
                return clan ?? Option.None<Clan>();
            });
        }

        public async Task<Option<Clan>> GetClanByName(string clanName)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var clan = await context.Clans.FirstOrDefaultAsync(x => x.Name == clanName)
                    .ConfigureAwait(false);
                return clan ?? Option.None<Clan>();
            });
        }

        public async Task<Option<List<User>>> GetClanMembers(int clanId, int? limit = null)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var members = (limit.HasValue
                    ? (await context.Clans
                        .Where(x => x.Id == clanId)
                        .SelectMany(x => x.Members)
                        .Select(x => x.User)
                        .OrderByDescending(x => x.Exp)
                        .Take(limit.Value)
                        .ToListAsync()
                        .ConfigureAwait(false))
                    : (await context.Clans
                        .Where(x => x.Id == clanId)
                        .SelectMany(x => x.Members)
                        .Select(x => x.User)
                        .ToListAsync()
                        .ConfigureAwait(false)));

                return members?.Count == 0 ? Option.None<List<User>>() : members;
            });
        }

        public async Task<Option<List<Clan>>> GetTopClans(int limit = 10, int offset = 0) =>
            await _soraTransactor.DoAsync(async context =>
            {
                var clans = await context.Clans
                    .OrderByDescending(x => x.Members
                        .Select(y => y.User)
                        .Sum(y => y.Exp))
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

                return clans?.Count == 0 ? Option.None<List<Clan>>() : clans;
            });

        public async Task<int> GetMemberCount(int clanId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var count = await context.ClanMembers
                    .CountAsync(x => x.ClanId == clanId);

                return count;
            });
        }

        public async Task<bool> DoesClanExistByName(string name) =>
            await _soraTransactor.DoAsync(async context =>
                (await context.Clans
                    .Where(x => x.Name == name)
                    .CountAsync()) > 0);

        public async Task<bool> IsUserInAClan(ulong userId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                return (await context.ClanMembers
                    .Where(x => x.UserId == userId)
                    .CountAsync()) > 0;
            });
        }

        public async Task<bool> IsUserInClan(int clanId, ulong userId) =>
            await _soraTransactor.DoAsync(async context =>
                await context.ClanMembers
                    .CountAsync(x => x.ClanId == clanId && x.UserId == userId)
                > 0);

        public async Task<Option<Clan>> GetClanByUserId(ulong userId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var clan = await context.ClanMembers
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Clan)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                return clan ?? Option.None<Clan>();
            });
        }

        public async Task CreateClan(string name, ulong ownerId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                // Check if user is in clan already
                if (await context.ClanMembers
                    .Where(x => x.UserId == ownerId)
                    .CountAsync() > 0)
                {
                    return;
                }
                // Check if clan with this name already exists, abort if it does
                if (await context.Clans.Where(x => x.Name == name)
                    .CountAsync() > 0)
                {
                    return;
                }
                
                // Now get or create user we attach to the clan
                var _ = await context.Users.GetOrCreateUserNoSaveAsync(ownerId);
                var clan = new Clan()
                {
                    Name = name,
                    OwnerId = ownerId,
                    Created = DateTime.UtcNow,
                    Level = 0
                };

                context.Clans.Add(clan);
                await context.SaveChangesAsync();
                
                // Create clan member for the owner
                var member = new ClanMember()
                {
                    ClanId = clan.Id,
                    UserId = ownerId
                };
                context.ClanMembers.Add(member);
                
                await context.SaveChangesAsync();
            });

        public async Task RemoveClan(int clanId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;

                context.Clans.Remove(clan);
                await context.SaveChangesAsync();
            });

        public async Task AppointNewOwner(int clanId, ulong userId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;

                clan.OwnerId = userId;
                await context.SaveChangesAsync();
            });

        public async Task SetClanDescription(int clanId, string description) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;

                if (description?.Length > 110)
                    description = description.Substring(0, 110);
                clan.Description = description;
                
                await context.SaveChangesAsync();
            });

        public async Task SetClanAvatar(int clanId, string avatarUrl) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;
                // Assume we already did url sanity checks
                clan.AvatarUrl = avatarUrl;
                await context.SaveChangesAsync();
            });

        public async Task LevelUp(int clanId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;

                clan.Level += 1;
                await context.SaveChangesAsync();
            });

        public async Task ChangeClanName(int clanId, string newName) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;
                
                // Check if clan with this name already exists, abort if it does
                if (await context.Clans.Where(x => x.Name == newName)
                    .CountAsync() > 0)
                {
                    return;
                }

                clan.Name = newName;
                await context.SaveChangesAsync();
            });

        public async Task UserJoinClan(int clanId, ulong userId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                // Check if user is in a clan already
                if (await context.ClanMembers.CountAsync(x => x.UserId == userId) > 0)
                    return;

                context.ClanMembers.Add(new ClanMember()
                {
                    ClanId = clanId,
                    UserId = userId
                });
                await context.SaveChangesAsync();
            });

        public async Task UserLeaveClan(ulong userId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var member = await context.ClanMembers.FindAsync(userId);
                if (member == null)
                    return;

                context.ClanMembers.Remove(member);
                await context.SaveChangesAsync();
            });

        public async Task InviteUser(int clanId, ulong userId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                // Check if same user was already invited to this clan
                if ((await context.ClanInvites
                         .CountAsync(x => x.ClanId == clanId && x.UserId == userId) > 0))
                    return;
                
                // Otherwise create an invite
                var invite = new ClanInvite()
                {
                    ClanId = clanId,
                    UserId = userId
                };

                context.ClanInvites.Add(invite);
                await context.SaveChangesAsync();

            });

        public async Task<bool> DoesInviteExist(int clanId, ulong userId) =>
            await _soraTransactor.DoAsync(async context => await context.ClanInvites
                .CountAsync(x => x.ClanId == clanId && x.UserId == userId) > 0);

        public async Task RemoveInvite(int clanId, ulong userId) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var invite = await context.ClanInvites
                    .FirstOrDefaultAsync(x => x.ClanId == clanId && x.UserId == userId);

                if (invite == null)
                    return;

                context.ClanInvites.Remove(invite);
                await context.SaveChangesAsync();
            });

        public async Task<Option<List<User>>> GetInvitedUsers(int clanId) =>
            await _soraTransactor.DoAsync(async context =>
            {
                var invites = await context.ClanInvites
                    .Where(x => x.ClanId == clanId)
                    .Select(x => x.User)
                    .ToListAsync();

                return invites?.Count == 0
                    ? Option.None<List<User>>()
                    : invites;
            });

        public async Task<long> GetClanTotalExp(int clanId) =>
            await _soraTransactor.DoAsync(async context =>
            {
                var total = await context.ClanMembers
                    .Where(x => x.ClanId == clanId)
                    .Select(x => x.User)
                    .SumAsync(x => x.Exp);
                return total;
            });
    }
}