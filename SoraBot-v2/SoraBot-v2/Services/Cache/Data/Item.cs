using System;

namespace SoraBot_v2.Services
{
    public class Item
    {
        public object Content { get; }
        public DateTime Timeout { get; }

        public Item(object content, DateTime timeout)
        {
            Content = content;
            Timeout = timeout;
        }
    }
}