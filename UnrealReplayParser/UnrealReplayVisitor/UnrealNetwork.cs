using System;
using System.Collections.Generic;
using System.Text;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public partial class UnrealReplayVisitor : IDisposable
    {

        /// <summary>
        /// Was writed to support how Fortnite store replays.
        /// This may need to be upgrade to support other games, or some future version of Fortnite.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns></returns>
        public virtual bool ParsePlaybackPacket( ChunkReader chunkReader )
        {
            bool appendPacket = true;
            bool hasLevelStreamingFixes = true;//TODO: this method
            int currentLevelIndex = chunkReader.ReadInt32();//TODO: use replayVersion. HasLevelStreamingFixes
            float timeSeconds = chunkReader.ReadSingle();
            if(float.IsNaN(timeSeconds))
            {

            }
            ParseExportData( chunkReader );//TODO: use replayVersion. HasLevelStreamingFixes
            if( (chunkReader.ReplayInfo.DemoHeader.HeaderFlags & DemoHeader.ReplayHeaderFlags.HasStreamingFixes) > 0 )
            {
                uint levelAddedThisFrameCount = chunkReader.ReadIntPacked();
                for( int i = 0; i < levelAddedThisFrameCount; i++ )
                {
                    string levelName = chunkReader.ReadString();
                }
            }
            else
            {
                throw new NotSupportedException( "TODO" );
            }
            long skipExternalOffset = 0;
            if( hasLevelStreamingFixes ) //TODO HasLevelStreamingFixes
            {
                skipExternalOffset = chunkReader.ReadInt64();
            }

            ParseExternalData( chunkReader );//there is a branch on fastForward
            uint seenLevelIndex = 0;

            while( true )
            {
                if( hasLevelStreamingFixes )
                {
                    seenLevelIndex = chunkReader.ReadIntPacked();
                }
                (bool success, int amount) = ParsePacket( chunkReader );
                if( amount == 0 ) break;
                if( appendPacket ) continue;
            }//There is more data ?
            return true;
        }
    }
}
