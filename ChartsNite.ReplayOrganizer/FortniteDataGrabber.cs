using Common.StreamHelpers;
using FortniteReplayParser;
using FortniteReplayParser.Chunk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;
using UnrealReplayParser.Chunk;

namespace ChartsNite.ReplayOrganizer
{
    class FortniteDataGrabber : FortniteReplayVisitor
    {
        public List<byte[]> ReplayDataDumps { get; private set; }
        public List<byte[]> CheckpointsDumps { get; private set; }
        public ReplayInfo? ReplayInfo { get; private set; }
        public FortniteHeaderChunk? FortniteHeaderChunk { get; private set; }
        public FortniteDataGrabber( Stream stream ) : base( stream )
        {
            ReplayDataDumps = new List<byte[]>();
            CheckpointsDumps = new List<byte[]>();
        }
        public override Task<bool> VisitReplayInfo( ReplayInfo replayInfo )
        {
            ReplayInfo = replayInfo;
            return base.VisitReplayInfo( replayInfo );
        }
        public override Task<bool> VisitFortniteHeaderChunk( FortniteHeaderChunk headerChunk )
        {
            FortniteHeaderChunk = headerChunk;
            return base.VisitFortniteHeaderChunk( headerChunk );
        }

        public override async Task<bool> ParseCheckpointContent( ChunkReader chunkReader, string id, string group, string metadata, uint time1, uint time2 )
        {
            bool result = await base.ParseCheckpointContent( chunkReader, id, group, metadata, time1, time2 );
            CheckpointsDumps.Add( await chunkReader.ReadBytes( (int)(chunkReader.BaseStream.Length - chunkReader.BaseStream.Position) ) );
            return result;
        }
        public override async Task<bool> ParseReplayData( CustomBinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            bool result = await base.ParseReplayData( binaryReader, replayDataInfo );
            ReplayDataDumps.Add( await binaryReader.ReadBytes( (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) ) );
            return result;
        }
    }
}
