using System;

namespace SoraBot.Services.Cache
{
    public class CacheItem
    {
        public object Content { get; }
        public DateTime? ValidUntil { get; }

        public CacheItem(object content, DateTime? validUntil)
        {
            this.Content = content;
            this.ValidUntil = validUntil;
        }
        
        public CacheItem(object content, in TimeSpan timeSpan)
        {
            this.Content = content;
            this.ValidUntil = DateTime.UtcNow.Add(timeSpan);
        }

        public bool IsValid()
        {
            if (this.ValidUntil.HasValue && this.ValidUntil.Value.CompareTo(DateTime.UtcNow) <= 0)
            {
                return false;
            }

            return true;
        }
    }
}