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

        public async Task<int> GetMemberCount(int clanId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var count = await context.Clans
                    .Where(x => x.Id == clanId)
                    .Select(x => x.Members.Count)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

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
            });

        public async Task SetClanDescription(int clanId, string description) =>
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var clan = await context.Clans.FindAsync(clanId);
                if (clan == null)
                    return;

                if (description.Length > 110)
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
                
                
            });

        public Task UserLeaveClan(ulong userId)
        {
            throw new NotImplementedException();
        }

        public Task InviteUser(int clanId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveInvite(int clanId, ulong userId)
        {
            throw new NotImplementedException();
        }
    }
}