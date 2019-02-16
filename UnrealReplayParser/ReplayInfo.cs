using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class ReplayInfo
    {
        public readonly int LengthInMs;
        /// <summary>
        /// Always 2
        /// </summary>
        public readonly uint NetworkVersion;
        /// <summary>
        /// moved from 3 to 5
        /// </summary>
        public readonly uint Changelist;
        public readonly string FriendlyName;
        public readonly DateTime Timestamp;
        /// <summary>
        /// Not used !
        /// </summary>
        public readonly long TotalDataSizeInBytes;
        /// <summary>
        /// Actually used
        /// </summary>
        public readonly bool BIsLive;
        /// <summary>
        /// Always true on Fortnite replay
        /// </summary>
        public readonly bool BCompressed;
        /// <summary>
        /// This is <see cref="null"/> until the <see cref="ChunkParser"/> have read the Header Chunk
        /// </summary>

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
