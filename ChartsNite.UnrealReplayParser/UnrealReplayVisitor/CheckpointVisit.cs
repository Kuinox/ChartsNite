using System;
using System.Buffers;
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
        public virtual async ValueTask<bool> ParseCheckpointHeader( ReplayArchiveAsync binaryReader )
        {
            string id = await binaryReader.ReadStringAsync();
            string group = await binaryReader.ReadStringAsync();
            string metadata = await binaryReader.ReadStringAsync();
            uint time1 = await binaryReader.ReadUInt32Async();
            uint time2 = await binaryReader.ReadUInt32Async();
            int eventSizeInBytes = await binaryReader.ReadInt32Async();
            using( IMemoryOwner<byte> uncompressed = await binaryReader.UncompressData() )
            {

                //return ParseCheckpointContent( new ChunkArchive( uncompressed.Memory, DemoHeader!.EngineNetworkProtocolVersion ), id, group, metadata, time1, time2 );
                return true;
            }
            
        }

        public virtual bool ParseCheckpointContent( ChunkArchive ar, string id, string group, string metadata, uint time1, uint time2 )
        {
            long packetOffset = ar.ReadInt64();
            int levelForCheckpoint = ar.ReadInt32();

            string[] deletedNetStartupActors = ar.ReadArray( ar.ReadString );
            int valuesCount = ar.ReadInt32();
            for( int i = 0; i < valuesCount; i++ )
            {
                uint guid = ar.ReadIntPacked();
                uint outerGuid = ar.ReadIntPacked();
                string path = ar.ReadString();
                uint checksum = ar.ReadUInt32();
                byte flags = ar.ReadByte();
            }
            NetFieldExportGroupMap( ar );
            ParsePlaybackPacket( ar );
           // File.WriteAllBytes( "dump.dump", binaryReader.DumpRemainingBytes() );
            return true;
        }

        public virtual bool NetFieldExportGroupMap( ChunkArchive binaryReader )
        {
            uint numNetFieldExportGroups = binaryReader.ReadUInt32();
            for( int i = 0; i < numNetFieldExportGroups; i++ )
            {
                ParseNetFieldExportGroup( binaryReader );
            }
            return true;
        }

        public virtual bool ParseNetFieldExportGroup( ChunkArchive ar )
        {
            string a = ar.ReadString();
            uint packedInt = ar.ReadIntPacked();
            uint count = ar.ReadIntPacked();
            for( int i = 0; i < count; i++ )
            {
                ar.ReadNetFieldExport();
            }
            return true;
        }

        public virtual ValueTask<bool> ErrorOnParseEventOrCheckpointHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
    }
}
