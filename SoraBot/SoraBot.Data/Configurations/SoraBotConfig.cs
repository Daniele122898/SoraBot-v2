namespace SoraBot.Data.Configurations
{
    public class SoraBotConfig
    {
        public string DiscordToken { get; set; }
        public string DbConnection { get; set; }
        public int TotalShards { get; set; }
        public string DiscordSupportInvite { get; set; }
        public string SoraBotInvite { get; set; }
    }
}