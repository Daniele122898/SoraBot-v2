using System.Collections.Generic;

namespace SoraBot_v2.WebApiModels
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

    public class GlobalLeaderboard
    {
        public int ShardId { get; set; }
        public List<GuildRank> Ranks { get; set; } = new List<GuildRank>();
    }

    public class GuildRank
    {
        public string Name { get; set; }
        public string Discrim { get; set; }
        public int Rank { get; set; }
        public string AvatarUrl { get; set; }
        public int Exp { get; set; }
        public int NextExp { get; set; }
        public string UserId { get; set; }
    }
}