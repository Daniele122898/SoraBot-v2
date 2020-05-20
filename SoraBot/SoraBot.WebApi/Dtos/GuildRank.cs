namespace SoraBot.WebApi.Dtos
{
    public class GuildRank
    {
        public string Name { get; set; }
        public string Discrim { get; set; }
        public int Rank { get; set; }
        public string AvatarUrl { get; set; }
        public uint Exp { get; set; }
        public string UserId { get; set; }
    }
}