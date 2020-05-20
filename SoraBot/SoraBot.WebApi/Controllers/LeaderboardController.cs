using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
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
        private readonly DiscordSocketClient _client;
        private readonly DiscordRestClient _restClient;

        public LeaderboardController(
            IProfileRepository profileRepo, 
            IUserService userService,
            DiscordSocketClient client,
            DiscordRestClient restClient)
        {
            _profileRepo = profileRepo;
            _userService = userService;
            _client = client;
            _restClient = restClient;
        }

        [HttpGet("{guildId}")]
        public async Task<IActionResult> GetLocalLeaderboard(ulong guildId)
        {
            var guild = _client.GetGuild(guildId);
            if (guild == null)
                return NotFound("The specified guild could not be found!");
            var gl = new GuildLeaderboard()
            {
                AvatarUrl = guild.IconUrl ?? _client.CurrentUser.GetAvatarUrl(),
                GuildName = guild.Name,
                Ranks = new List<GuildRank>(guild.MemberCount > 100 ? 100: guild.MemberCount) // Properly set capacity already for best performance!
            };

            var gUsersO = await _profileRepo.GetGuildUsersSorted(guildId);
            if (!gUsersO)
                return NotFound("Guild has no saved users!");

            var g = await _restClient.GetGuildAsync(guildId);
            // Create dictionary for easy and fast lookups later :)
            var users = (await g.GetUsersAsync().FlattenAsync()).ToDictionary(x=> x.Id, x=> x);
            var gUsers = (~gUsersO);
            for (var i = 0; i < gUsers.Count; i++)
            {
                var user = gUsers[i];
                int rank = i + 1;
                if (rank > 100) break;
                if (!users.TryGetValue(user.UserId, out var u))
                    continue;
                gl.Ranks.Add(new GuildRank()
                {
                    AvatarUrl = u.GetAvatarUrl() ?? u.GetDefaultAvatarUrl(),
                    Discrim = u.Discriminator,
                    Exp = user.Exp,
                    Name = u.Username,
                    Rank = rank
                });
            }

            return Ok(gl);
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