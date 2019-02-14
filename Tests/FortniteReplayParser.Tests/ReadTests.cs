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
            using (FortniteReplayVisitor fortniteVisitor = new FortniteReplayVisitor(factory))
            {
                (await fortniteVisitor.Visit()).Should().Be(true);
            }
        }

        //class FortniteErrorCounter : FortniteReplayVisitor
        //{
        //    protected FortniteErrorCounter(ReplayInfo info, SubStreamFactory subStreamFactory) : base(info, subStreamFactory)
        //    {
        //    }
        //    public static async Task<FortniteErrorCounter> ErrorCounterFromStream(SubStreamFactory subStreamFactory)
        //    {
        //        return new FortniteErrorCounter((await FromStream(subStreamFactory)).Info, subStreamFactory);
        //    }

        //}

        //[Test, TestCaseSource(typeof(ReplayFetcher), nameof(ReplayFetcher.GetAllReplaysStreams))]
        //public async Task ParsedWithoutError(string replayPath)
        //{
        //    using (FileStream replayStream = File.OpenRead(replayPath))
        //    using (SubStreamFactory factory = new SubStreamFactory(replayStream))
        //    using (FortniteErrorCounter fortniteVisitor = await FortniteErrorCounter.ErrorCounterFromStream(factory))
        //    {

        //    }
        //}
    }
}
