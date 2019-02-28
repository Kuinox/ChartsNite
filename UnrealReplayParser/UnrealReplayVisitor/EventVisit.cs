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
        public virtual async Task<bool> ParseEventHeader( ChunkReader chunkReader )
        {
            string id = await chunkReader.ReadString();
            string group = await chunkReader.ReadString();
            string metadata = await chunkReader.ReadString();
            uint time1 = await chunkReader.ReadUInt32();
            uint time2 = await chunkReader.ReadUInt32();
            int eventSizeInBytes = await chunkReader.ReadInt32();
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
        public virtual Task<bool> ErrorOnParseEventHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns>always <see langword="true"/></returns>
        public virtual Task<bool> ChooseEventChunkType( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
    }
}
