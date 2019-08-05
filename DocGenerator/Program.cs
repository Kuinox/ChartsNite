using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayParser;
using UnrealReplayParser.Tests;

namespace DocGenerator
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var paths = new ReplayFetcher().GetAllReplaysPath().ToList();
            foreach( var path in paths )
            {
                using( var replayStream = File.OpenRead( path ) )
                using( var reader = new Reader( replayStream ) )
                {
                    bool success = await reader.Visit();
                    if( !success ) Console.WriteLine( $"Error on : {path}" );
                    Console.WriteLine( $"{path} ReplayLength: {reader.ReplayLength} ReplayStart:{reader.GameStartTimestamp} ReplayEnd:{reader.EndOfTheReplay} " );
                }
            }
        }

        class Reader : UnrealReplayVisitor
        {

            public TimeSpan ReplayLength;
            public DateTime GameStartTimestamp;
            public DateTime EndOfTheReplay => GameStartTimestamp + ReplayLength;
            public Reader( Stream stream ) : base( stream )
            {
            }

            public override ValueTask<bool> VisitChunks()
            {
                ReplayLength = TimeSpan.FromMilliseconds(ReplayHeader.LengthInMs);
                GameStartTimestamp = ReplayHeader.Timestamp;
                return new ValueTask<bool>(true);
            }
        }
    }

    public class ReplayFetcher
    {
        public IEnumerable<string> GetAllReplaysPath()
        {
            string path = Directory.GetCurrentDirectory();
            while( !Directory.Exists( path + Path.DirectorySeparatorChar + "Replays" ) )
            {
                path = Directory.GetParent( path ).FullName;
            }
            path += Path.DirectorySeparatorChar + "Replays";
            return Directory.GetFiles( path, "*.replay" );
        }
    }
}
