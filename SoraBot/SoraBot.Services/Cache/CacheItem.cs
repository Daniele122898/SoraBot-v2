using System;

namespace SoraBot.Services.Cache
{
    public class CacheItem
    {
        public object Item { get; }
        public DateTime? ValidUntil { get; }

        public CacheItem(object item, DateTime? validUntil)
        {
            this.Item = item;
            this.ValidUntil = validUntil;
        }
    }
}