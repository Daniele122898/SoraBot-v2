using System;

namespace SoraBot_v2.Services
{
    public class RequestOptions
    {
        public object GetFrom { get; }
        public TimeSpan Timeout { get; }

        public RequestOptions(object getFrom, TimeSpan timeout)
        {
            GetFrom = getFrom;
            Timeout = timeout;
        }
    }
}