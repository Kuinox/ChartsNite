using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public virtual async ValueTask<bool> ParseCheckpointHeader( CustomBinaryReaderAsync binaryReader )
        {
            string id = await binaryReader.ReadStringAsync();
            string group = await binaryReader.ReadStringAsync();
            string metadata = await binaryReader.ReadStringAsync();
            uint time1 = await binaryReader.ReadUInt32Async();
            uint time2 = await binaryReader.ReadUInt32Async();
            int eventSizeInBytes = await binaryReader.ReadInt32Async();
            return ParseCheckpointContent( await binaryReader.UncompressData(), id, group, metadata, time1, time2 );
        }

        public virtual bool ParseCheckpointContent( CustomBinaryReader binaryReader, string id, string group, string metadata, uint time1, uint time2 )
        {
            long packetOffset = binaryReader.ReadInt64();
            int levelForCheckpoint = binaryReader.ReadInt32();

            string[] deletedNetStartupActors = binaryReader.ReadSparseArray( binaryReader.ReadString );
            int valuesCount = binaryReader.ReadInt32();
            for( int i = 0; i < valuesCount; i++ )
            {
                uint guid = binaryReader.ReadIntPacked();
                uint outerGuid = binaryReader.ReadIntPacked();
                string path = binaryReader.ReadString();
                uint checksum = binaryReader.ReadUInt32();
                byte flags = binaryReader.ReadOneByte();
            }
            NetFieldExportGroupMap( binaryReader );
            ParsePlaybackPacket( binaryReader );
           // File.WriteAllBytes( "dump.dump", binaryReader.DumpRemainingBytes() );
            return true;
        }

        public virtual bool NetFieldExportGroupMap( CustomBinaryReader binaryReader )
        {
            uint numNetFieldExportGroups = binaryReader.ReadUInt32();
            for( int i = 0; i < numNetFieldExportGroups; i++ )
            {
                ParseNetFieldExportGroup( binaryReader );
            }
            return true;
        }

        public virtual bool ParseNetFieldExportGroup( CustomBinaryReader binaryReader )
        {
            string a = binaryReader.ReadString();
            uint packedInt = binaryReader.ReadIntPacked();
            uint count = binaryReader.ReadIntPacked();
            for( int i = 0; i < count; i++ )
            {
                binaryReader.ReadNetFieldExport( DemoHeader!.EngineNetworkProtocolVersion );
            }
            return true;
        }

        public virtual ValueTask<bool> ErrorOnParseEventOrCheckpointHeader()
        {
            return ErrorOnChunkContentParsingAsync();
        }
    }
}
