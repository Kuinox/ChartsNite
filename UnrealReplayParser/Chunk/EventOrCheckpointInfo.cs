using System;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public class EventOrCheckpointInfo
    {
        public readonly string Id;
        public readonly string Group;
        public readonly string Metadata;
        public readonly uint Time1;
        public readonly uint Time2;
        public readonly bool IsCheckpoint;

        public EventOrCheckpointInfo(string id, string group, string metadata, uint time1, uint time2)
        {
            Id = id;
            Group = group;
            Metadata = metadata;
            Time1 = time1;
            Time2 = time2;
        }

        protected EventOrCheckpointInfo(EventOrCheckpointInfo info)
        {
            Id = info.Id;
            Group = info.Group;
            Metadata = info.Metadata;
            Time1 = info.Time1;
            Time2 = info.Time2;
        }

        
    }
}
