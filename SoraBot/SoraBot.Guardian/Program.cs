using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace SoraBot.Guardian
{
    // This Script is extremely crude and was just made as a quick fix to not have to manually 
    // restart the services when D.Net thrashes the RAM. In an ideal world this script would 
    // not need to exist but sadly D.Net seems to leak RAM over time or not properly clean it's
    // caches thus RAM usage slowly creeps up until it's completely full and we run into slowdowns. 
    // DO NOT copy this or use this please find a better way of dealing with this issue :P
    class Program
    {
        // ReSharper disable once NotAccessedField.Local
        private static Timer _timer;
        private const int _OVERWATCH_COOLDOWN_MINS = 5;
        private const float _FREE_THRESHOLD = 0.15f;

        private static int _count = 0;
        private const int _MAX_COUNT = 4;
        
        // ReSharper disable once UnusedParameter.Local
        static async Task Main(string[] args)
        {
            #if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            #else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
            #endif
            
            Log.Information(@"
   _____                                                
  / ___/____  _________ _                               
  \__ \/ __ \/ ___/ __ `/                               
 ___/ / /_/ / /  / /_/ /                                
/____/\____/_/   \__,_/                                 
               ______                     ___           
              / ____/_  ______ __________/ (_)___ _____ 
             / / __/ / / / __ `/ ___/ __  / / __ `/ __ \
            / /_/ / /_/ / /_/ / /  / /_/ / / /_/ / / / /
            \____/\__,_/\__,_/_/   \__,_/_/\__,_/_/ /_/ 
                                            Version 1.0");
            Log.Information("Starting overwatch timer");
            
            // Check if we're on Linux
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Log.Fatal("For simplicity reasons this script only supports Linux!");
                Environment.Exit(-1);
                return;
            }
            // Check if script is running in root
            if (Mono.Unix.Native.Syscall.geteuid() != 0)
            {
                Log.Fatal("Script must run as root or it cannot restart the systemd services");
                Environment.Exit(-1);
                return;
            }
            
            _timer = new Timer(OverwatchCallback, null, 
                TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(_OVERWATCH_COOLDOWN_MINS));

            await Task.Delay(-1);
        }

        private static void OverwatchCallback(object obj)
        {
            Log.Debug("Starting Overwatch callback");
            try
            {
                // To simplify the code greatly we do not check the RAM usage of the Sora processes but rather just check
                // the total system ram usage. This goes by the assumption that our server mainly runs the Sora processes. 
                // If total RAM usage is above a certain percentage we just restart the services.
                var memInfo = File.ReadAllText("/proc/meminfo");
                var memTotalStr = memInfo.Substring(0, memInfo.IndexOf("\n", StringComparison.Ordinal));
                var reduced = memInfo.Substring(memInfo.IndexOf("MemAvailable", StringComparison.Ordinal));
                var memFreeStr = reduced.Substring(0, reduced.IndexOf("kB", StringComparison.Ordinal));

                var memTotal = float.Parse(Regex.Match(memTotalStr, @"\d+").Value);
                var memFree = float.Parse(Regex.Match(memFreeStr, @"\d+").Value);
                Log.Debug("\nTotal Memory: {TotalMem}\nAvailable Memory: {FreeMem}", memTotal, memFree);


                var freePerc = memFree / memTotal;
                if (freePerc > _FREE_THRESHOLD)
                {
                    if (_count == 0)
                        Log.Debug("Available Memory percentage of {FreePerc} over minimum Threshold of {MinThreshold}, resetting count.", freePerc, _FREE_THRESHOLD);
                    else
                        Log.Information("Available Memory percentage of {FreePerc} over minimum Threshold of {MinThreshold}, resetting count.", freePerc, _FREE_THRESHOLD);
                    _count = 0;
                    return;
                }
                ++_count;
                if (_count < _MAX_COUNT)
                {
                    Log.Information("Available Memory below threshold. Count increased to {Count} but below Threshold {Threshold}, monitoring...", _count, _MAX_COUNT);
                    return;
                }
                _count = 0;
                
                Log.Information("Available Memory below threshold and count exceeded. Restarting Sora services.");
                var soraProd = RestartSoraService("sora-oct-prod@{0..2}");
                var soraBeta = RestartSoraService("sora-oct");
                soraProd.Start();
                soraBeta.Start();
                
                Log.Information("Waiting for SoraProd to restart...");
                soraProd.WaitForExit();
                Log.Information("SoraProd restarted. Waiting for SoraBeta...");
                soraBeta.WaitForExit();
                Log.Information("SoraBeta restarted. Monitoring again");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed in Overwatch callback");
            }
        }

        private static Process RestartSoraService(string service)
        {
            var bash = new ProcessStartInfo()
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"systemctl restart {service}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            return new Process(){ StartInfo = bash};
        } 
    }
}
