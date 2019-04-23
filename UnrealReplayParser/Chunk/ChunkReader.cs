using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static UnrealReplayParser.DemoHeader;

namespace UnrealReplayParser.Chunk
{
    public class ChunkReader : CustomBinaryReaderAsync//TODO: only sync
    {
        public readonly ReplayInfo ReplayInfo;
        public EngineNetworkVersionHistory EngineNetworkProtocolVersion => ReplayInfo.DemoHeader.EngineNetworkProtocolVersion;
        public readonly ChunkType ChunkType;
        public ChunkReader( ChunkType chunkType, Stream stream, ReplayInfo replayInfo, bool leaveOpen = false ) : base( stream, leaveOpen )
        {
            ReplayInfo = replayInfo;
            ChunkType = chunkType;
        }

        public ChunkReader( ChunkType chunkType, ReplayInfo replayInfo ) : base( new MemoryStream(), false )
        {
            ReplayInfo = replayInfo;
            ChunkType = chunkType;
        }

        void StaticParseName()
        {
            byte b = ReadOneByte();
            bool hardcoded = b != 0;
            if( hardcoded )
            {
                if( EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
                {
                    ReadInt32();
                }
                else
                {
                    ReadIntPacked();
                }

                //hard coded names in "UnrealNames.inl"
            }
            else
            {
                string inString = ReadString();
                int inNumber = ReadInt32();
            }
        }

        public NetFieldExport ReadNetFieldExport()
        {
            byte flags = ReadOneByte();
            bool exported = 1 == flags;
            if( !exported )
            {
                return NetFieldExport.InitializeNotExported();
            }
            uint handle = ReadIntPacked();
            uint compatibleChecksum = ReadUInt32();

            if( EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZATION )
            {
                string name = ReadString();
                string type = ReadString();
            }
            else
            {
                if( EngineNetworkProtocolVersion < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZE_FIX )
                {
                    throw new NotImplementedException();
                }
                else
                {
                    StaticParseName();
                }
            }


            return NetFieldExport.InitializeExported( handle, compatibleChecksum, "", "" );
        }

        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// Will simply read the chunk of data and return it if it's not needed.
        /// I didn't test against not compressed replay, so it may fail.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns>Readable data uncompresed if it was needed</returns>
        public virtual async ValueTask<ChunkReader> UncompressDataIfNeeded()//TODO change what i return
        {
            byte[] output;
            if( ReplayInfo.ReplayHeader.Compressed )
            {
                int decompressedSize = await ReadInt32Async();
                int compressedSize = await ReadInt32Async();
                byte[] compressedBuffer = (await ReadBytesAsync( compressedSize )).ToArray();//TODO: Use Memory<T>
                output = OodleBinding.Decompress( compressedBuffer, compressedBuffer.Length, decompressedSize );
            }
            else
            {
                output = await DumpRemainingBytesAsync();
            }
            return new ChunkReader( ChunkType, new MemoryStream( output ), ReplayInfo );
        }
    }
}
