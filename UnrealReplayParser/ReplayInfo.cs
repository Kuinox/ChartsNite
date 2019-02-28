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
        public readonly bool IsLive;
        /// <summary>
        /// Always true on Fortnite replay
        /// </summary>
        public readonly bool Compressed;
        public readonly ReplayVersionHistory FileVersion;

        public ReplayInfo(int lengthInMs, uint networkVersion, uint changelist, string friendlyName, DateTime timestamp, long totalDataSizeInBytes, bool bIsLive, bool bCompressed, ReplayVersionHistory fileVersion )
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
