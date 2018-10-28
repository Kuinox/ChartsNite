using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReplayAnalyzer
{
    public class ReplayDataInfo : ChunkInfo
    {
        public readonly int ChunkIndex;
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly int ReplayDataSizeInBytes;
        public readonly long ReplayDataOffset;
        public readonly long StreamOffset;
        public ReplayDataInfo(int chunkIndex, uint time1, uint time2, int replayDataSizeInBytes, long replayDataOffset, long streamOffset, ChunkInfo info) : base(info)
        {
            if(info.Type != ChunkType.ReplayData) throw new InvalidOperationException();
            ChunkIndex = chunkIndex;
            Time1 = time1;
            Time2 = time2;
            ReplayDataSizeInBytes = replayDataSizeInBytes;
            ReplayDataOffset = replayDataOffset;
            StreamOffset = streamOffset;
        }

        public ReplayDataInfo(ReplayDataInfo info) : base(info)
        {
            ChunkIndex = info.ChunkIndex;
            ChunkIndex = info.ChunkIndex;
            Time1 = info.Time1;
            Time2 = info.Time2;
            ReplayDataSizeInBytes = info.ReplayDataSizeInBytes;
            ReplayDataOffset = info.ReplayDataOffset;
            StreamOffset = info.StreamOffset;
        }
    }
}
