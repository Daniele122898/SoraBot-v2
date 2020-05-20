using System;
using System.Diagnostics;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SoraBot.Data.Configurations;
using SoraBot.Services.Utils;
using SoraBot.WebApi.Dtos;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly DiscordSocketClient _client;
        private SoraBotConfig _config;

        public StatsController(DiscordSocketClient client, IOptions<SoraBotConfig> config)
        {
            _client = client;
            _config = config.Value;
        }
        
        [HttpGet]
        public ActionResult<SoraStats> GetSoraStats()
        {
            var users = 0;
            // Not using LINQ bcs the delegate slows shit down unnecessarily
            foreach (var guild in _client.Guilds)
            {
                users += guild.MemberCount;
            }

            using var proc = Process.GetCurrentProcess();
            return new SoraStats()
            {
                Version = _config.SoraVersion,
                Ping = _client.Latency,
                GuildCount = _client.Guilds.Count,
                UserCount = users,
                ShardNum = _config.TotalShards,
                Uptime = (DateTime.Now - proc.StartTime).Humanize(),
                CommandsExecuted = GlobalConstants.CommandsExecuted,
                MessagesReceived = GlobalConstants.MessagesReceived.ToString()
            };
        }
    }
}