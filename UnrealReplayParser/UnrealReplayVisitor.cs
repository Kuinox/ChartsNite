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
        const uint _fileMagic = 0x1CA2E27F;

        protected readonly SubStreamFactory SubStreamFactory;
        public UnrealReplayVisitor( Stream stream )
        {
            SubStreamFactory = new SubStreamFactory( stream );
        }

        #region ReplayHeaderParsing
        public virtual async Task<bool> Visit()
        {
            bool noErrorOrRecovered = true;
            using( BinaryReaderAsync binaryReader = new BinaryReaderAsync( SubStreamFactory.BaseStream, true, async () =>
             {
                 noErrorOrRecovered = await VisitReplayHeaderParsingError();
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
                bool bIsLive = await binaryReader.ReadUInt32() != 0;
                DateTime timestamp = DateTime.MinValue;
                if( fileVersion >= (uint)VersionHistory.HISTORY_RECORDED_TIMESTAMP )
                {
                    timestamp = DateTime.FromBinary( await binaryReader.ReadInt64() );
                }
                bool bCompressed = false;
                if( fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION )
                {
                    bCompressed = await binaryReader.ReadUInt32() != 0;
                }
                var replayInfo = new ReplayInfo( lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion );
                return noErrorOrRecovered && await VisitReplayInfo( replayInfo ) && await VisitReplayChunks( replayInfo );
            }
        }
        /// <summary>
        /// Error occured while parsing the header.
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> VisitReplayHeaderParsingError()
        {
            return Task.FromResult( false );
        }


        public virtual Task<bool> VisitBadReplayHeader()
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> VisitReplayInfo( ReplayInfo replayInfo )
        {
            return Task.FromResult( true );
        }

        #region MagicNumber
        public virtual async Task<bool> ParseMagicNumber( BinaryReaderAsync binaryReader )
        {
            return await VisitMagicNumber( await binaryReader.ReadUInt32() );
        }
        /// <summary>
        /// Check that the magic number is equal to <see cref="_fileMagic"/>
        /// </summary>
        /// <param name="magicNumber"></param>
        /// <returns><see langword="true"/> if the magic number is correct.</returns>
        public virtual Task<bool> VisitMagicNumber( uint magicNumber )
        {
            return Task.FromResult( magicNumber == _fileMagic );
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
        public virtual async Task<bool> ParseChunkHeader( ReplayInfo replayInfo )
        {
            bool headerFatal = false;
            bool headerError = false;
            bool endOfStream = false;
            int chunkSize;
            ChunkType chunkType;
            using( Stream chunkHeader = await SubStreamFactory.Create( 8, true ) )
            using( BinaryReaderAsync chunkHeaderBinaryReader = new BinaryReaderAsync( chunkHeader ) )
            {
                chunkType = (ChunkType)await chunkHeaderBinaryReader.ReadUInt32();
                chunkSize = await chunkHeaderBinaryReader.ReadInt32();
                endOfStream = chunkHeaderBinaryReader.EndOfStream;
                headerFatal = chunkHeaderBinaryReader.Fatal;
                headerError = chunkHeaderBinaryReader.IsError;
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
            return await ChooseChunkType( replayInfo, chunkType, chunkSize );
        }
        /// <summary>
        /// Only does routing to the right method
        /// No operation should be done here.
        /// </summary>
        /// <param name="replayInfo"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual async Task<bool> ChooseChunkType( ReplayInfo replayInfo, ChunkType chunkType, int chunkSize )
        {
            using( SubStream subStream = await SubStreamFactory.Create( chunkSize, true ) )
            using( BinaryReaderAsync binaryReader = new BinaryReaderAsync( subStream ) )
            {
                bool success = (chunkType switch
                {
                    ChunkType.Header => await ParseHeaderChunk( replayInfo, binaryReader ),
                    ChunkType.Checkpoint => await ParseEventChunkHeader( replayInfo, binaryReader, true ),
                    ChunkType.Event => await ParseEventChunkHeader( replayInfo, binaryReader, false ),
                    ChunkType.ReplayData => await ParseReplayDataChunkHeader( replayInfo, binaryReader ),
                    _ => throw new InvalidOperationException( "Invalid ChunkType" )
                });
                if( !success )
                {

                }
                return success ? true : await VisitChunkContentParsingError();
            }
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
        /// <summary>
        /// Simply return true and does nothing else.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual Task<bool> ParseHeaderChunk( ReplayInfo replayInfo, BinaryReaderAsync binaryReader )
        {
            return Task.FromResult( true );
        }
        public virtual async Task<bool> ParseEventChunkHeader( ReplayInfo replayInfo, BinaryReaderAsync binaryReader, bool isCheckpoint )
        {
            string id = await binaryReader.ReadString();
            string group = await binaryReader.ReadString();
            string metadata = await binaryReader.ReadString();
            uint time1 = await binaryReader.ReadUInt32();
            uint time2 = await binaryReader.ReadUInt32();
            int eventSizeInBytes = await binaryReader.ReadInt32();
            if( binaryReader.IsError || (!binaryReader.AssertRemainingCountOfBytes( eventSizeInBytes ) && !await VisitChunkContentParsingError()) )
            {
                return false;
            }
            return await ChooseEventChunkType( replayInfo, binaryReader, new EventInfo( id, group, metadata, time1, time2, isCheckpoint ) );
        }

        /// <summary>
        /// Error inside chunk content parsing
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> VisitChunkContentParsingError()
        {
            return Task.FromResult( true );
        }


        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns>always <see langword="true"/></returns>
        public virtual Task<bool> ChooseEventChunkType( ReplayInfo replayInfo, BinaryReaderAsync binaryReader, EventInfo eventInfo )
        {
            return Task.FromResult( true );
        }

        public virtual async Task<bool> ParseReplayDataChunkHeader( ReplayInfo replayInfo, BinaryReaderAsync binaryReader )
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
            if( binaryReader.IsError || !correctSize && !await VisitChunkContentParsingError() )
            {
                return false;
            }
            return await ParseReplayData( await UncompressData( binaryReader, replayInfo ), new ReplayDataInfo( time1, time2 ) ); ;
        }

        public virtual Task<bool> ParseReplayData(BinaryReader binaryReader, ReplayDataInfo replayDataInfo)
        {
            return Task.FromResult(true);
        }
        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns></returns>
        public virtual async Task<BinaryReader> UncompressData( BinaryReaderAsync binaryReader, ReplayInfo replayInfo )
        {
            byte[] output;
            if( replayInfo.BCompressed )
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
            return new BinaryReader( new MemoryStream( output ) );
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
