using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;
using SoraBot.Services.Utils;
using SoraBot.WebApi.Dtos;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly IProfileRepository _profileRepo;
        private readonly IUserService _userService;

        public LeaderboardController(IProfileRepository profileRepo, IUserService userService)
        {
            _profileRepo = profileRepo;
            _userService = userService;
        }

        [HttpGet("{guildId}")]
        public async Task<IActionResult> GetLocalLeaderboard(ulong guildId)
        {
            
        }

        [HttpGet("global")]
        public async Task<ActionResult<GlobalLeaderboard>> GetGlobalLeaderboard()
        {
            var topUsers = await _profileRepo.GetTop150Users().ConfigureAwait(false);
            if (!topUsers)
                return NotFound("Could not fetch Leaderboard");
            
            var leaderboard = new GlobalLeaderboard(){ShardId = GlobalConstants.ShardId};
            var users = topUsers.Some();
            for (var i = 0; i < users.Count; i++)
            {
                var dbUser = users[i];
                var user = _userService.Get(dbUser.Id);
                if (!user) continue;
                int rank = i + 1;
                leaderboard.Ranks.Add(user.MatchSome((u) => new GuildRank()
                {
                    Rank = rank,
                    AvatarUrl = u.GetAvatarUrl() ?? u.GetDefaultAvatarUrl(),
                    Discrim = u.Discriminator,
                    Exp = dbUser.Exp,
                    Name = u.Username
                }));
                if (leaderboard.Ranks.Count >= 100) break;
            }

            return Ok(leaderboard);
        }
    }
}