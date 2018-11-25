using System;
using System.Collections.Generic;
using System.Text;
using Common.StreamHelpers;
using ReplayAnalyzer;

namespace FortniteReplayAnalyzer.Chunk
{
    public class HeaderChunk : ChunkInfo
    {
        protected HeaderChunk(ChunkInfo info) : base(info)
        {
        }
    }
}
