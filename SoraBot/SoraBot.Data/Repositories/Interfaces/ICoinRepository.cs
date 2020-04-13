using System.Threading.Tasks;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface ICoinRepository
    {
        /// <summary>
        /// Gives the specified user the specified amount. Does nothing if the user isn't found
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task GiveAmount(ulong userId, uint amount);

        /// <summary>
        /// Takes the specified amount and does validation that he does have at least that much
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> TryTakeAmount(ulong userId, uint amount);

        /// <summary>
        /// Updates the user data with the new information.
        /// REQUIRED A USER TO EXIST. This will not create one
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dailyAmount"></param>
        /// <returns></returns>
        Task<bool> DoDaily(ulong userId, uint dailyAmount);

        /// <summary>
        /// This will make another check on the user amount. The error will not be descriptive tho
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="receiverId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> TryMakeTransfer(ulong userId, ulong receiverId, uint amount);
        
        /// <summary>
        /// Returns the amount of coins a user has. 0 If the user is not found 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        uint GetCoins(ulong userId);
    }
}