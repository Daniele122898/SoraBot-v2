using ArgonautCore.Network.Health.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace SoraBot.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly DiscordSocketClient _client;

        public HealthController(DiscordSocketClient client)
        {
            _client = client;
        }
        
        [HttpGet]
        public ActionResult<HealthStatus> GetHealthStatus()
        {
            return _client.ConnectionState switch
            {
                ConnectionState.Connected => Ok(new HealthStatus($"Sora Shard {_client.ShardId.ToString()}",
                    Status.Healthy)),
                ConnectionState.Connecting => Ok(new HealthStatus($"Sora Shard {_client.ShardId.ToString()}",
                    Status.PartialOutage) {Error = "Socket connection is currently in process of recovery"}),
                _ => Ok(new HealthStatus($"Sora Shard {_client.ShardId.ToString()}", Status.Outage)
                {
                    Error = "Websocket has completely disconnected"
                })
            };
        }
    }
}