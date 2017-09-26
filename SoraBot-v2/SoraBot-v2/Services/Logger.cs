using System;
using System.IO;
using Discord.Commands;

namespace SoraBot_v2.Services
{
    public static class Logger
    {
        public const string RAMLog = "logs/RAMLogging.txt";
        private static long _lastRam;
        private const int MAX_JUMP = 20;


        public static void WriteRamLog(SocketCommandContext context)
        {
            long FormatRamValue(long d)
            {
                while (d > 1000)
                {
                    d /= 1000;
                }
                return d;
            }

            string FormatRamUnit(long d)
            {
                var units = new string[] {"B", "KB", "MB", "GB", "TB", "PB"};
                var unitCount = 0;
                while (d > 1000)
                {
                    d /= 1000;
                    unitCount++;
                }
                return units[unitCount];
            }

            if(!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }
            var mem = GC.GetTotalMemory(false);
            var jump = (mem - _lastRam)/1000000; //1000000 bytes are 1 MB
            //convert from bytes to MB check if its above 10 mb
            bool bigJump = ((jump) > MAX_JUMP);
            File.AppendAllText(RAMLog, $"{(bigJump ? "!!! - " : "")}[{DateTime.UtcNow}] : RAM {FormatRamValue(mem):f2} {FormatRamUnit(mem)} JUMP: {jump} MB MESSAGE: {context.Message.Content}\n");
            _lastRam = mem;
        }
    }
}