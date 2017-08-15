using System.Threading;
using Discord.Audio;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class AudioData
    {
        public IAudioClient AudioClient { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public bool Repeat { get; set; }
    }
}