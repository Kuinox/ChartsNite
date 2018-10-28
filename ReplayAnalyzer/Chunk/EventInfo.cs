using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReplayAnalyzer
{
    public class EventInfo : ChunkInfo
    {
        public readonly int ChunkIndex;
        public readonly string Id;
        public readonly string Group;
        public readonly string Metadata;
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly int EventSizeInBytes;
        public readonly long EventDataOffset;

        public EventInfo(ChunkInfo info, int chunkIndex, string id, string group, string metadata, uint time1, uint time2, int eventSizeInBytes, long eventDataOffset) : base(info)
        {
            if(info.Type != ChunkType.Event && info.Type != ChunkType.Checkpoint) throw new InvalidDataException();
            ChunkIndex = chunkIndex;
            Id = id;
            Group = group;
            Metadata = metadata;
            Time1 = time1;
            Time2 = time2;
            EventSizeInBytes = eventSizeInBytes;
            EventDataOffset = eventDataOffset;
        }
        public EventInfo(EventInfo info) : base(info)
        {
            ChunkIndex = info.ChunkIndex;
            Id = info.Id;
            Group = info.Group;
            Metadata = info.Metadata;
            Time1 = info.Time1;
            Time2 = info.Time2;
            EventSizeInBytes = info.EventSizeInBytes;
            EventDataOffset = info.EventDataOffset;
        }
    }
}
