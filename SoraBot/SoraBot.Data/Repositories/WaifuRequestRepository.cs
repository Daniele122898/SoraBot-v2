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

        public async Task ChangeRequestStatus(uint requestId, RequestState requestState)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var req = await context.WaifuRequests.FindAsync(requestId).ConfigureAwait(false);
                if (req == null) return;

                req.RequestState = requestState;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        
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

        public async Task<Option<WaifuRequest>> GetWaifuRequest(uint requestId)
            => await _soraTransactor.DoAsync(async context =>
                    (await context.WaifuRequests.FindAsync(requestId).ConfigureAwait(false)) ??
                    Option.None<WaifuRequest>())
                .ConfigureAwait(false);

        public async Task<bool> RequestExistsAndBelongsToUser(uint requestId, ulong userId)
            => await _soraTransactor.DoAsync(async context =>
                await context.WaifuRequests.CountAsync(x => x.Id == requestId && x.UserId == userId) == 1
            ).ConfigureAwait(false);

        public async Task<bool> RequestExists(uint requestId)
            => await _soraTransactor.DoAsync(async context =>
                await context.WaifuRequests.CountAsync(x => x.Id == requestId) == 1
            ).ConfigureAwait(false);

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

        public async Task<uint> AddWaifuRequest(WaifuRequestAddDto waifuRequestAddDto)
            => await _soraTransactor.DoInTransactionAndGetAsync(async context =>
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
                return req.Id;
            }).ConfigureAwait(false);

        public async Task EditWaifuRequest(WaifuRequestEditDto waifuRequestAddDto)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                uint id = (uint) waifuRequestAddDto.RequestId;
                var req = await context.WaifuRequests.FindAsync(id).ConfigureAwait(false);
                if (req == null) return; // no op if it doesn't exist

                req.Name = waifuRequestAddDto.Name;
                req.ImageUrl = waifuRequestAddDto.ImageUrl;
                req.Rarity = waifuRequestAddDto.Rarity;

                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task<bool> WaifuExists(string waifuName)
            => await _soraTransactor.DoAsync(async context =>
                    await context.Waifus.CountAsync(x =>
                        x.Name.Equals(waifuName, StringComparison.OrdinalIgnoreCase)) >= 1)
                .ConfigureAwait(false);

        public async Task<bool> WaifuExists(int id)
            => await _soraTransactor.DoAsync(async context =>
                    await context.Waifus.CountAsync(x => x.Id == id) >= 1)
                .ConfigureAwait(false);

        public async Task<int> UserRequestCountLast24Hours(ulong userId)
            => await _soraTransactor.DoAsync<int>(async context =>
                {
                    var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(24));
                    return await context.WaifuRequests.CountAsync(x =>
                            x.UserId == userId && x.RequestTime > dt && x.RequestState == RequestState.Pending)
                        .ConfigureAwait(false);
                })
                .ConfigureAwait(false);

        public async Task AddWaifu(WaifuRequest wr)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                context.Waifus.Add(new Waifu()
                {
                    ImageUrl = wr.ImageUrl,
                    Name = wr.Name,
                    Rarity = wr.Rarity
                });
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task RemoveWaifuRequest(uint requestId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var req = await context.WaifuRequests.FindAsync(requestId).ConfigureAwait(false);
                if (req == null) return; // Do nothing when it doesnt exist
                context.WaifuRequests.Remove(req);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}