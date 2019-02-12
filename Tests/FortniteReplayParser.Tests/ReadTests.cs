using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChartsNite.TestHelper;
using NUnit.Framework;
using UnrealReplayParser;
using FortniteReplayParser;
using FluentAssertions;
using Common.StreamHelpers;

namespace FortniteReplayParser.Tests
{
    [TestFixture]
    public class ReadTests
    {
        [Test, TestCaseSource(typeof(ReplayFetcher), nameof(ReplayFetcher.GetAllReplaysStreams))]
        public async Task CanReadWithoutException(string replayPath)
        {
            using (FileStream replayStream = File.OpenRead(replayPath))
            using (SubStreamFactory factory = new SubStreamFactory(replayStream))
            using (FortniteReplayVisitor fortniteVisitor = await FortniteReplayVisitor.FortniteVisitorFromStream(factory))
            {
                (await fortniteVisitor.Visit()).Should().Be(true);
            }
        }
    }
}
