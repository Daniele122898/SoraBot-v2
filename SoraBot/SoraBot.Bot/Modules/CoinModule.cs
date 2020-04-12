using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    public class CoinModule : SoraSocketCommandModule
    {
        private readonly IUserRepository _userRepo;
        private readonly ICoinRepository _coinRepo;
        private readonly ILogger<CoinModule> _logger;

        public CoinModule(IUserRepository userRepo, ICoinRepository coinRepo, ILogger<CoinModule> logger)
        {
            _userRepo = userRepo;
            _coinRepo = coinRepo;
            _logger = logger;
        }

        [Command("daily")]
        public async Task EarnDaily()
        {
            
        }

        [Command("coins")]
        public async Task GetCoinAmount(IUser userT = null)
        {
            var user = userT ?? Context.User;
            // We dont care if the user exists. So we take the easy way out
            var amount = await _coinRepo.GetCoins(user.Id);

            await ReplyInfoEmbed(
                $"{(user.Id == Context.User.Id ? "You have" : $"{Formatter.UsernameDiscrim(user)} has")} {amount.ToString()} Sora Coins.");
        }
    }
}