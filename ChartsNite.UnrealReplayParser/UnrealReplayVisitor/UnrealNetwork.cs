using ChartsNite.UnrealReplayParser.StreamArchive;
using ChartsNite.UnrealReplayParser.UnrealObject;
using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnrealReplayParser.Chunk;
using UnrealReplayParser.UnrealObject;

namespace UnrealReplayParser
{
    public partial class UnrealReplayVisitor : IDisposable
    {

        public bool HasLevelStreamingFixes()
        {
            return (DemoHeader!.HeaderFlags & DemoHeader.ReplayHeaderFlags.HasStreamingFixes) >= 0;
        }
        public enum ReadPacketState
        {
            Success,
            End,
            Error
        }

        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/7d9919ac7bfd80b7483012eab342cb427d60e8c9/Engine/Source/Runtime/Engine/Private/DemoNetDriver.cpp#L3220
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual ReadPacketState ParsePacket( ChunkArchive reader )
        {
            const int MaxBufferSize = 2 * 1024;
            int bufferSize = reader.ReadInt32();
            if( bufferSize > MaxBufferSize || bufferSize < 0 ) return ReadPacketState.Error;
            if( bufferSize == 0 ) return ReadPacketState.End;
            var buffer = reader.HeapReadBytes( bufferSize );
            if( ProcessRawPacket( new BitArchive( buffer, DemoHeader!, ReplayHeader! ) ) ) return ReadPacketState.Success;
            return ReadPacketState.Error;
        }
        /// <summary>
        /// Was writed to support how Fortnite store replays.
        /// This may need to be upgrade to support other games, or some future version of Fortnite.
        /// https://github.com/EpicGames/UnrealEngine/blob/7d9919ac7bfd80b7483012eab342cb427d60e8c9/Engine/Source/Runtime/Engine/Private/DemoNetDriver.cpp#L2848
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns></returns>
        public virtual bool ParsePlaybackPacket( ChunkArchive reader )
        {
            if( DemoHeader!.NetworkVersion >= DemoHeader.NetworkVersionHistory.multipleLevels )
            {
                int currentLevelIndex = reader.ReadInt32();
            }
            float timeSeconds = reader.ReadSingle();
            if( float.IsNaN( timeSeconds ) )
            {
                throw new InvalidDataException();
            }
            if( DemoHeader!.NetworkVersion >= DemoHeader.NetworkVersionHistory.levelStreamingFixes )
            {
                ParseExportData( reader );
            }
            if( HasLevelStreamingFixes() )
            {
                uint streamingLevelscount = reader.ReadIntPacked();
                for( int i = 0; i < streamingLevelscount; i++ )
                {
                    string levelName = reader.ReadString();
                }
            }
            else
            {
                throw new NotSupportedException( "TODO" );
            }
            long skipExternalOffset = 0;
            if( HasLevelStreamingFixes() )
            {
                skipExternalOffset = reader.ReadInt64();
            }
            else
            {
                throw new NotImplementedException();
            }

            ParseExternalData( reader );
            return ReadPackets( reader );
        }

        public virtual bool ReadPackets( ChunkArchive reader )
        {
            uint seenLevelIndex = 0;
            while( true )
            {
                if( HasLevelStreamingFixes() )
                {
                    seenLevelIndex = reader.ReadIntPacked();
                }
                var result = ParsePacket( reader );
                switch( result )
                {
                    case ReadPacketState.Success:
                        return true;//TO REMOVE
                        continue;
                    case ReadPacketState.End:
                        return true;
                    case ReadPacketState.Error:
                        return false;
                    default:
                        throw new InvalidOperationException();
                }
            }//There is more data ?
        }
        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/7d9919ac7bfd80b7483012eab342cb427d60e8c9/Engine/Source/Runtime/Engine/Private/DemoNetDriver.cpp#L2106
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual bool ParseExternalData( ChunkArchive reader )
        {
            while( true )
            {
                uint externalDataBitsCount = reader.ReadIntPacked();
                if( externalDataBitsCount == 0 ) return true;
                uint netGuid = reader.ReadIntPacked();
                reader.ReadBytes( (int)(externalDataBitsCount + 7) >> 3 );//TODO: We dont do anything with it yet. We burn byte now.
            }
        }
        public virtual bool ProcessRawPacket( BitArchive reader )
        {
            Incoming( reader );
            ReceivedPacket( reader );
            if( reader.Offset == reader.Length ) return true;

            return true;
        }
        static Stream FileDump = File.OpenWrite( "dumppackets.dump" );
        public virtual bool Incoming( BitArchive reader )
        {
            reader.RemoveTrailingZeros();
            //We need to know the handlers components used in the


            //byte[] dump = bitReader.ReadBytes( (int)((bitReader.BitCount - bitReader.BitPosition) / 8) );
            //FileDump.Write( dump );
            //FileDump.Write( new byte[128 - (dump.Length % 128)] );
            //Console.WriteLine( BitConverter.ToString( dump ) );
            return true;
        }

        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Engine/Private/NetConnection.cpp#L1525
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public virtual bool ReceivedPacket( BitArchive ar )
        {
            ReadHeader( ar );
            ReadPacketInfo( ar );

            //TODO LOOP while( !Reader.AtEnd() && State!=USOCK_Closed )
            //while( ar.Offset < ar.Length )
            {

                if( DemoHeader!.EngineNetworkProtocolVersion < DemoHeader.EngineNetworkVersionHistory.HISTORY_ACKS_INCLUDED_IN_HEADER )
                {
                    ar.ReadBit();
                }

                InBunch bunch = ar.ReadInBunch();

                int bunchDataBits = (int)ar.ReadUInt32( 2048 * 8 );
            }
            return true;
        }

        public virtual bool ReadHeader( BitArchive reader )
        {
            uint header = reader.ReadUInt32();
            uint historyWordCount = GetHistoryWordCount( header );
            for( int i = 0; i < historyWordCount; i++ )
            {
                reader.ReadBit();
            }
            return true;
        }

        public virtual bool ReadPacketInfo( BitArchive reader )
        {
            bool hasServerFrameTime = reader.ReadBit();
            if( hasServerFrameTime )
            {
                byte frameTimeByte = reader.ReadByte();
            }
            byte remoteInKBytesPerSecondByte = reader.ReadByte();
            return true;
        }

        public virtual uint GetHistoryWordCount( uint Packed ) { return Packed & (int)HistoryWordCountMask; }

        const uint HistoryWordCountBits = 4;
        const uint SeqMask = (1 << (int)SequenceNumberBits) - 1;
        const uint HistoryWordCountMask = (1 << (int)HistoryWordCountBits) - 1;
        const uint AckSeqShift = HistoryWordCountBits;
        const uint SeqShift = AckSeqShift + SequenceNumberBits;
        const uint SequenceNumberBits = 14;
        const uint MaxSequenceHistoryLength = 256;
        #region ExportData
        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Engine/Private/PackageMapClient.cpp#L1348
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual bool ParseExportData( ChunkArchive reader )
        {
            return ParseNetFieldExports( reader )
                && ParseNetExportGUIDs( reader );
        }
        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Engine/Private/PackageMapClient.cpp#L1579
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual bool ParseNetExportGUIDs( ChunkArchive reader )
        {
            uint guidCount = reader.ReadIntPacked();
            for( int i = 0; i < guidCount; i++ )
            {
                reader.ReadBytes( reader.ReadInt32() );//burn.
            }
            return true;
        }
        /// <summary>
        /// https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Engine/Private/PackageMapClient.cpp#L1497
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual bool ParseNetFieldExports( ChunkArchive reader )
        {
            uint exportCount = reader.ReadIntPacked();
            for( int i = 0; i < exportCount; i++ )
            {
                uint pathNameIndex = reader.ReadIntPacked();
                uint wasExported = reader.ReadIntPacked();
                Debug.Assert( wasExported == 0 || wasExported == 1 );
                if( wasExported > 0 )
                {
                    string pathName = reader.ReadString();
                    uint numExports = reader.ReadIntPacked();
                }
                else
                {
                    //We does nothing here but Unreal does something
                }
                var netExports = reader.ReadNetFieldExport();
            }
            return true;
        }
        #endregion ExportData
    }
}

