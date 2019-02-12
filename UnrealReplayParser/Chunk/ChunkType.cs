using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser.Chunk
{
    public enum ChunkType : uint
    {
        Header,
        ReplayData,
        Checkpoint,
        Event,
        Unknown = 0xFFFFFFFF
    };
}
