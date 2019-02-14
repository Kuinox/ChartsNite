using System;

namespace UnrealReplayParser.Chunk
{
    public class ReplayDataInfo : ChunkInfo
    {
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly int ReplayDataSizeInBytes;
        public ReplayDataInfo(uint time1, uint time2, int replayDataSizeInBytes, ChunkInfo info) : base(info)
        {
            if(info.ChunkType != ChunkType.ReplayData) throw new InvalidOperationException();
            Time1 = time1;
            Time2 = time2;
            ReplayDataSizeInBytes = replayDataSizeInBytes;
        }

        protected ReplayDataInfo(ReplayDataInfo info) : base(info)
        {
            Time1 = info.Time1;
            Time2 = info.Time2;
            ReplayDataSizeInBytes = info.ReplayDataSizeInBytes;
        }
    }
}
