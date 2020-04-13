namespace SoraBot.Services.Cache
{
    // These are for custom Cache Integers.
    // Since these are VERY low integers they should technically not interfere 
    // with the Discord Snowflake IDs.
    // I use these since integer dictionary accesses and additions are 10x faster
    // than string. See https://stackoverflow.com/questions/5743474/key-performance-for-a-dictionary
    public enum CustomCacheIDs
    {
        WaifuList
    }
}