using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Dtos;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IWaifuRequestRepository
    {
        Task<Option<List<WaifuRequest>>> GetUserWaifuRequests(ulong userId);
        Task<bool> UserHasNotificationOn(ulong userId);
        Task ActivateUserNotification(ulong userId);
        Task RemoveUserNotification(ulong userId);
        Task<Option<List<WaifuRequest>>> AllActiveRequests();
        Task<Option<List<WaifuRequest>>> AllRequests();
        Task AddWaifuRequest(WaifuRequestAddDto waifuRequestAddDto);
        Task EditWaifuRequest(WaifuRequestEditDto waifuRequestAddDto);
        Task<bool> WaifuExists(string waifuName);
        Task<bool> WaifuExists(int id);
        Task<int> UserRequestCount(ulong userId);
    }
}