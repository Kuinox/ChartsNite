using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public partial class UnrealReplayVisitor : IDisposable
    {

        public bool HaveLevelStreamingFixes()
        {
            return (DemoHeader!.HeaderFlags & DemoHeader.ReplayHeaderFlags.HasStreamingFixes) >= 0;
        }
        public virtual int ParsePacket( MemoryReader reader )
        {
            const int MaxBufferSize = 2 * 1024;
            int outBufferSize = reader.ReadInt32();
            if( outBufferSize > MaxBufferSize || outBufferSize < 0 )
            {
                throw new InvalidDataException( "Invalid packet size" );
            }
            if( outBufferSize == 0 ) return outBufferSize;
            var outBuffer = reader.ReadBytes( outBufferSize );
            ProcessRawPacket( new BitReader( outBuffer.Span.ToArray() ) ); //TODO avoid array alloc
            return outBufferSize;
        }
        /// <summary>
        /// Was writed to support how Fortnite store replays.
        /// This may need to be upgrade to support other games, or some future version of Fortnite.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns></returns>
        public virtual bool ParsePlaybackPacket( MemoryReader reader )
        {
            bool appendPacket = true;
            bool hasLevelStreamingFixes = true;//TODO: this method
            int currentLevelIndex = reader.ReadInt32();//TODO: use replayVersion. HasLevelStreamingFixes
            float timeSeconds = reader.ReadSingle();
            if( float.IsNaN( timeSeconds ) )
            {
                throw new InvalidDataException();
            }
            ParseExportData( reader );//TODO: use replayVersion. HasLevelStreamingFixes
            if( HaveLevelStreamingFixes() )
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
            if( HaveLevelStreamingFixes() )
            {
                skipExternalOffset = reader.ReadInt64();
            }
            else
            {
                throw new NotImplementedException();
            }

            ParseExternalData( reader );
            uint seenLevelIndex = 0;

            while( true )
            {
                if( hasLevelStreamingFixes )
                {
                    seenLevelIndex = reader.ReadIntPacked();
                }
                int amount = ParsePacket( reader );
                if( amount == 0 ) break;
                if( appendPacket ) continue;
            }//There is more data ?
            return true;
        }

        public virtual bool ParseExternalData( MemoryReader reader )
        {
            while( true )
            {
                uint externalDataBitsCount = reader.ReadIntPacked();
                if( externalDataBitsCount == 0 ) return true;
                uint netGuid = reader.ReadIntPacked();
                reader.Offset += (int)(externalDataBitsCount + 7) >> 3;//TODO: We dont do anything with it yet. We burn byte now.
            }
        }
        public virtual bool ProcessRawPacket( BitReader reader )
        {
            Incoming( reader );
            ReceivedPacket( reader );
            if( !reader.AtEnd )
            {
                if( DemoHeader!.EngineNetworkProtocolVersion < DemoHeader.EngineNetworkVersionHistory.HISTORY_ACKS_INCLUDED_IN_HEADER )
                {
                    reader.ReadBit();
                }

                int startPos = (int)reader.BitPosition;
                bool control = reader.ReadBit();
                bool open = control ? reader.ReadBit() : false;
                bool close = control ? reader.ReadBit() : false;

                if( DemoHeader.EngineNetworkProtocolVersion < DemoHeader.EngineNetworkVersionHistory.HISTORY_CHANNEL_CLOSE_REASON )
                {
                    bool dormant = close ? reader.ReadBit() : false;

                }
                else
                {
                    uint closeReason = close ? reader.ReadSerialisedInt( 15 ) : 0;
                }

                bool isReplicationPaused = reader.ReadBit();
                bool reliable = reader.ReadBit();

                if( DemoHeader.EngineNetworkProtocolVersion < DemoHeader.EngineNetworkVersionHistory.HISTORY_MAX_ACTOR_CHANNELS_CUSTOMIZATION )
                {
                    uint chIndex = reader.ReadSerialisedInt( 10240 );
                }
                else
                {
                    uint chIndex = reader.ReadIntPacked();//TODO: this don't work
                }
                bool hasPackageMapExports = reader.ReadBit();
                bool hasMustBeMappedGUIDs = reader.ReadBit();
                bool partial = reader.ReadBit();

                if( reliable )
                {
                    if( true )
                    {

                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if( partial )
                {

                }
                else
                {
                    //This is modifying a value but not reading int the reader
                }

                bool partialInitial = partial ? reader.ReadBit() : false;
                bool partialFinal = partial ? reader.ReadBit() : false;
                if( DemoHeader.EngineNetworkProtocolVersion < DemoHeader.EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if(reliable || open)
                    {

                    }
                }
            }
            return true;
        }
        static Stream FileDump = File.OpenWrite( "dumppackets.dump" );
        public virtual bool Incoming( BitReader reader )
        {
            reader.RemoveTrailingZeros();
            //We need to know the handlers components used in the


            //byte[] dump = bitReader.ReadBytes( (int)((bitReader.BitCount - bitReader.BitPosition) / 8) );
            //FileDump.Write( dump );
            //FileDump.Write( new byte[128 - (dump.Length % 128)] );
            //Console.WriteLine( BitConverter.ToString( dump ) );
            return true;
        }


        public virtual bool ReceivedPacket( BitReader reader )
        {
            ReadHeader( reader );
            ReadPacketInfo( reader );
            return true;
        }

        public virtual bool ReadHeader( BitReader reader )
        {
            uint header = reader.ReadUInt32();
            uint historyWordCount = GetHistoryWordCount( header );
            for( int i = 0; i < historyWordCount; i++ )
            {
                reader.ReadBit();
            }
            return true;
        }

        public virtual bool ReadPacketInfo( BitReader reader )
        {
            bool hasServerFrameTime = reader.ReadBit();
            if( hasServerFrameTime )
            {
                byte frameTimeByte = reader.ReadOneByte();
            }
            byte remoteInKBytesPerSecondByte = reader.ReadOneByte();
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
        public virtual bool ParseExportData( MemoryReader reader )
        {
            return ParseNetFieldExports( reader )
                && ParseNetExportGUIDs( reader );
        }
        public virtual bool ParseNetExportGUIDs( MemoryReader reader )
        {
            uint guidCount = reader.ReadIntPacked();
            for( int i = 0; i < guidCount; i++ )
            {
                reader.Offset = reader.ReadInt32() + reader.Offset;
            }
            return true;
        }
        public virtual bool ParseNetFieldExports( MemoryReader reader )
        {
            uint exportCount = reader.ReadIntPacked();
            for( int i = 0; i < exportCount; i++ )
            {
                uint pathNameIndex = reader.ReadIntPacked();
                uint wasExported = reader.ReadIntPacked();
                if( wasExported > 0 )
                {
                    string pathName = reader.ReadString();
                    uint numExports = reader.ReadIntPacked();
                }
                else
                {
                    //We does nothing here but Unreal does something
                }
                var netExports = reader.ReadNetFieldExport( DemoHeader!.EngineNetworkProtocolVersion );
            }
            return true;
        }
        #endregion ExportData
    }
}

