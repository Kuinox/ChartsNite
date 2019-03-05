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
        public virtual async ValueTask<bool> ParseEventHeader( ChunkReader chunkReader )
        {
            string id = await chunkReader.ReadStringAsync();
            string group = await chunkReader.ReadStringAsync();
            string metadata = await chunkReader.ReadStringAsync();
            uint time1 = await chunkReader.ReadUInt32Async();
            uint time2 = await chunkReader.ReadUInt32Async();
            int eventSizeInBytes = await chunkReader.ReadInt32Async();
            if( chunkReader.IsError || (!chunkReader.AssertRemainingCountOfBytes( eventSizeInBytes ) && !await ErrorOnParseEventOrCheckpointHeader()) )
            {
                return false;
            }
            bool success = await ChooseEventChunkType( chunkReader, new EventOrCheckpointInfo( id, group, metadata, time1, time2 ));
            if(!success || chunkReader.IsError)
            {
                return await ErrorOnParseEventHeader();
            }
            return true;
        }
        public virtual ValueTask<bool> ErrorOnParseEventHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns>always <see langword="true"/></returns>
        public virtual ValueTask<bool> ChooseEventChunkType( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>(true);
        }
    }
}
