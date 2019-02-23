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
    public class UnrealReplayVisitor : IDisposable
    {
        protected const uint FileMagic = 0x1CA2E27F;

        protected readonly SubStreamFactory SubStreamFactory;
        public UnrealReplayVisitor( Stream stream )
        {
            SubStreamFactory = new SubStreamFactory( stream );
        }

        #region ReplayHeaderParsing
        public virtual async Task<bool> Visit()
        {
            bool noErrorOrRecovered = true;
            using( CustomBinaryReaderAsync binaryReader = new CustomBinaryReaderAsync( SubStreamFactory.BaseStream, true, async () =>
             {
                 noErrorOrRecovered = await ErrorOnReplayHeaderParsing();
             } ) )//TODO check if header have a constant size
            {
                if( !await ParseMagicNumber( binaryReader ) )
                {
                    return false;
                }
                uint fileVersion = await binaryReader.ReadUInt32();
                int lengthInMs = await binaryReader.ReadInt32();
                uint networkVersion = await binaryReader.ReadUInt32();
                uint changelist = await binaryReader.ReadUInt32();
                string friendlyName = await binaryReader.ReadString();
                bool isLive = await binaryReader.ReadUInt32() != 0;
                DateTime timestamp = DateTime.MinValue;
                if( fileVersion >= (uint)VersionHistory.HISTORY_RECORDED_TIMESTAMP )
                {
                    timestamp = DateTime.FromBinary( await binaryReader.ReadInt64() );
                }
                bool compressed = false;
                if( fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION )
                {
                    compressed = await binaryReader.ReadUInt32() != 0;
                }
                var replayInfo = new ReplayInfo( lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                isLive, compressed, fileVersion );
                return noErrorOrRecovered && await VisitReplayInfo( replayInfo ) && await VisitReplayChunks( replayInfo );
            }
        }
        /// <summary>
        /// Error occured while parsing the header.
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> ErrorOnReplayHeaderParsing()
        {
            return Task.FromResult( false );
        }
        /// <summary>
        /// Does nothing, overload this if you want to grab the <see cref="ReplayInfo"/>
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <returns></returns>
        public virtual Task<bool> VisitReplayInfo( ReplayInfo replayInfo )
        {
            return Task.FromResult( true );
        }

        #region MagicNumber
        /// <summary>
        /// I don't know maybe you want to change that ? Why i did this ? I don't know me too.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <returns></returns>
        public virtual async Task<bool> ParseMagicNumber( CustomBinaryReaderAsync binaryReader )
        {
            return await VisitMagicNumber( await binaryReader.ReadUInt32() );
        }
        /// <summary>
        /// Check that the magic number is equal to <see cref="FileMagic"/>
        /// </summary>
        /// <param name="magicNumber"></param>
        /// <returns><see langword="true"/> if the magic number is correct.</returns>
        public virtual Task<bool> VisitMagicNumber( uint magicNumber )
        {
            return Task.FromResult( magicNumber == FileMagic );
        }
        #endregion MagicNumber
        #endregion ReplayHeaderParsing

        #region ReplayContentParsing
        public virtual async Task<bool> VisitReplayChunks( ReplayInfo replayInfo )
        {
            while( await ParseChunkHeader( replayInfo ) )
            {
            }

            return true;
        }

        #region ChunkParsing

        #region ChunkHeaderParsing
        /// <summary>
        /// Parse the header of a chunk, with that we know the <see cref="ChunkType"/> and the length of the chunk
        /// Took extra caution because there is no <see cref="SubStream"/> to protect the reading.
        /// When I discover the length of the replay, I immediatly create a SubStream so i can protect the rest of the replay.
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <returns></returns>
        public virtual async Task<bool> ParseChunkHeader( ReplayInfo replayInfo )
        {
            bool headerFatal = false;
            bool headerError = false;
            bool endOfStream = false;
            int chunkSize;
            ChunkType chunkType;
            using( Stream chunkHeader = await SubStreamFactory.Create( 8, true ) )
            using( CustomBinaryReaderAsync binaryReader = new CustomBinaryReaderAsync( chunkHeader ) )
            {
                chunkType = (ChunkType)await binaryReader.ReadUInt32();
                endOfStream = binaryReader.EndOfStream;
                chunkSize = await binaryReader.ReadInt32();
                headerFatal = binaryReader.Fatal;
                headerError = binaryReader.IsError;
                binaryReader.SetErrorReported();
            }
            if( endOfStream )
            {
                return await VisitEndOfStream();
            }
            if( headerFatal )
            {
                return await VisitIncompleteChunkHeader( chunkType );
            }
            if( headerError || (uint)chunkType > 3 )//All chunk types are between 0 and 3 included. If it's not in between, there is an issue.
            {
                return await VisitCorruptedChunk( SubStreamFactory.BaseStream, chunkType );
            }
            bool result;
            bool isError;
            using( SubStream subStream = await SubStreamFactory.Create( chunkSize, true ) )
            using( CustomBinaryReaderAsync binaryReader = new CustomBinaryReaderAsync( subStream ) )
            {
                result = await ChooseChunkType( binaryReader, replayInfo, chunkType, chunkSize );
                isError = binaryReader.IsError;
                binaryReader.SetErrorReported();
            }
            if(isError && await ErrorOnChunkContentParsingAsync() )
            {
                return false;
            }
            return result;
        }
        /// <summary>
        /// Only does routing to the right method
        /// No operation should be done here.
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual async Task<bool> ChooseChunkType( CustomBinaryReaderAsync binaryReader, ReplayInfo replayInfo, ChunkType chunkType, int chunkSize )
        {
            return (chunkType switch
            {
                ChunkType.Header => await ParseGameSpecificHeaderChunk( replayInfo, binaryReader ),
                ChunkType.Checkpoint => await ParseEventOrCheckpointHeader( replayInfo, binaryReader, true ),
                ChunkType.Event => await ParseEventOrCheckpointHeader( replayInfo, binaryReader, false ),
                ChunkType.ReplayData => await ParseReplayDataChunkHeader( replayInfo, binaryReader ),
                _ => throw new InvalidOperationException( "Invalid ChunkType" )
            }) ? true : await ErrorOnChunkContentParsingAsync();
        }
        /// <summary>
        /// Error inside chunk content parsing
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> ErrorOnChunkContentParsingAsync()
        {
            return Task.FromResult( true );
        }
        /// <summary>
        /// Error inside chunk content parsing
        /// </summary>
        /// <returns></returns>
        public virtual bool ErrorOnChunkContentParsing()
        {
            return true;
        }

        #region ChunkHeaderErrorHandling
        /// <summary>
        /// Does nothing, but you can try to recover from a corrupted/unsupported file here.
        /// Usually we can't read further, because we don't know the length of the actual block.
        /// You can try to detect the next block so the Visitor can finish his job properly
        /// </summary>
        /// <param name="subStreamFactory"></param>
        /// <param name="chunkType"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns>if the Visitor can safely continue is job, return always false if not overriden</returns>
        public virtual Task<bool> VisitCorruptedChunk( Stream replayStream, ChunkType chunk )
        {
            return Task.FromResult( false );
        }

        /// <summary>
        /// return true to continue
        /// false to stop the parsing.
        /// If you don't touch the <see cref="SubStreamFactory"/> it can't continue it's job.
        /// </summary>
        /// <returns><see langword="false"/>if not overidden</returns>
        public virtual Task<bool> VisitEndOfStream()
        {
            return Task.FromResult( false );
        }

        /// <summary>
        /// The replay finished abruptly. Worst than that, it finished at the beginning of a chunk.
        /// </summary>
        /// <param name="chunkType">May have a bad value</param>
        /// <param name="sizeInBytes">May have a bad value</param>
        /// <returns>Always false.</returns>
        public virtual Task<bool> VisitIncompleteChunkHeader( ChunkType chunk )
        {
            return Task.FromResult( false );
        }
        #endregion ChunkHeaderErrorHandling
        #endregion ChunkHeaderParsing



        #region ChunkContentParsing
        #region Header
        /// <summary>
        /// Simply return true and does nothing else. It depends on the implementation of the game.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual Task<bool> ParseGameSpecificHeaderChunk( ReplayInfo replayInfo, CustomBinaryReaderAsync binaryReader )
        {
            return Task.FromResult( true );
        }
        #endregion Header
        #region EventOrCheckPoint
        public virtual async Task<bool> ParseEventOrCheckpointHeader( ReplayInfo replayInfo, CustomBinaryReaderAsync binaryReader, bool isCheckpoint )
        {
            string id = await binaryReader.ReadString();
            string group = await binaryReader.ReadString();
            string metadata = await binaryReader.ReadString();
            uint time1 = await binaryReader.ReadUInt32();
            uint time2 = await binaryReader.ReadUInt32();
            int eventSizeInBytes = await binaryReader.ReadInt32();
            if( binaryReader.IsError || (!binaryReader.AssertRemainingCountOfBytes( eventSizeInBytes ) && !await ErrorOnParseEventOrCheckpointHeader()) )
            {
                return false;
            }

            if(isCheckpoint)
            {
                return await ParseCheckpointContent( await UncompressDataIfNeeded( binaryReader, replayInfo ), id, group, metadata, time1, time2 );
            }
            bool success = await ChooseEventChunkType( replayInfo, binaryReader, new EventOrCheckpointInfo( id, group, metadata, time1, time2, isCheckpoint ));
            if(!success || binaryReader.IsError)
            {
                return await ErrorOnParseEventOrCheckpointHeader();
            }
            return true;
        }

        public virtual Task<bool> ParseCheckpointContent(CustomBinaryReaderAsync binaryReader, string id, string group, string metadata, uint time1, uint time2)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ErrorOnParseEventOrCheckpointHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns>always <see langword="true"/></returns>
        public virtual Task<bool> ChooseEventChunkType( ReplayInfo replayInfo, CustomBinaryReaderAsync binaryReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        #endregion EventOrCheckPoint


        #region ReplayData
        public virtual async Task<bool> ParseReplayDataChunkHeader( ReplayInfo replayInfo, CustomBinaryReaderAsync binaryReader )
        {
            bool correctSize = true;
            uint time1 = uint.MaxValue;
            uint time2 = uint.MaxValue;
            if( replayInfo.FileVersion >= (uint)VersionHistory.HISTORY_STREAM_CHUNK_TIMES )
            {
                time1 = await binaryReader.ReadUInt32();
                time2 = await binaryReader.ReadUInt32();
                int replaySizeInBytes = await binaryReader.ReadInt32();
                correctSize = binaryReader.AssertRemainingCountOfBytes( replaySizeInBytes );
            }
            if( binaryReader.IsError || !correctSize && !ErrorOnParseReplayDataChunk() )
            {
                return false;
            }
            return await ParseReplayData( await UncompressDataIfNeeded( binaryReader, replayInfo ), new ReplayDataInfo( time1, time2 ) ); ;
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
                if(wasExported>0)
                {
                    string pathName = await binaryReader.ReadString();
                    uint numExports = await binaryReader.ReadIntPacked();
                } else
                {
                    //We does nothing here but Unreal does something
                }
                var netExports = await binaryReader.ReadNetFieldExport();
            }
            if(binaryReader.IsError)
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
        #endregion ReplayData

        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// Will simply read the chunk of data and return it if it's not needed.
        /// I didn't test against not compressed replay, so it may fail.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns>Readable data uncompresed if it was needed</returns>
        public virtual async Task<CustomBinaryReaderAsync> UncompressDataIfNeeded( CustomBinaryReaderAsync binaryReader, ReplayInfo replayInfo )
        {
            byte[] output;
            if( replayInfo.Compressed )
            {
                int decompressedSize = await binaryReader.ReadInt32();
                int compressedSize = await binaryReader.ReadInt32();
                byte[] compressedBuffer = await binaryReader.ReadBytes( compressedSize );
                output = OodleBinding.Decompress( compressedBuffer, compressedBuffer.Length, decompressedSize );
            }
            else
            {
                output = await binaryReader.DumpRemainingBytes();
            }
            return new CustomBinaryReaderAsync( new MemoryStream( output ) );
        }

        #endregion ChunkContentParsing
        #endregion ChunkParsing

        #endregion ReplayContentParsing
        public void Dispose()
        {
            SubStreamFactory.Dispose();
        }
    }
}
