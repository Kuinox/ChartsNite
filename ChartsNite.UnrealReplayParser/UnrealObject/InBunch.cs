using ChartsNite.UnrealReplayParser.StreamArchive;
using System;
using System.Collections.Generic;
using System.Text;
using ChartsNite.UnrealReplayParser.UnrealObject;
using static UnrealReplayParser.DemoHeader;
using static ChartsNite.UnrealReplayParser.UnrealObject.InBunch;
using UnrealReplayParser.UnrealObject;

namespace ChartsNite.UnrealReplayParser.UnrealObject
{
    public class InBunch
    {
        public readonly InBunch? Next;
        public readonly int ChIndex;
        public readonly ChannelType ChType;
        public readonly string ChName;
        public readonly int ChSequence;
        public readonly bool Open;
        public readonly bool Close;
        public readonly bool Dormant;
        public readonly bool IsReplicationPaused;
        public readonly bool Reliable;
        public readonly bool Partial;
        public readonly bool PartialInitial;
        public readonly bool PartialFinal;
        public readonly bool HasPackageMapExports;
        public readonly bool HasMustBeMappedGUIDs;
        public readonly bool IgnoreRPCs;
        public readonly ChannelCloseReason CloseReason;
        public InBunch(
            InBunch? next,
            int chIndex,
            ChannelType chType,
            string chName,
            int chSequence,
            bool open,
            bool close,
            bool dormant,
            bool isReplicationPaused,
            bool reliable,
            bool partial,
            bool partialInitial,
            bool partialFinal,
            bool hasPackageMapExports,
            bool hasMustBeMappedGUIDs,
            bool ignoreRPCs )
        {
            Next = next;
            ChIndex = chIndex;
            ChType = chType;
            ChName = chName;
            ChSequence = chSequence;
            Open = open;
            Close = close;
            Dormant = dormant;
            IsReplicationPaused = isReplicationPaused;
            Reliable = reliable;
            Partial = partial;
            PartialInitial = partialInitial;
            PartialFinal = partialFinal;
            HasPackageMapExports = hasPackageMapExports;
            HasMustBeMappedGUIDs = hasMustBeMappedGUIDs;
            IgnoreRPCs = ignoreRPCs;
        }
        public enum ChannelCloseReason : byte
        {
            Destroyed,
            Dormancy,
            LevelUnloaded,
            Relevancy,
            TearOff,
            MAX = 15
        }

        public enum ChannelType
        {
            CHTYPE_None = 0,  // Invalid type.
            CHTYPE_Control = 1,  // Connection control.
            CHTYPE_Actor = 2,  // Actor-update channel.

            CHTYPE_File = 3,  // Binary file transfer.

            CHTYPE_Voice = 4,  // VoIP data channel
            CHTYPE_MAX = 8,  // Maximum.
        };


    }

    public static class InBunchParsing
    {
        public static InBunch ReadInBunch( this Archive ar )
        {
            bool control = ar.ReadBit();
            bool open = control ? ar.ReadBit() : false;
            bool close = control ? ar.ReadBit() : false;
            bool dormant;
            ChannelCloseReason closeReason;
            if( ar.DemoHeader.EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_CHANNEL_CLOSE_REASON )
            {
                dormant = close ? ar.ReadBit() : false;
                closeReason = dormant ? ChannelCloseReason.Dormancy : ChannelCloseReason.Destroyed;
            }
            else
            {
                closeReason = close ? (ChannelCloseReason)ar.ReadUInt32( (uint)ChannelCloseReason.MAX ) : ChannelCloseReason.Destroyed;
                dormant = closeReason == ChannelCloseReason.Dormancy;
            }

            bool isReplicationPaused = ar.ReadBit();
            bool reliable = ar.ReadBit();
            uint chIndex;
            if( ar.DemoHeader.EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_MAX_ACTOR_CHANNELS_CUSTOMIZATION )
            {
                chIndex = ar.ReadUInt32( 10240 );
            }
            else
            {
                chIndex = ar.ReadIntPacked();
            }
            bool hasPackageMapExports = ar.ReadBit();
            bool hasMustBeMappedGUIDs = ar.ReadBit();
            bool partial = ar.ReadBit();

            int chSequence;
            if( reliable )
            {
                bool internalAck = true;//maybe one day it will be usefull.
                if( internalAck )
                {
                    //todo, currently seeing no usage.
                    chSequence = -1;
                }
                else
                {
                    throw new NotImplementedException(); //yes, what is the point of the 'if' you will ask, well, it's 3:00 AM when i wrote that.
                }
            }
            else if( partial )
            {
                //todo, currently seeing no usage.
                chSequence = -1;//inPacketId
            }
            else
            {
                chSequence = 0;
            }

            bool partialInitial = partial ? ar.ReadBit() : false;
            bool partialFinal = partial ? ar.ReadBit() : false;

            ChannelType chType = ChannelType.CHTYPE_None;
            string chName;
            if( ar.DemoHeader.EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
            {
                chType = (reliable || open) ? (ChannelType)ar.ReadUInt32( (uint)ChannelType.CHTYPE_MAX ) : ChannelType.CHTYPE_None;
                switch( chType )
                {
                    case ChannelType.CHTYPE_Control:
                        chName = FName.GetName( FName.FNameId.Control );
                        break;
                    case ChannelType.CHTYPE_Actor:
                        chName = FName.GetName( FName.FNameId.Voice );
                        break;
                    case ChannelType.CHTYPE_Voice:
                        chName = FName.GetName( FName.FNameId.Voice );
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                if( reliable || open )
                {
                    chName = ar.ReadStaticName().Name;
                    if( chName == FName.GetName( FName.FNameId.Control ) )
                    {
                        chType = ChannelType.CHTYPE_Control;
                    }
                    else if( chName == FName.GetName( FName.FNameId.Voice ) )
                    {
                        chType = ChannelType.CHTYPE_Voice;
                    }
                    else if( chName == FName.GetName( FName.FNameId.Actor ) )
                    {
                        chType = ChannelType.CHTYPE_Actor;
                    }
                }
                else
                {
                    chType = ChannelType.CHTYPE_None;
                    chName = FName.GetName( FName.FNameId.None );
                }
            }
            return new InBunch( null, (int)chIndex, chType, chName, chSequence, open, close, dormant, isReplicationPaused, reliable, partial, partialInitial, partialFinal, hasPackageMapExports, hasMustBeMappedGUIDs, false );
        }
    }
}
