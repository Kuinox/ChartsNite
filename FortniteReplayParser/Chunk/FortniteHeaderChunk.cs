using System;
using System.Collections.Generic;
using System.Text;

namespace FortniteReplayParser.Chunk
{
    public class FortniteHeaderChunk
    {
        public readonly uint BuildNumber;
        public readonly string Release;
        public readonly string SubGame;
        public readonly string MapPath;
        public readonly uint HeaderVersion;
        public readonly uint NotVersion;
        public readonly uint NotSeasonNumber;
        public readonly byte[] GuidLike;
        public readonly uint A20Or21;
        public FortniteHeaderChunk( uint buildNumber,
            string release,
            string subGame,
            string mapPath,
            byte[] guidLike,
            uint headerVersion,
            uint notVersion,
            uint notSeasonNumber,
            uint a20Or21)
        {
            BuildNumber = buildNumber;
            Release = release;
            SubGame = subGame;
            MapPath = mapPath;
            GuidLike = guidLike;
            HeaderVersion = headerVersion;
            NotVersion = notVersion;
            NotSeasonNumber = notSeasonNumber;
            A20Or21 = a20Or21;
        }
    }
}
