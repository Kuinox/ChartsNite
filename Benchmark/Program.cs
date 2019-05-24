using FortniteReplayParser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayParser;
using UnrealReplayParser.Tests;

namespace Benchmark
{
    class Program
    {
        static async Task Main()
        {

            ConcurrentQueue<TimeSpan> times = new ConcurrentQueue<TimeSpan>();
            ConcurrentQueue<long> sizes = new ConcurrentQueue<long>();
            
            Stopwatch realTime = new Stopwatch();
            realTime.Start();
            var paths = new ReplayFetcher().GetAllReplaysPath().ToList().Where(p=>p.Contains( "replay24-05.replay") || p.Contains("newshinyreplay.replay") );
            foreach(var path in paths)
            {
                Stopwatch stopwatch = new Stopwatch();
                using( var replayStream = File.OpenRead( path ) )
                using( var bufferedStream = new MemoryStream() )
                using( var fortniteDataGrabber = new FortniteReplayVisitor( bufferedStream ) )
                {
                    await replayStream.CopyToAsync( bufferedStream );
                    bufferedStream.Position = 0;
                    sizes.Enqueue( replayStream.Length );
                    stopwatch.Start();
                    bool success = await fortniteDataGrabber.Visit();
                    stopwatch.Stop();
                    times.Enqueue( stopwatch.Elapsed );
                    Console.WriteLine( $"Done time:{stopwatch.ElapsedMilliseconds.ToString().PadRight( 4 )}ms, speed:{(replayStream.Length / 1024 / 1024 / stopwatch.Elapsed.TotalSeconds).ToString( "0.00" ).PadRight( 5 ) } path: {Path.GetFileNameWithoutExtension( path )}" );
                    stopwatch.Reset();
                }
            } 
            double totalTime = times.Sum( p => p.TotalSeconds );
            Console.WriteLine( "Finished in:" + totalTime + "s" );
            Console.WriteLine( "Average of " + totalTime / times.Count );
            Console.WriteLine( "Parsing speed of " + (float)sizes.Sum() / 1024 / 1024 / totalTime + "MB/s" );
            realTime.Stop();
            Console.WriteLine( "Real time: " + realTime.Elapsed.TotalSeconds + "s" );
            Console.WriteLine( "Real speed: " + (float)sizes.Sum() / 1024 / 1024 / realTime.Elapsed.TotalSeconds + "MB/s" );
        }
    }
}
