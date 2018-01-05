namespace SoraBot_v2.WebApiModels
{
    public class SoraStats
    {
        public string Version { get; set; }
        public int Ping { get; set; }
        public int GuildCount { get; set; }
        public int UserCount { get; set; }
        public double MessagesReceived { get; set; }
        public int CommandsExecuted { get; set; }
    }
}