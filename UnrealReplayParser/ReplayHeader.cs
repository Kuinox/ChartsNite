using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class ReplayHeader
    {
        public readonly int LengthInMs;
        public readonly uint NetworkVersion;
        public readonly uint Changelist;
        public readonly string FriendlyName;
        public readonly DateTime Timestamp;
        public readonly long TotalDataSizeInBytes;
        public readonly bool IsLive;
        public readonly bool Compressed;
        public readonly ReplayVersionHistory FileVersion;

        public ReplayHeader(int lengthInMs, uint networkVersion, uint changelist, string friendlyName, DateTime timestamp, long totalDataSizeInBytes, bool bIsLive, bool bCompressed, ReplayVersionHistory fileVersion )
        {
            LengthInMs = lengthInMs;
            NetworkVersion = networkVersion;
            Changelist = changelist;
            FriendlyName = friendlyName;
            Timestamp = timestamp;
            TotalDataSizeInBytes = totalDataSizeInBytes;
            IsLive = bIsLive;
            Compressed = bCompressed;
            FileVersion = fileVersion;
        }

        public enum ReplayVersionHistory
        {
            initial = 0,
            fixedSizeFriendlyName = 1,
            compression = 2,
            recordedTimestamp = 3,
            streamChunkTimes = 4,
            friendlyNameEncoding = 5,
            newVersion,
            latest = newVersion - 1
        };
    }
}
