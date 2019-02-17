using System;

namespace UnrealReplayParser.Chunk
{
    public class ReplayDataInfo
    {
        public readonly uint Time1;
        public readonly uint Time2;
        public ReplayDataInfo(uint time1, uint time2)
        {
            Time1 = time1;
            Time2 = time2;
        }

        protected ReplayDataInfo(ReplayDataInfo info)
        {
            Time1 = info.Time1;
            Time2 = info.Time2;
        }
    }
}
