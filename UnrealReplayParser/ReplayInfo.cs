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
        public readonly uint FileVersion;

        public ReplayInfo(int lengthInMs, uint networkVersion, uint changelist, string friendlyName, DateTime timestamp, long totalDataSizeInBytes, bool bIsLive, bool bCompressed, uint fileVersion)
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


        public enum ENetworkVersionHistory
        {
            HISTORY_REPLAY_INITIAL = 1,
            HISTORY_SAVE_ABS_TIME_MS = 2,               // We now save the abs demo time in ms for each frame (solves accumulation errors)
            HISTORY_INCREASE_BUFFER = 3,                // Increased buffer size of packets, which invalidates old replays
            HISTORY_SAVE_ENGINE_VERSION = 4,            // Now saving engine net version + InternalProtocolVersion
            HISTORY_EXTRA_VERSION = 5,                  // We now save engine/game protocol version, checksum, and changelist
            HISTORY_MULTIPLE_LEVELS = 6,                // Replays support seamless travel between levels
            HISTORY_MULTIPLE_LEVELS_TIME_CHANGES = 7,   // Save out the time that level changes happen
            HISTORY_DELETED_STARTUP_ACTORS = 8,         // Save DeletedNetStartupActors inside checkpoints
            HISTORY_HEADER_FLAGS = 9,                   // Save out enum flags with demo header
            HISTORY_LEVEL_STREAMING_FIXES = 10,         // Optional level streaming fixes.
            HISTORY_SAVE_FULL_ENGINE_VERSION = 11,      // Now saving the entire FEngineVersion including branch name
            HISTORY_HEADER_GUID = 12,                   // Save guid to demo header
            HISTORY_CHARACTER_MOVEMENT = 13,            // Change to using replicated movement and not interpolation

            // -----<new versions can be added before this line>-------------------------------------------------
            HISTORY_PLUS_ONE,
            HISTORY_LATEST = HISTORY_PLUS_ONE - 1
        }
    }
}
