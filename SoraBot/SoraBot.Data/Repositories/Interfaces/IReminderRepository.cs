using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task<Option<List<Reminder>>> GetUserReminders(ulong userId);
        Task<int> GetUserReminderCount(ulong userId);
        Task AddReminderToUser(ulong userId, string message, DateTime dueDate);

        Task<List<Reminder>> GetAllReminders();
        Task<List<Reminder>> GetAllRemindersThatAreDue();
        Task RemoveReminder(uint id);
        Task RemoveReminders(List<uint> ids);
    }
}