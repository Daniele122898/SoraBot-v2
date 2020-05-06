using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task<List<Reminder>> GetUserReminders(ulong userId);
        Task<List<Reminder>> GetAllReminders();
        Task<List<Reminder>> GetAllRemindersThatAreDue();
        Task RemoveReminder(uint id);
        Task RemoveReminders(List<uint> ids);
    }
}