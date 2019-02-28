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

        public ChunkReader(Stream stream, ReplayInfo replayInfo, bool leaveOpen = false ) : base( stream, leaveOpen )
        {
            ReplayInfo = replayInfo;
        }

        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// Will simply read the chunk of data and return it if it's not needed.
        /// I didn't test against not compressed replay, so it may fail.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns>Readable data uncompresed if it was needed</returns>
        public virtual async Task<ChunkReader> UncompressDataIfNeeded()//TODO change what i return
        {
            byte[] output;
            if( ReplayInfo.Compressed )
            {
                int decompressedSize = await ReadInt32();
                int compressedSize = await ReadInt32();
                byte[] compressedBuffer = await ReadBytes( compressedSize );
                output = OodleBinding.Decompress( compressedBuffer, compressedBuffer.Length, decompressedSize );
            }
            else
            {
                output = await DumpRemainingBytes();
            }
            return new ChunkReader( new MemoryStream( output ), ReplayInfo );
        }
    }
}
