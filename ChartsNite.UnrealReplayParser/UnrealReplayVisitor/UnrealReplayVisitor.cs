using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChartsNite.UnrealReplayParser;
using ChartsNite.UnrealReplayParser.StreamArchive;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;
using UnrealReplayParser.UnrealObject;

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

        protected readonly SubStreamFactory SubStreamFactory;
        public UnrealReplayVisitor( Stream stream )
        {
            SubStreamFactory = new SubStreamFactory( stream );
        }

        #region ReplayContentParsing
        public virtual async ValueTask<bool> VisitChunks()
        {
            while( true )
            {
                ChunkHeader chunkHeader = await ParseChunkHeader();
                await using( SubStream stream = SubStreamFactory.CreateSubstream( chunkHeader.ChunkSize ) )
                using( ReplayArchiveAsync binaryReader = new ReplayArchiveAsync( stream, DemoHeader!.EngineNetworkProtocolVersion, ReplayHeader!.Compressed , true ) )
                {
                    if( chunkHeader.ChunkType == ChunkType.EndOfStream )
                    {
                        if( await VisitEndOfStream() )
                        {
                            continue;
                        }
                        return true;
                    }
                    if( !await ChooseChunkType( binaryReader, chunkHeader.ChunkType ) )
                    {
                        return false;
                    }
                }
            }
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
        public virtual async ValueTask<ChunkHeader> ParseChunkHeader()
        {
            if( SubStreamFactory.CanReadLength && SubStreamFactory.CanReadPosition
                && SubStreamFactory.BaseStream.Position == SubStreamFactory.BaseStream.Length )
            {
                return new ChunkHeader { ChunkType = ChunkType.EndOfStream, ChunkSize = 0 };
            }

            int chunkSize;
            ChunkType chunkType;
            await using( SubStream chunkHeader = SubStreamFactory.CreateSubstream( 8 ) )
            await using( ReplayArchiveAsync customReader = new ReplayArchiveAsync( chunkHeader, 0, false, true ) )
            {
                try //TODO add case when you can seek.
                {
                    chunkType = (ChunkType)await customReader.ReadUInt32Async();
                }
                catch( EndOfStreamException )
                {
                    chunkHeader.CancelSelfRepositioning();
                    return new ChunkHeader { ChunkType = ChunkType.EndOfStream, ChunkSize = 0 };
                }
                chunkSize = await customReader.ReadInt32Async();
                if( (uint)chunkType > 3 )
                {
                    return new ChunkHeader { ChunkType = ChunkType.Unknown, ChunkSize = 0 };
                }
            }
            return new ChunkHeader { ChunkType = chunkType, ChunkSize = chunkSize };
        }

        /// <summary>
        /// Only does routing to the right method
        /// No operation should be done here.
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual async ValueTask<bool> ChooseChunkType( ReplayArchiveAsync binaryReader, ChunkType chunkType )
        {
            return (chunkType switch
            {
                ChunkType.Header => throw new InvalidOperationException( "Replay Header was already read." ),
                ChunkType.Checkpoint => await ParseCheckpointHeader( binaryReader ),
                ChunkType.Event => await ParseEventHeader( binaryReader ),
                ChunkType.ReplayData => await ParseReplayDataChunkHeader( binaryReader ),
                _ => throw new InvalidOperationException( "Invalid ChunkType" )
            }) ? true : await ErrorOnChunkContentParsingAsync();
        }
        /// <summary>
        /// Error inside chunk content parsing
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask<bool> ErrorOnChunkContentParsingAsync()
        {
            return new ValueTask<bool>( true );
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
        public virtual ValueTask<bool> VisitCorruptedChunk( Stream replayStream, ChunkType chunk )
        {
            return new ValueTask<bool>( false );
        }

        /// <summary>
        /// return true to continue
        /// false to stop the parsing.
        /// If you don't touch the <see cref="SubStreamFactory"/> it can't continue it's job.
        /// </summary>
        /// <returns><see langword="false"/>if not overidden</returns>
        public virtual ValueTask<bool> VisitEndOfStream()
        {
            return new ValueTask<bool>( false );
        }

        /// <summary>
        /// The replay finished abruptly. Worst than that, it finished at the beginning of a chunk.
        /// </summary>
        /// <param name="chunkType">May have a bad value</param>
        /// <param name="sizeInBytes">May have a bad value</param>
        /// <returns>Always false.</returns>
        public virtual ValueTask<bool> VisitIncompleteChunkHeader( ChunkType chunk )
        {
            return new ValueTask<bool>( false );
        }
        #endregion ChunkHeaderErrorHandling
        #endregion ChunkHeaderParsing



        #endregion ChunkParsing

        #endregion ReplayContentParsing
        public void Dispose()
        {
            SubStreamFactory.Dispose();
        }
    }
}
