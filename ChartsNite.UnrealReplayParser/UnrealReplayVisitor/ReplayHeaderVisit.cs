using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChartsNite.UnrealReplayParser;
using ChartsNite.UnrealReplayParser.StreamArchive;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;
using UnrealReplayParser.UnrealObject;
using static UnrealReplayParser.DemoHeader;
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
        protected const uint FileMagic = 0x1CA2E27F;
        protected const uint DemoHeaderMagic = 0x2CF5A13D;
        protected ReplayHeader? ReplayHeader { get; set; }
        protected DemoHeader? DemoHeader { get; set; }
        public virtual async ValueTask<bool> Visit()
        {
            return await ParseReplayInfo() ? await VisitChunks() : false;
        }

        public virtual async ValueTask<bool> ParseReplayInfo()
        {
            using ReplayArchiveAsync ar = new ReplayArchiveAsync( SubStreamFactory.BaseStream, 0, compressed: false, leaveOpen: true );
            if( !await ParseReplayHeader( ar ) ) return false;
            ChunkHeader chunkHeader = await ParseChunkHeader();
            if( chunkHeader.ChunkType != ChunkType.Header ) return false;
            await using SubStream stream = SubStreamFactory.CreateSubstream( chunkHeader.ChunkSize );
            using ReplayArchiveAsync chunkReader = new ReplayArchiveAsync( stream, 0/*We are parsing the chunk and don't know the version yet.*/, false, true );

            return await ParseGameSpecificHeaderChunk( chunkReader );
        }

        public virtual async ValueTask<bool> ParseReplayHeader( ReplayArchiveAsync binaryReader )
        {
            if( !await ParseMagicNumber( binaryReader ) )
            {
                return false;
            }
            ReplayHeader.ReplayVersionHistory fileVersion = (ReplayHeader.ReplayVersionHistory)await binaryReader.ReadUInt32Async();
            int lengthInMs = await binaryReader.ReadInt32Async();
            uint networkVersion = await binaryReader.ReadUInt32Async();
            uint changelist = await binaryReader.ReadUInt32Async();
            string friendlyName = await binaryReader.ReadStringAsync();
            bool isLive = await binaryReader.ReadUInt32Async() != 0;
            DateTime timestamp = DateTime.MinValue;
            if( fileVersion >= ReplayHeader.ReplayVersionHistory.recordedTimestamp )
            {
                timestamp = DateTime.FromBinary( await binaryReader.ReadInt64Async() );
            }
            bool compressed = false;
            if( fileVersion >= ReplayHeader.ReplayVersionHistory.compression )
            {
                compressed = await binaryReader.ReadUInt32Async() != 0;
            }
            ReplayHeader = new ReplayHeader( lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
            isLive, compressed, fileVersion );
            return true;
        }
        /// <summary>
        /// Error occured while parsing the header.
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask<bool> ErrorOnReplayHeaderParsing()
        {
            return new ValueTask<bool>( false );
        }
        /// <summary>
        /// I don't know maybe you want to change that ? Why i did this ? I don't know me too.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <returns></returns>
        public virtual async ValueTask<bool> ParseMagicNumber( ReplayArchiveAsync binaryReader )
        {
            return await VisitMagicNumber( await binaryReader.ReadUInt32Async() );
        }
        /// <summary>
        /// Check that the magic number is equal to <see cref="FileMagic"/>
        /// </summary>
        /// <param name="magicNumber"></param>
        /// <returns><see langword="true"/> if the magic number is correct.</returns>
        public virtual ValueTask<bool> VisitMagicNumber( uint magicNumber )
        {
            return new ValueTask<bool>( magicNumber == FileMagic );
        }



        /// <summary>
        /// Simply return true and does nothing else. It depends on the implementation of the game.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual async ValueTask<bool> ParseGameSpecificHeaderChunk( ReplayArchiveAsync binaryReader )
        {
            if( await binaryReader.ReadUInt32Async() != DemoHeaderMagic ) return false;
            NetworkVersionHistory version = (NetworkVersionHistory)await binaryReader.ReadUInt32Async();
            if( version < NetworkVersionHistory.saveFullEngineVersion ) return false;
            uint networkChecksum = await binaryReader.ReadUInt32Async();
            EngineNetworkVersionHistory engineNetworkProtocolVersion = (EngineNetworkVersionHistory)await binaryReader.ReadUInt32Async();
            uint gameNetworkProtocolVersion = await binaryReader.ReadUInt32Async();
            Guid guid = Guid.Empty;
            if( version >= NetworkVersionHistory.guidDemoHeader )
            {
                guid = new Guid((await binaryReader.ReadBytesAsync( 16 )).Span);
            }
            ushort major = await binaryReader.ReadUInt16Async();
            ushort minor = await binaryReader.ReadUInt16Async();
            ushort patch = await binaryReader.ReadUInt16Async();
            uint changeList = await binaryReader.ReadUInt32Async();
            string branch = await binaryReader.ReadStringAsync();
            (string, uint)[] levelNamesAndTimes = await binaryReader.ReadArrayAsync( async () => (await binaryReader.ReadStringAsync(), await binaryReader.ReadUInt32Async()) );
            //Headerflags
            ReplayHeaderFlags replayHeaderFlags = (ReplayHeaderFlags)await binaryReader.ReadUInt32Async();
            string[] gameSpecificData = await binaryReader.ReadArrayAsync( binaryReader.ReadStringAsync );
            DemoHeader = new DemoHeader( version, networkChecksum, engineNetworkProtocolVersion, gameNetworkProtocolVersion, guid, major, minor, patch, changeList, branch, levelNamesAndTimes, replayHeaderFlags, gameSpecificData );
            return true;
        }
    }
}
