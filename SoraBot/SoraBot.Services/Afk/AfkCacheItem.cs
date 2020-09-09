
namespace SoraBot.Services.Afk
{
    public class AfkCacheItem
    {
        public Data.Models.SoraDb.Afk Afk { get; set; }
        public bool InCache { get; set; }

        public AfkCacheItem(Data.Models.SoraDb.Afk afk, bool inCache)
        {
            this.Afk = afk;
            InCache = inCache;
        }
        
    }
}