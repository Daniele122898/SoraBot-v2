using System;
using Victoria.EventArgs;

namespace SoraBot.Bot.Modules.AudioModule
{
    public sealed class AudioStats
    {
        /// <summary>Machine's CPU info.</summary>
        public Cpu Cpu { get; private set; }

        /// <summary>General memory information about Lavalink.</summary>
        public Memory Memory { get; private set; }

        /// <summary>Connected players.</summary>
        public int Players { get; private set; }

        /// <summary>Players that are currently playing.</summary>
        public int PlayingPlayers { get; private set; }

        /// <summary>Lavalink uptime.</summary>
        public TimeSpan Uptime { get; private set; }

        public AudioStats(StatsEventArgs e)
        {
            this.Update(e);
        }

        public void Update(StatsEventArgs e)
        {
            this.Cpu = e.Cpu;
            this.Memory = e.Memory;
            this.Players = e.Players;
            this.PlayingPlayers = e.Players;
            this.Uptime = e.Uptime;
        }
    }
    
    public class AudioStatsService
    {
        public AudioStats AudioStats { get; private set; }

        public void SetStats(StatsEventArgs e)
        {
            if (AudioStats == null)
                AudioStats = new AudioStats(e);
            else 
                AudioStats.Update(e);
        }
    }
}