﻿using System;
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

        public async Task<Option<List<User>>> GetClanMembers(int clanId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var members = await context.Clans
                    .Where(x => x.Id == clanId)
                    .SelectMany(x => x.Members)
                    .Select(x => x.User)
                    .ToListAsync()
                    .ConfigureAwait(false);

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

        public async Task SetClanDescription(string clanId, string description)
        {
            throw new System.NotImplementedException();
        }

        public async Task SetClanAvatar(string avatarUrl)
        {
            throw new System.NotImplementedException();
        }

        public async Task LevelUp(int clanId)
        {
            throw new System.NotImplementedException();
        }

        public async Task ChangeClanName(int clanId, string newName)
        {
            throw new System.NotImplementedException();
        }
    }
}