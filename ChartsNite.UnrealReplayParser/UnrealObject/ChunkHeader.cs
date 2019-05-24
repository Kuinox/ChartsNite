using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser.UnrealObject
{
    public enum ChunkType : uint
    {
        Header,
        ReplayData,
        Checkpoint,
        Event,
        EndOfStream = Unknown - 1,
        Unknown = 0xFFFFFFFF
    };

    public class ChunkHeader
    {
        public ChunkType ChunkType { get; set; }
        public int ChunkSize { get; set; }
    }
}
