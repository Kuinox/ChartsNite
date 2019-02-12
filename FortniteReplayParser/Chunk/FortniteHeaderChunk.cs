using System;
using System.Collections.Generic;
using System.Text;

namespace FortniteReplayParser.Chunk
{
    public class FortniteHeaderChunk
    {
        public readonly uint Version;
        public readonly string Release;
        public readonly string SubGame;
        public readonly string MapPath;
        public FortniteHeaderChunk(uint version, string release, string subGame, string mapPath)
        {
            Version = version;
            Release = release;
            SubGame = subGame;
            MapPath = mapPath;
        }
    }
}
