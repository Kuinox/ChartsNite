using System;
using System.Collections.Generic;
using System.Text;
using ReplayAnalyzer;

namespace FortniteReplayAnalyzer
{
    class KillEventChunk : EventInfo
    {
        public KillEventChunk(int chunkIndex, string id, string @group, string metadata, uint time1, uint time2, int eventSizeInBytes, long eventDataOffset, ChunkInfo info) : base(chunkIndex, id, @group, metadata, time1, time2, eventSizeInBytes, eventDataOffset, info)
        {
        }

        public KillEventChunk(EventInfo info) :base(info.ChunkIndex, info.Id, info.Group, info.Metadata, info.Time1, info.Time2, info.EventSizeInBytes, info.EventDataOffset, info)
        {
            
        }
    }
}
