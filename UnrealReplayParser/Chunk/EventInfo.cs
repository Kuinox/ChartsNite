using System;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public class EventInfo
    {
        public readonly string Id;
        public readonly string Group;
        public readonly string Metadata;
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly bool IsCheckpoint;

        public EventInfo(string id, string group, string metadata, uint time1, uint time2, bool isCheckpoint)
        {
            Id = id;
            Group = group;
            Metadata = metadata;
            Time1 = time1;
            Time2 = time2;
            IsCheckpoint = isCheckpoint;
        }

        protected EventInfo(EventInfo info)
        {
            Id = info.Id;
            Group = info.Group;
            Metadata = info.Metadata;
            Time1 = info.Time1;
            Time2 = info.Time2;
        }

        
    }
}
