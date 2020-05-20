using System.Collections.Generic;

namespace SoraBot.WebApi.Dtos
{
    public class GlobalLeaderboard
    {
        public int ShardId { get; set; }
        public List<GuildRank> Ranks { get; set; } = new List<GuildRank>(150);
    }
}