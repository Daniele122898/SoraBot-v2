using System.Collections.Generic;

namespace SoraBot_v2.WebApiModels
{
    public class UserGuilds
    {
        public string UserId { get; set; }
        public List<WebGuild> Guilds {get; set;} = new List<WebGuild>();
    }

    public class WebGuild
    {
        public string GuildId { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public WebUser Owner { get; set; }
        public int MemberCount { get; set; }
        public int OnlineMembers { get; set; }
        public string Region { get; set; }
        public int RoleCount { get; set; }
        public int TextChannelCount { get; set; }
        public int VoiceChannelCount { get; set; }
        public string AfkChannel { get; set; }
        public int EmoteCount { get; set; }
        public string Prefix { get; set; }
        public bool IsDjRestricted { get; set; }
        public int StarMinimum { get; set; }
        public int TagCount { get; set; }
        public int StarMessageCount { get; set; }
        public int SarCount { get; set; }
        public int ModCaseCount { get; set; }
        public SoraPerms SoraPerms { get; set; }
    }
}