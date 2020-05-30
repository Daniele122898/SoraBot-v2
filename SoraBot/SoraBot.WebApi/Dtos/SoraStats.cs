namespace SoraBot.WebApi.Dtos
{
    public class SoraStats
    {
        public string Uptime { get; set; }
        public string MessagesReceived { get; set; }
        public uint CommandsExecuted { get; set; }
        public int Ping { get; set; }
        public int GuildCount { get; set; }
        public int UserCount { get; set; }
        public int ShardNum { get; set; }
        public string Version { get; set; }
    }
}