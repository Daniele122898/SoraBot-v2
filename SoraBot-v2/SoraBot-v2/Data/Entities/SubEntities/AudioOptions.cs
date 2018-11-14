using System.Collections.Generic;
using Discord;
using Victoria.Objects;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class AudioOptions
    {
        public bool RepeatTrack { get; set; }
        public IUser Summoner { get; set; }
        public LavaTrack VotedTrack { get; set; }
        public HashSet<ulong> Voters { get; set; }
    }
}