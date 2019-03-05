using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnrealReplayParser.Chunk
{
    public class ChunkReader : CustomBinaryReaderAsync
    {
        public readonly ReplayInfo ReplayInfo;
        public readonly ChunkType ChunkType;
        public ChunkReader(ChunkType chunkType, Stream stream, ReplayInfo replayInfo, bool leaveOpen = false ) : base( stream, leaveOpen )
        {
            ReplayInfo = replayInfo;
            ChunkType = chunkType;
        }

        public ChunkReader(ChunkType chunkType, ReplayInfo replayInfo) : base(new MemoryStream(), false)
        {
            ReplayInfo = replayInfo;
            ChunkType = chunkType;
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
                byte[] compressedBuffer = await ReadBytesAsync( compressedSize );
                output = OodleBinding.Decompress( compressedBuffer, compressedBuffer.Length, decompressedSize );
            }
            else
            {
                output = await DumpRemainingBytes();
            }
            return new ChunkReader(ChunkType, new MemoryStream( output ), ReplayInfo );
        }
    }
}
