using System.Threading;

namespace SoraBot.Services.Utils
{
    public static class GlobalConstants
    {
        public static int ShardId { get; private set; }
        public static int Port { get; private set; }
        public static uint CommandsExecuted { get; set; }
        public static uint MessagesReceived { get; set; }
        public static bool Production { get; set; } = false;

        public static CancellationTokenSource ApplicationCancellationTokenSource { get; private set; }

        public static void SetApplicationCancellationToken(CancellationTokenSource token)
        {
            ApplicationCancellationTokenSource = token;
        }
        
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