namespace SoraBot_v2.WebApiModels
{
    public class SoraStats
    {
        public string Uptime { get; set; }
        public string MessagesReceived { get; set; }
        public int CommandsExecuted { get; set; }
        public int Ping { get; set; }
        public int GuildCount { get; set; }
        public int UserCount { get; set; }
        public int ShardNum { get; set; }
        public string Version { get; set; }
    }
}