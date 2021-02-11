namespace SoraBot.Data.Dtos.Profile
{
    public class ProfileImageGenDto
    {
        public ulong UserId { get; set; }
        public bool HasCustomBg { get; set; }
        public string Name { get; set; }
        public string ClanName { get; set; }
        public int GlobalRank { get; set; }
        public uint GlobalExp { get; set; }
        public int GlobalLevel { get; set; }
        public uint GlobalNextLevelExp { get; set; }
        public int LocalRank { get; set; }
        public uint LocalExp { get; set; }
        public int LocalLevel { get; set; }
        public uint LocalNextLevelExp { get; set; }
    }
}