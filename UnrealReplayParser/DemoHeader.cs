using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class DemoHeader
    {
        public DemoHeader( NetworkVersionHistory version, uint networkChecksum, uint engineNetworkProtocolVersion, uint gameNetworkProtocolVerrsion, byte[] guid, ushort major, ushort minor, ushort patch, uint changeList, string branch, (string, uint)[] levelNamesAndTimes, ReplayHeaderFlags headerFlags, string[] gameSpecificData )
        {
            Version = version;
            NetworkChecksum = networkChecksum;
            EngineNetworkProtocolVersion = engineNetworkProtocolVersion;
            GameNetworkProtocolVerrsion = gameNetworkProtocolVerrsion;
            Guid = guid;
            Major = major;
            Minor = minor;
            Patch = patch;
            ChangeList = changeList;
            Branch = branch;
            LevelNamesAndTimes = levelNamesAndTimes;
            HeaderFlags = headerFlags;
            GameSpecificData = gameSpecificData;
        }

        public NetworkVersionHistory Version { get; }
        public uint NetworkChecksum { get; }
        public uint EngineNetworkProtocolVersion { get; }
        public uint GameNetworkProtocolVerrsion { get; }
        public byte[] Guid { get; }
        public ushort Major { get; }
        public ushort Minor { get; }
        public ushort Patch { get; }
        public uint ChangeList { get; }
        public string Branch { get; }
        public (string, uint)[] LevelNamesAndTimes { get; }
        public ReplayHeaderFlags HeaderFlags { get; }
        public string[] GameSpecificData { get; }

        public enum NetworkVersionHistory
        {
            initial = 1,
            absoluteTime = 2,               // We now save the abs demo time in ms for each frame (solves accumulation errors)
            increasedBuffer = 3,            // Increased buffer size of packets, which invalidates old replays
            engineVersion = 4,              // Now saving engine net version + InternalProtocolVersion
            extraVersion = 5,               // We now save engine/game protocol version, checksum, and changelist
            multiLevels = 6,                // Replays support seamless travel between levels
            multiLevelTimeChange = 7,       // Save out the time that level changes happen
            deletedStartupActors = 8,       // Save DeletedNetStartupActors inside checkpoints
            demoHeaderEnumFlags = 9,        // Save out enum flags with demo header
            levelStreamingFixes = 10,       // Optional level streaming fixes.
            saveFullEngineVersion = 11,     // Now saving the entire FEngineVersion including branch name
            guidDemoHeader = 12,            // Save guid to demo header
            historyCharacterMovement = 13,  // Change to using replicated movement and not interpolation
            newVersion,
            latest = newVersion - 1
        };
        public enum ReplayHeaderFlags
        {
            None = 0,
            ClientRecorded = (1 << 0),
            HasStreamingFixes = (1 << 1),
        };
    }
}
