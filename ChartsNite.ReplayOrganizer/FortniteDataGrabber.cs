using Common.StreamHelpers;
using FortniteReplayParser;
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
        public bool Error { get; private set; }
        public List<byte[]> ReplayDataDumps { get; private set; }
        public List<byte[]> CheckpointsDumps { get; private set; }
        public ReplayInfo? ReplayInfo { get; private set; }
        public FortniteDataGrabber( Stream stream ) : base( stream )
        {
            ReplayDataDumps = new List<byte[]>();
            CheckpointsDumps = new List<byte[]>();
        }
        public override async Task<ReplayInfo?> ParseReplayInfo()
        {
            ReplayInfo = await base.ParseReplayInfo();
            return ReplayInfo;
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

        public override Task<bool> ErrorOnChunkContentParsingAsync()
        {
            Error = true;
            return base.ErrorOnChunkContentParsingAsync();
        }
    }
}
