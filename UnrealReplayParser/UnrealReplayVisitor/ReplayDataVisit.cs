using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            using( IMemoryOwner<byte> uncompressedData = await chunkReader.UncompressData() )//TODO: check compress
            {
                //return ParseReplayData( new MemoryReader( uncompressedData.Memory, Endianness.Native ));
            }
            return true;

        }

        public virtual bool ParseReplayData( MemoryReader streamReader )
        {
            while( streamReader.Length > streamReader.Offset )
            {
                if( !ParsePlaybackPacket( streamReader ) )
                {
                    return false;
                }
            }
            return true;
        }
    }
}
