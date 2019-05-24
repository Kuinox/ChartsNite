using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser
{
    public class ReplayInfo
    {
        public ReplayInfo( ReplayHeader replayHeader, DemoHeader demoHeader )
        {
            ReplayHeader = replayHeader;
            DemoHeader = demoHeader;
        }
        public ReplayHeader ReplayHeader { get; }
        public DemoHeader DemoHeader { get; }
    }
}
