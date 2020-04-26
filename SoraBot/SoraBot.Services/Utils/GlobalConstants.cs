namespace SoraBot.Services.Utils
{
    public class GlobalConstants
    {
        public int ShardId { get; private set; }

        public void SetShardId(int shardId)
        {
            this.ShardId = shardId;
        }
    }
}