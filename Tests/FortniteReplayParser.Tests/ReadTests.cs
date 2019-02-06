using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChartsNite.TestHelper;
using NUnit.Framework;
using UnrealReplayParser;
using FortniteReplayParser;
namespace FortniteReplayParser.Tests
{
    [TestFixture]
    public class ReadTests
    {
        [Test, TestCaseSource(typeof(ReplayFetcher), nameof(ReplayFetcher.GetAllReplaysStreams))]
        public async Task CanReadWithoutException(string replayPath)
        {
            using (FileStream replayStream = File.OpenRead(replayPath))
            using(FortniteReplayParser fortniteParser = new FortniteReplayParser(await ChunkReader.FromStream(replayStream)))
            {
                {
                    while (true)
                    {
                        using (ChunkInfo? chunkInfo = await fortniteParser.ReadChunk())
                        {
                            if (chunkInfo == null) break;
                            await chunkInfo.Stream.ReadAsync(new byte[chunkInfo.SizeInBytes], 0, chunkInfo.SizeInBytes);
                        }
                    }
                }
            }
        }
    }
}
