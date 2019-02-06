using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class ReplayInfo
    {
        public readonly int LengthInMs;
        public readonly uint NetworkVersion;
        public readonly uint Changelist;
        public readonly string FriendlyName;
        public readonly DateTime Timestamp;
        public readonly long TotalDataSizeInBytes;
        public readonly bool BIsLive;
        public readonly bool BCompressed;
        /// <summary>
        /// This is <see cref="null"/> until the <see cref="ChunkReader"/> have read the Header Chunk
        /// </summary>
        public ChunkInfo? HeaderChunk;

        public readonly uint FileVersion;

        public ReplayInfo(int lengthInMs, uint networkVersion, uint changelist, string friendlyName, DateTime timestamp, long totalDataSizeInBytes, bool bIsLive, bool bCompressed, uint fileVersion)
        {
            LengthInMs = lengthInMs;
            NetworkVersion = networkVersion;
            Changelist = changelist;
            FriendlyName = friendlyName;
            Timestamp = timestamp;
            TotalDataSizeInBytes = totalDataSizeInBytes;
            BIsLive = bIsLive;
            BCompressed = bCompressed;
            FileVersion = fileVersion;
        }
    }
}
