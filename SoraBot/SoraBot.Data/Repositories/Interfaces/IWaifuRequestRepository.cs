using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Dtos;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IWaifuRequestRepository
    {
        Task ChangeRequestStatus(uint requestId, RequestState requestState, string rejectReason = null);
        Task<Option<List<WaifuRequest>>> GetUserWaifuRequests(ulong userId);
        Task<Option<WaifuRequest>> GetWaifuRequest(uint requestId);
        Task<bool> RequestExistsAndBelongsToUser(uint requestId, ulong userId);
        Task<bool> RequestExists(uint requestId);
        Task<bool> UserHasNotificationOn(ulong userId);
        Task ActivateUserNotification(ulong userId);
        Task RemoveUserNotification(ulong userId);
        Task<Option<List<WaifuRequest>>> AllActiveRequests();
        Task<Option<List<WaifuRequest>>> AllRequests();
        Task<uint> AddWaifuRequest(WaifuRequestAddDto waifuRequestAddDto);
        Task EditWaifuRequest(WaifuRequestEditDto waifuRequestAddDto);
        Task<bool> WaifuExists(string waifuName);
        Task<bool> WaifuExists(int id);
        Task<int> UserRequestCountLast24Hours(ulong userId);
        Task AddWaifu(WaifuRequest wr);
        Task RemoveWaifuRequest(uint requestId);
    }
}