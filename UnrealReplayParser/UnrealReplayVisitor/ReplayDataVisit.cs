using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;
using UnrealReplayParser.UnrealObject;
using static UnrealReplayParser.ReplayHeader;

namespace UnrealReplayParser
{
    /// <summary>
    /// Start to read the header, then the chunks of the replay.
    /// Lot of error handling to process the headers:
    /// If the replay header is not correctly parsed, we can't read the replay.
    /// If a chunk header is not correctly parsed, we don't know where stop this chunk and where start the next.
    /// When we read the content of the chunk everything is protected, so there is less error handling
    /// </summary>
    public partial class UnrealReplayVisitor : IDisposable
    {
        public virtual async ValueTask<bool> ParseReplayDataChunkHeader( CustomBinaryReaderAsync chunkReader )
        {
            uint time1 = uint.MaxValue;
            uint time2 = uint.MaxValue;
            if( ReplayHeader!.FileVersion >= ReplayVersionHistory.streamChunkTimes )
            {
                time1 = await chunkReader.ReadUInt32Async();
                time2 = await chunkReader.ReadUInt32Async();
                int replaySizeInBytes = await chunkReader.ReadInt32Async();
            }
            using( CustomBinaryReader uncompressedData = await chunkReader.UncompressData() )//TODO: check compress
            {
                return ParseReplayData( uncompressedData );
            }

        }

        public virtual bool ErrorOnParseReplayDataChunk()
        {
            return ErrorOnChunkContentParsing();
        }

        public virtual bool ParseReplayData( CustomBinaryReader streamReader )
        {
            if( !streamReader.BaseStream.CanSeek ) throw new ArgumentException();
            while( streamReader.BaseStream.Length > streamReader.BaseStream.Position )
            {
                if( !ParsePlaybackPacket( streamReader ) )
                {
                    return false;
                }
            }
            return true;
        }



        public virtual (bool success, int amount) ParsePacket( CustomBinaryReader chunkReader )
        {
            const int MaxBufferSize = 2 * 1024;
            int outBufferSize = chunkReader.ReadInt32();
            if( outBufferSize > MaxBufferSize || outBufferSize < 0 )
            {
                throw new InvalidDataException( "Invalid packet size" );
            }
            if( outBufferSize == 0 ) return (true, outBufferSize);
            byte[] outBuffer = chunkReader.ReadBytes( outBufferSize );
            ProcessRawPacket( new BitReader( outBuffer ) );
            return (true, outBufferSize);
        }

        public virtual bool ProcessRawPacket( BitReader bitReader )
        {
            Incoming( bitReader );
            //bool handshakePacket = bitReader.ReadBit();
            //if( handshakePacket )
            //{
            //    return true;//Never had a handshake packet.
            //}
            ////if( bitReader.BitRemaining > 0 )
            ////{

            ////}
            //else
            //{
            //    return true;//packet has been consumed
            //}
            return true;
        }

        public virtual bool Incoming( BitReader bitReader )
        {
            bitReader.RemoveTrailingZeros();
            //We need to know the handlers components used in the 
            byte[] dump = bitReader.ReadBytes( (int)((bitReader.BitCount - bitReader.BitPosition) / 8) );
            //Console.WriteLine( BitConverter.ToString( dump ) );
            return true;
        }
        //public virtual bool ProcessIncomingPacket( BitReader bitReader )
        //{

        //}

        public virtual bool ParseExternalData( CustomBinaryReader chunkReader )
        {
            while( true )
            {
                uint externalDataBitsCount = chunkReader.ReadIntPacked();
                if( externalDataBitsCount == 0 )
                {
                    return true;
                }
                uint netGuid = chunkReader.ReadIntPacked();
                int byteCount = (int)(externalDataBitsCount + 7) >> 3;
                byte[] burnBytes = chunkReader.ReadBytes( byteCount );//We dont do anything with it yet.
            }
        }

        public virtual bool ParseExportData( CustomBinaryReader binaryReader )
        {
            return ParseNetFieldExports( binaryReader )
                && ParseNetExportGUIDs( binaryReader );
        }

        public virtual bool ParseNetFieldExports( CustomBinaryReader binaryReader )
        {
            uint exportCount = binaryReader.ReadIntPacked();
            for( int i = 0; i < exportCount; i++ )
            {
                uint pathNameIndex = binaryReader.ReadIntPacked();
                uint wasExported = binaryReader.ReadIntPacked();
                if( wasExported > 0 )
                {
                    string pathName = binaryReader.ReadString();
                    uint numExports = binaryReader.ReadIntPacked();
                }
                else
                {
                    //We does nothing here but Unreal does something
                }
                var netExports = binaryReader.ReadNetFieldExport( DemoHeader!.EngineNetworkProtocolVersion );
            }
            return true;
        }

        public virtual bool ParseNetExportGUIDs( CustomBinaryReader binaryReader )
        {
            uint guidCount = binaryReader.ReadIntPacked();
            for( int i = 0; i < guidCount; i++ )
            {
                byte[] guidData = binaryReader.ReadBytes( binaryReader.ReadInt32() );
                //TODO: use memory reader
            }
            return true;
        }
        public virtual bool ErrorOnParseNetFieldExports()
        {
            return ErrorOnParseReplayDataChunk();
        }
    }
}
