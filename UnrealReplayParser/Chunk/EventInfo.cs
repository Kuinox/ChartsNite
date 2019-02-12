using System;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public class EventInfo : ChunkInfo
    {
        public readonly string Id;
        public readonly string Group;
        public readonly string Metadata;
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly int EventSizeInBytes;
        public readonly bool IsCheckpoint;

        public EventInfo(ChunkInfo info, string id, string group, string metadata, uint time1, uint time2, int eventSizeInBytes, bool isCheckpoint) : base(info)
        {
            if(info.Type != (int)ChunkType.Event && info.Type != (int)ChunkType.Checkpoint) throw new InvalidOperationException();
            Id = id;
            Group = group;
            Metadata = metadata;
            Time1 = time1;
            Time2 = time2;
            EventSizeInBytes = eventSizeInBytes;
            IsCheckpoint = isCheckpoint;
        }

        protected EventInfo(EventInfo info) : base(info)
        {
            Id = info.Id;
            Group = info.Group;
            Metadata = info.Metadata;
            Time1 = info.Time1;
            Time2 = info.Time2;
            EventSizeInBytes = info.EventSizeInBytes;
        }

        
    }
}
