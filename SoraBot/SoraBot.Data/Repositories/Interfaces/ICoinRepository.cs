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
        /// Returns the amount of coins a user has. 0 If the user is not found 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<uint> GetCoins(ulong userId);
    }
}