using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        
        public Task<List<Reminder>> GetUserReminders(ulong userId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Reminder>> GetAllReminders()
        {
            throw new System.NotImplementedException();
        }


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

        public Task RemoveReminder(uint id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReminders(List<uint> ids)
        {
            throw new NotImplementedException();
        }
    }
}