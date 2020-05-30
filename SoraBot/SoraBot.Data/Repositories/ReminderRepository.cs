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
    public class ReminderRepository : IReminderRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public ReminderRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Option<List<Reminder>>> GetUserReminders(ulong userId)
            => await _soraTransactor.DoAsync(async context =>
            {
                var rems = await context.Reminders
                    .Where(x => x.UserId == userId)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (rems == null || rems.Count == 0)
                    return Option.None<List<Reminder>>();

                return rems;
            }).ConfigureAwait(false);

        public async Task<int> GetUserReminderCount(ulong userId)
            => await _soraTransactor.DoAsync(async context =>
                await context.Reminders
                    .Where(x => x.UserId == userId)
                    .CountAsync()
                    .ConfigureAwait(false)
            ).ConfigureAwait(false);

        public async Task AddReminderToUser(ulong userId, string message, DateTime dueDate)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var user = await context.Users.GetOrCreateUserNoSaveAsync(userId).ConfigureAwait(false);
                user.Reminders.Add(new Reminder(userId, message, dueDate));
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task<List<Reminder>> GetAllReminders()
            => await _soraTransactor.DoAsync(async context =>
                await context.Reminders.ToListAsync().ConfigureAwait(false)
            ).ConfigureAwait(false);


        /// <summary>
        /// Uses UTC now to get all the reminders which due date are smaller than this :D
        /// </summary>
        public async Task<List<Reminder>> GetAllRemindersThatAreDue()
            => await _soraTransactor.DoAsync(async context =>
            {
                // Generally we dont need to do that here because there's no actual iteration.
                // But whatever idk what linq does lol
                var dueDate = DateTime.UtcNow;
                var reminders = await context.Reminders
                    .Where(x => x.DueDateUtc < dueDate)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return reminders;
            }).ConfigureAwait(false);

        public async Task RemoveReminder(uint id)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var remToRemove = await context.Reminders.FindAsync(id).ConfigureAwait(false);
                if (remToRemove == null) return;
                context.Reminders.Remove(remToRemove);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task RemoveReminders(List<uint> ids)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var remsToRemove = await context.Reminders
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync()
                    .ConfigureAwait(false);
                if (remsToRemove == null || remsToRemove.Count == 0) return;

                context.Reminders.RemoveRange(remsToRemove);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}