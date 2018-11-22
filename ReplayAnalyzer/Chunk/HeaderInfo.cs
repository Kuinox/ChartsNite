using System;
using System.Collections.Generic;
using System.Text;
using Common.StreamHelpers;
using ReplayAnalyzer;

namespace UnrealReplayAnalyzer.Chunk
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
