namespace SoraBot.Services.Utils
{
    public static class GlobalConstants
    {
        public static int ShardId { get; private set; }

        public static void SetShardId(int shardId)
        {
            ShardId = shardId;
        }
    }
}