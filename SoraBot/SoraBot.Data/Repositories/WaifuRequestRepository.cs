using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Dtos;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class WaifuRequestRepository : IWaifuRequestRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public WaifuRequestRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Option<List<WaifuRequest>>> GetUserWaifuRequests(ulong userId)
            => await _soraTransactor.DoAsync<Option<List<WaifuRequest>>>(async context =>
            {
                var requests = await context.WaifuRequests
                    .Where(x => x.UserId == userId)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (requests.Count == 0)
                    return Option.None<List<WaifuRequest>>();

                return requests;
            }).ConfigureAwait(false);

        public async Task<bool> UserHasNotificationOn(ulong userId)
            => await _soraTransactor.DoAsync(async context =>
                    await context.UserNotifiedOnRequestProcesses.FindAsync(userId) != null)
                .ConfigureAwait(false);

        public async Task ActivateUserNotification(ulong userId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var notif = await context.UserNotifiedOnRequestProcesses.FindAsync(userId).ConfigureAwait(false);
                if (notif != null) return; // Already set so we ignore
                var setNotif = new UserNotifiedOnRequestProcess(userId);
                context.UserNotifiedOnRequestProcesses.Add(setNotif);
                await context.SaveChangesAsync().ConfigureAwait(false);
            });

        public async Task RemoveUserNotification(ulong userId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var notif = await context.UserNotifiedOnRequestProcesses.FindAsync(userId).ConfigureAwait(false);
                if (notif == null) return; // Already not existant so we ignore
                context.UserNotifiedOnRequestProcesses.Remove(notif);
                await context.SaveChangesAsync().ConfigureAwait(false);
            });

        public async Task<Option<List<WaifuRequest>>> AllActiveRequests()
            => await _soraTransactor.DoAsync<Option<List<WaifuRequest>>>(async context =>
            {
                var reqs = await context.WaifuRequests
                    .Where(x => x.RequestState == RequestState.Pending)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return reqs.Count == 0 ? Option.None<List<WaifuRequest>>() : reqs;
            }).ConfigureAwait(false);

        public async Task<Option<List<WaifuRequest>>> AllRequests()
            => await _soraTransactor.DoAsync<Option<List<WaifuRequest>>>(async context =>
            {
                var reqs = await context.WaifuRequests
                    .ToListAsync()
                    .ConfigureAwait(false);
                return reqs.Count == 0 ? Option.None<List<WaifuRequest>>() : reqs;
            }).ConfigureAwait(false);

        public async Task AddWaifuRequest(WaifuRequestAddDto waifuRequestAddDto)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                WaifuRequest req = new WaifuRequest()
                {
                    Name = waifuRequestAddDto.Name,
                    ImageUrl = waifuRequestAddDto.ImageUrl,
                    Rarity = waifuRequestAddDto.Rarity,
                    RequestState = RequestState.Pending,
                    RequestTime = DateTime.UtcNow,
                    UserId = ulong.Parse(waifuRequestAddDto.UserId),
                };
                context.WaifuRequests.Add(req);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task EditWaifuRequest(WaifuRequestEditDto waifuRequestAddDto)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var req = await context.WaifuRequests.FindAsync(waifuRequestAddDto.RequestId).ConfigureAwait(false);
                if (req == null) return; // no op if it doesn't exist

                req.Name = waifuRequestAddDto.Name;
                req.ImageUrl = waifuRequestAddDto.ImageUrl;
                req.Rarity = waifuRequestAddDto.Rarity;

                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task<bool> WaifuExists(string waifuName)
            => await _soraTransactor.DoAsync(async context =>
                    await context.Waifus.FirstOrDefaultAsync(x =>
                        x.Name.Equals(waifuName, StringComparison.OrdinalIgnoreCase)) != null)
                .ConfigureAwait(false);

        public async Task<bool> WaifuExists(int id)
            => await _soraTransactor.DoAsync(async context =>
                    await context.Waifus.FindAsync(id) != null)
                .ConfigureAwait(false);

        public async Task<int> UserRequestCountLast24Hours(ulong userId)
            => await _soraTransactor.DoAsync<int>(async context =>
                {
                    var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(24));
                    return await context.WaifuRequests.CountAsync(x => x.UserId == userId && x.RequestTime > dt && x.RequestState == RequestState.Pending)
                        .ConfigureAwait(false);
                })
                .ConfigureAwait(false);
    }
}