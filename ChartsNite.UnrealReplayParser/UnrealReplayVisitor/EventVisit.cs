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
        public virtual async ValueTask<bool> ParseEventHeader( CustomBinaryReaderAsync binaryReader )
        {
            string id = await binaryReader.ReadStringAsync();
            string group = await binaryReader.ReadStringAsync();
            string metadata = await binaryReader.ReadStringAsync();
            uint time1 = await binaryReader.ReadUInt32Async();
            uint time2 = await binaryReader.ReadUInt32Async();
            int eventSizeInBytes = await binaryReader.ReadInt32Async();
            return await ChooseEventChunkType( binaryReader, new EventOrCheckpointInfo( id, group, metadata, time1, time2 ) );
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
        public virtual ValueTask<bool> ChooseEventChunkType( CustomBinaryReaderAsync binaryReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>(true);
        }
    }
}
