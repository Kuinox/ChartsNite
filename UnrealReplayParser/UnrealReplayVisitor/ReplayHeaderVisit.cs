using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public virtual async ValueTask<bool> Visit()
        {
            ReplayInfo? replayInfo = await ParseReplayInfo();
            if( replayInfo == null )
            {
                return false;
            }
            return await VisitReplayChunks( replayInfo );
        }

        public virtual async ValueTask<ReplayInfo?> ParseReplayInfo()
        {
            await using( CustomBinaryReaderAsync binaryReader = new CustomBinaryReaderAsync( SubStreamFactory.BaseStream, true ) )
            {
                ReplayHeader? replayHeader = await ParseReplayHeader( binaryReader );
                if( replayHeader == null )
                {
                    return null;
                }

                await using( ChunkReader chunkReader = await ParseChunkHeader( new ReplayInfo( replayHeader, new DemoHeader( NetworkVersionHistory.initial, 0, 0, 0, new byte[0], 0, 0, 0, 0, "", new (string, uint)[0], ReplayHeaderFlags.None, new string[0] ) ) ) )
                {
                    if( chunkReader == null )
                    {
                        return null;
                    }
                    DemoHeader? demoHeader;
                    await using( chunkReader )
                    {
                        demoHeader = await ParseGameSpecificHeaderChunk( chunkReader );
                    }
                    if( demoHeader == null )
                    {
                        return null;
                    }
                    return new ReplayInfo( replayHeader, demoHeader );
                }
            }
        }

        public virtual async ValueTask<ReplayHeader?> ParseReplayHeader( CustomBinaryReaderAsync binaryReader )
        {
            if( !await ParseMagicNumber( binaryReader ) )
            {
                return null;
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
            return new ReplayHeader( lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
            isLive, compressed, fileVersion );
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
        public virtual async ValueTask<bool> ParseMagicNumber( CustomBinaryReaderAsync binaryReader )
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
        public virtual async ValueTask<DemoHeader?> ParseGameSpecificHeaderChunk( ChunkReader chunkReader )
        {
            if( await chunkReader.ReadUInt32Async() != DemoHeaderMagic )
            {
                return null;
            }
            NetworkVersionHistory version = (NetworkVersionHistory)await chunkReader.ReadUInt32Async();
            if( version < NetworkVersionHistory.saveFullEngineVersion )
            {
                return null;
            }
            uint networkChecksum = await chunkReader.ReadUInt32Async();
            uint engineNetworkProtocolVersion = await chunkReader.ReadUInt32Async();
            uint gameNetworkProtocolVerrsion = await chunkReader.ReadUInt32Async();
            byte[] guid = new byte[0];
            if( version >= NetworkVersionHistory.guidDemoHeader )
            {
                guid = await chunkReader.ReadBytesAsync( 16 );
            }
            ushort major = await chunkReader.ReadUInt16();
            ushort minor = await chunkReader.ReadUInt16();
            ushort patch = await chunkReader.ReadUInt16();
            uint changeList = await chunkReader.ReadUInt32Async();
            string branch = await chunkReader.ReadStringAsync();
            (string, uint)[] levelNamesAndTimes = await new ArrayParser<(string, uint), TupleParser<StringParser, UInt32Parser, string, uint>>( chunkReader, new TupleParser<StringParser, UInt32Parser, string, uint>( new StringParser( chunkReader ), new UInt32Parser( chunkReader ) ) ).Parse();
            //Headerflags
            ReplayHeaderFlags replayHeaderFlags = (ReplayHeaderFlags)await chunkReader.ReadUInt32Async();
            string[] gameSpecificData = await new ArrayParser<string, StringParser>( chunkReader, new StringParser( chunkReader ) ).Parse();
            return new DemoHeader(version, networkChecksum, engineNetworkProtocolVersion, gameNetworkProtocolVerrsion, guid, major, minor, patch, changeList, branch, levelNamesAndTimes, replayHeaderFlags, gameSpecificData);
        }
    }
}
