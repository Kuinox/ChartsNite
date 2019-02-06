using System;
using System.Collections.Generic;
using System.Text;
using Common.StreamHelpers;
using UnrealReplayParser;

namespace FortniteReplayParser.Chunk
{
    public class HeaderChunk : ChunkInfo
    {
        protected HeaderChunk(ChunkInfo info) : base(info)
        {
        }
    }
}
