using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;
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
        public virtual async Task<bool> ParseCheckpointHeader( ChunkReader chunkReader )
        {
            string id = await chunkReader.ReadString();
            string group = await chunkReader.ReadString();
            string metadata = await chunkReader.ReadString();
            uint time1 = await chunkReader.ReadUInt32();
            uint time2 = await chunkReader.ReadUInt32();
            int eventSizeInBytes = await chunkReader.ReadInt32();
            if( chunkReader.IsError && !await ErrorOnParseEventOrCheckpointHeader() || (!chunkReader.AssertRemainingCountOfBytes( eventSizeInBytes ) && !await ErrorOnParseEventOrCheckpointHeader()) )
            {
                return false;
            }
            return await ParseCheckpointContent( await chunkReader.UncompressDataIfNeeded(), id, group, metadata, time1, time2 );
        }

        public virtual Task<bool> ParseCheckpointContent( ChunkReader chunkReader, string id, string group, string metadata, uint time1, uint time2 )
        {
            return Task.FromResult( true );
        }

        public virtual Task<bool> ErrorOnParseEventOrCheckpointHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
    }
}
