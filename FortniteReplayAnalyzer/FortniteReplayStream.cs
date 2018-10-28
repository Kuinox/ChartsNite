using ReplayAnalyzer;
using System.IO;
using System.Threading.Tasks;

namespace FortniteReplayAnalyzer
{
    class FortniteReplayStream : ReplayStream
    {
        protected FortniteReplayStream(Stream stream) : base(stream)
        {
        }

        public override async Task<ChunkInfo> ReadChunk()
        {
            ChunkInfo chunkInfo = await base.ReadChunk();
            if (chunkInfo is EventInfo eventInfo)
            {
                switch (eventInfo.Group)
                {
                    case "playerElim":
                        new KillEventChunk(eventInfo);
                        break;
                    case "AthenaMatchStats":
                        break;
                    case "AthenaMatchTeamStats":
                        break;
                }
            }

            return chunkInfo;
        }
    }
}
