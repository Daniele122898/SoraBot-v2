using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task<Maybe<List<Reminder>>> GetUserReminders(ulong userId);
        Task<int> GetUserReminderCount(ulong userId);
        Task AddReminderToUser(ulong userId, string message, DateTime dueDate);

        Task<List<Reminder>> GetAllReminders();
        Task<List<Reminder>> GetAllRemindersThatAreDue();
        Task RemoveReminder(uint id);
        Task RemoveReminders(List<uint> ids);
    }
}