using System.IO;
using System.Threading.Tasks;
using ChartsNite.TestHelper;
using Common.StreamHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace UnrealReplayParser.Tests
{
    [TestFixture]
    public class ReadTests
    {
        [Test, TestCaseSource(typeof(ReplayFetcher), nameof(ReplayFetcher.GetAllReplaysStreams))]
        public async Task CanReadWithoutException(string replayPath)
        {
            using (Stream replayStream = new DebugStream(File.OpenRead(replayPath)))
            using(SubStreamFactory factory = new SubStreamFactory(replayStream))
            using (UnrealReplayVisitor unrealVisitor = await UnrealReplayVisitor.FromStream(factory))
            {
                (await unrealVisitor.Visit()).Should().Be(true);
            }
        }
    }
}
