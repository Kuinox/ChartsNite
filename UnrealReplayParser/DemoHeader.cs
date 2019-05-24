using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class DemoHeader
    {
        public DemoHeader( NetworkVersionHistory version, uint networkChecksum, EngineNetworkVersionHistory engineNetworkProtocolVersion, uint gameNetworkProtocolVerrsion, Guid guid, ushort major, ushort minor, ushort patch, uint changeList, string branch, (string, uint)[] levelNamesAndTimes, ReplayHeaderFlags headerFlags, string[] gameSpecificData )
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
        public EngineNetworkVersionHistory EngineNetworkProtocolVersion { get; }
        public uint GameNetworkProtocolVerrsion { get; }
        public Guid Guid { get; }
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

        public enum EngineNetworkVersionHistory
        {
            HISTORY_INITIAL = 1,
            HISTORY_REPLAY_BACKWARDS_COMPAT = 2,            // Bump version to get rid of older replays before backwards compat was turned on officially
            HISTORY_MAX_ACTOR_CHANNELS_CUSTOMIZATION = 3,   // Bump version because serialization of the actor channels changed
            HISTORY_REPCMD_CHECKSUM_REMOVE_PRINTF = 4,      // Bump version since the way FRepLayoutCmd::CompatibleChecksum was calculated changed due to an optimization
            HISTORY_NEW_ACTOR_OVERRIDE_LEVEL = 5,           // Bump version since a level reference was added to the new actor information
            HISTORY_CHANNEL_NAMES = 6,                      // Bump version since channel type is now an fname
            HISTORY_CHANNEL_CLOSE_REASON = 7,               // Bump version to serialize a channel close reason in bunches instead of bDormant
            HISTORY_ACKS_INCLUDED_IN_HEADER = 8,            // Bump version since acks are now sent as part of the header
            HISTORY_NETEXPORT_SERIALIZATION = 9,            // Bump version due to serialization change to FNetFieldExport
            HISTORY_NETEXPORT_SERIALIZE_FIX = 10,           // Bump version to fix net field export name serialization
        };
        public enum ReplayHeaderFlags
        {
            None = 0,
            ClientRecorded = (1 << 0),
            HasStreamingFixes = (1 << 1),
        };
    }
}
