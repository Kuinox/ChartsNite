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
        public List<byte[]> CheckpointsDumps { get; private set; }
        public ReplayInfo? ReplayInfo { get; private set; }
        public FortniteDataGrabber( Stream stream ) : base( stream )
        {
            CheckpointsDumps = new List<byte[]>();
        }
        public override async ValueTask<ReplayInfo?> ParseReplayInfo()
        {
            ReplayInfo = await base.ParseReplayInfo();
            return ReplayInfo;
        }

        public override async ValueTask<bool> ParseCheckpointContent( ChunkReader chunkReader, string id, string group, string metadata, uint time1, uint time2 )
        {
            bool result = await base.ParseCheckpointContent( chunkReader, id, group, metadata, time1, time2 );
            //CheckpointsDumps.Add( await chunkReader.ReadBytesAsync( (int)(chunkReader.BaseStream.Length - chunkReader.BaseStream.Position) ) );
            return result;
        }

        public override ValueTask<bool> ErrorOnChunkContentParsingAsync()
        {
            Error = true;
            return base.ErrorOnChunkContentParsingAsync();
        }
    }
}
