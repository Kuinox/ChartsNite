using FortniteReplayParser;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayParser.Tests;

namespace Benchmark
{
    class Program
    {
        static async Task Main()
        {
            foreach( var path in new ReplayFetcher().GetAllReplaysPath().OrderByDescending(p=>p) )
            {
                using( var replayStream = File.OpenRead( path ) )
                using( var fortniteDataGrabber = new FortniteReplayVisitor( replayStream ) )
                {
                    bool success = await fortniteDataGrabber.Visit();
                    Console.WriteLine( "Done: " + path ); 
                }
            }
        }
    }
}
