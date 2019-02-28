using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;
using static UnrealReplayParser.ReplayInfo;

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
        public virtual async Task<bool> ParseReplayDataChunkHeader( ChunkReader chunkReader )
        {
            bool correctSize = true;
            uint time1 = uint.MaxValue;
            uint time2 = uint.MaxValue;
            if( chunkReader.ReplayInfo.FileVersion >= ReplayVersionHistory.streamChunkTimes )
            {
                time1 = await chunkReader.ReadUInt32();
                time2 = await chunkReader.ReadUInt32();
                int replaySizeInBytes = await chunkReader.ReadInt32();
                correctSize = chunkReader.AssertRemainingCountOfBytes( replaySizeInBytes );
            }
            if( chunkReader.IsError || !correctSize && !ErrorOnParseReplayDataChunk() )
            {
                return false;
            }
            return await ParseReplayData( await chunkReader.UncompressDataIfNeeded(), new ReplayDataInfo( time1, time2 ) ); ;
        }

        public virtual bool ErrorOnParseReplayDataChunk()
        {
            return ErrorOnChunkContentParsing();
        }
        /// <summary>
        /// Was writed to support how Fortnite store replays.
        /// This may need to be upgrade to support other games, or some future version of Fortnite.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns></returns>
        public virtual async Task<bool> ParseReplayData( CustomBinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            int currentLevelIndex = await binaryReader.ReadInt32();//TODO: use replayVersion.
            float timeSeconds = await binaryReader.ReadSingle();
            await ParseReceiveData( binaryReader, replayDataInfo );//TODO: use replayVersion.

            return true;
        }

        public virtual async Task<bool> ParseReceiveData( CustomBinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            return await ParseNetFieldExports( binaryReader, replayDataInfo )
                && await ParseNetExportGUIDs( binaryReader, replayDataInfo );
        }

        public virtual async Task<bool> ParseNetFieldExports( CustomBinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            uint exportCout = await binaryReader.ReadIntPacked();
            for( int i = 0; i < exportCout; i++ )
            {
                uint pathNameIndex = await binaryReader.ReadIntPacked();
                uint wasExported = await binaryReader.ReadIntPacked();
                if( wasExported > 0 )
                {
                    string pathName = await binaryReader.ReadString();
                    uint numExports = await binaryReader.ReadIntPacked();
                }
                else
                {
                    //We does nothing here but Unreal does something
                }
                var netExports = await binaryReader.ReadNetFieldExport();
            }
            if( binaryReader.IsError )
            {
                return await ErrorOnParseNetFieldExports();
            }
            return true;
        }

        public virtual async Task<bool> ParseNetExportGUIDs( CustomBinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            uint guidCount = await binaryReader.ReadIntPacked();
            for( int i = 0; i < guidCount; i++ )
            {
                byte[] guidData = await binaryReader.ReadBytes( await binaryReader.ReadInt32() );
                //TODO: use memory reader
            }
            return true;
        }
        public virtual Task<bool> ErrorOnParseNetFieldExports()
        {
            return Task.FromResult( ErrorOnParseReplayDataChunk() );
        }
    }
}
