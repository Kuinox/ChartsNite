using System;
using System.Collections.Generic;
using System.Text;
using Common.StreamHelpers;
using UnrealReplayParser;

namespace UnrealReplayParser.Chunk
{
    class HeaderInfo : ChunkInfo
    {
        public HeaderInfo(ChunkInfo info): base(info)
        {
            
        }

        protected HeaderInfo(HeaderInfo info) : base(info)
        {
        }
    }
}
