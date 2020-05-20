using System.Collections.Generic;

namespace SoraBot.WebApi.Dtos
{
    public class GuildLeaderboard
    {
        public bool Success { get; set; }
        public string AvatarUrl { get; set; }
        public string GuildName { get; set; }
        public List<GuildRank> Ranks { get; set; } = new List<GuildRank>();
        public List<RoleReward> RoleRewards { get; set; } = new List<RoleReward>();
    }

    public class RoleReward
    {
        public int LevelReq { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}