namespace SoraBot.Services.Utils
{
    public static class GlobalConstants
    {
        public static int ShardId { get; private set; }
        public static int Port { get; private set; }
        public static int CommandsExecuted { get; set; }

        public static void SetShardId(int shardId)
        {
            ShardId = shardId;
        }

        public static void SetPort(int port)
        {
            Port = port;
        }
    }
}