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
namespace UnrealReplayParser.Tests
{
    [TestFixture]
    public class ReadTests
    {

       
        //[Test, TestCaseSource(typeof(ReplayFetcher), nameof(ReplayFetcher.GetAllReplaysStreams))]
        //public async Task CanReadWithoutExceptionAndVisitorReturnTrue(string replayPath)
        //{
        //    using (Stream replayStream = new DebugStream(File.OpenRead(replayPath)))
        //    using (SubStreamFactory factory = new SubStreamFactory(replayStream))
        //    using (UnrealReplayVisitor unrealVisitor = new UnrealReplayVisitor(factory))
        //    {
        //        (await unrealVisitor.Visit()).Should().Be(true);
        //    }
        //}

        public static IEnumerable<(Type, string)> ParserProvider() => new ReplayFetcher().GetAllReplaysStreams();

        [Test, TestCaseSource(nameof(ParserProvider))]
        public async Task NoErrorWhileReading((Type,string) tuple)
        {
            var substituteOfType = typeof(Substitute).GetMethod(nameof(Substitute.ForPartsOf)).MakeGenericMethod(tuple.Item1);
            using (Stream replayStream = new DebugStream(File.OpenRead(tuple.Item2)))
            using (SubStreamFactory factory = new SubStreamFactory(replayStream))
            using (UnrealReplayVisitor unrealVisitor = (UnrealReplayVisitor)substituteOfType.Invoke(null, new[] { new object[] { factory } }))
            {
                (await unrealVisitor.Visit()).Should().Be(true);
                await unrealVisitor.DidNotReceiveWithAnyArgs().VisitChunkContentParsingError();
                await unrealVisitor.ReceivedWithAnyArgs().ChooseChunkType(Arg.Any<ReplayInfo>(), Arg.Any<ChunkInfo>());
            }
        }

    }
}
