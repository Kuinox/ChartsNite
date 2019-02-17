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
        public ReplayInfo? ReplayInfo { get; private set; }
        public FortniteHeaderChunk? FortniteHeaderChunk { get; private set; }
        public FortniteDataGrabber( Stream stream ) : base( stream )
        {
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

        public override async Task<bool> ParseReplayDataChunkContent( BinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo )
        {
            //await binaryReader.ReadBytes(replayDataInfo.ReplayDataSizeInBytes)//TODO: redo size
            return await base.ParseReplayDataChunkContent( binaryReader, replayDataInfo );
        }
    }
}
