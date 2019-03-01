using System;
using System.Collections.Generic;
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

        bool _endOfStream;
        protected readonly SubStreamFactory SubStreamFactory;
        public UnrealReplayVisitor( Stream stream )
        {
            SubStreamFactory = new SubStreamFactory( stream );
        }

        #region ReplayContentParsing
        public virtual async Task<bool> VisitReplayChunks( ReplayInfo replayInfo )
        {
            bool isEndOfStream = false;
            while( !isEndOfStream )
            {
                await foreach( (ChunkType chunkType, ChunkReader chunkReader) in ParseChunkHeader( replayInfo ) )
                {
                    if( chunkType == ChunkType.EndOfStream )
                    {
                        isEndOfStream = true;
                    }
                    if( !await ChooseChunkType( chunkReader, chunkType ))
                    {
                        return false;
                    }
                }
                isEndOfStream = !await VisitEndOfStream();
            }
            return isEndOfStream;
        }

        #region ChunkParsing

        #region ChunkHeaderParsing
        /// <summary>
        /// Parse the header of a chunk, with that we know the <see cref="ChunkType"/> and the length of the chunk
        /// Took extra caution because there is no <see cref="SubStream"/> to protect the reading.
        /// When I discover the length of the replay, I immediatly create a SubStream so i can protect the rest of the replay.
        /// </summary>
        /// <param name="replayHeader"></param>
        /// <returns></returns>
        public virtual async IAsyncEnumerable<(ChunkType chunkType, ChunkReader? chunkReader)> ParseChunkHeader( ReplayHeader replayHeader )
        {
            int chunkSize;
            ChunkType chunkType;
            await using( SubStream chunkHeader = SubStreamFactory.Create( 8, true ) )
            await using( CustomBinaryReaderAsync binaryReader = new CustomBinaryReaderAsync( chunkHeader, true ) )
            {
                chunkType = (ChunkType)await binaryReader.ReadUInt32();
                if( binaryReader.EndOfStream )
                {
                    if( _endOfStream )
                    {
                        binaryReader.SetErrorReported();
                        yield break;
                    }
                    else
                    {
                        _endOfStream = true;
                        binaryReader.SetErrorReported();
                        yield return (ChunkType.EndOfStream, null);
                    }
                }
                chunkSize = await binaryReader.ReadInt32();
                if( binaryReader.IsError || (uint)chunkType > 3 )
                {
                    binaryReader.SetErrorReported();
                    yield break;
                }
            }
            yield return (chunkType, new ChunkReader( SubStreamFactory.Create( chunkSize ), replayHeader ));
        }

        public virtual IAsyncEnumerable<(ChunkType chunkType, ChunkReader? chunkReader)> ParseChunkHeader( ReplayInfo replayInfo ) =>
            ParseChunkHeader( replayInfo.ReplayHeader );
        /// <summary>
        /// Only does routing to the right method
        /// No operation should be done here.
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual async Task<bool> ChooseChunkType( ChunkReader chunkReader, ChunkType chunkType )
        {
            return (chunkType switch
            {
                ChunkType.Header => throw new InvalidOperationException( "Replay Header was already read." ),
                ChunkType.Checkpoint => await ParseCheckpointHeader( chunkReader ),
                ChunkType.Event => await ParseEventHeader( chunkReader ),
                ChunkType.ReplayData => await ParseReplayDataChunkHeader( chunkReader ),
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
        #endregion ChunkContentParsing
        #endregion ChunkParsing

        #endregion ReplayContentParsing
        public void Dispose()
        {
            SubStreamFactory.Dispose();
        }
    }
}
