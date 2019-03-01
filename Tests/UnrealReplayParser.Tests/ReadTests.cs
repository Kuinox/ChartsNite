using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.StreamHelpers;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using FortniteReplayParser;
using System.Linq;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser.Tests
{
    [TestFixture]
    public class ReadTests
    {
        public static IEnumerable<(Type, string)> ParserProvider() => new ReplayFetcher().GetAllReplaysStreamsWithAllParsers();


        [Test, TestCaseSource( nameof( ParserProvider ) )]
        public async Task NoExceptionWhileReading( (Type, string) tuple )
        {
            using( Stream replayStream = new DebugStream( File.OpenRead( tuple.Item2 ) ) )
            using( UnrealReplayVisitor unrealVisitor = new UnrealReplayVisitor( replayStream ) )
            {
                (await unrealVisitor.Visit()).Should().Be( true );
            }
        }

        [Test, TestCaseSource(nameof(ParserProvider))]
        public async Task NoErrorWhileReading((Type,string) tuple)
        {
            var substituteOfType = typeof(Substitute).GetMethod(nameof(Substitute.ForPartsOf)).MakeGenericMethod(tuple.Item1);
            using (Stream replayStream = new DebugStream(File.OpenRead(tuple.Item2)))
            using (UnrealReplayVisitor unrealVisitor = (UnrealReplayVisitor)substituteOfType.Invoke(null, new[] { new object[] { replayStream } }))
            {
                (await unrealVisitor.Visit()).Should().Be(true);
                await unrealVisitor.DidNotReceiveWithAnyArgs().ErrorOnChunkContentParsingAsync();
                await unrealVisitor.ReceivedWithAnyArgs().ChooseChunkType(Arg.Any<ChunkReader>(), Arg.Any<ChunkType>());
            }
        }

    }
}
