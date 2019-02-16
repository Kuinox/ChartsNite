using FortniteReplayParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;

namespace ChartsNite.ReplayOrganizer
{
    class FortniteDataGrabber : FortniteReplayVisitor
    {
        public ReplayInfo? ReplayInfo { get; private set; }
        public FortniteDataGrabber(Stream stream) : base(stream)
        {
        }
        public override Task<bool> VisitReplayInfo(ReplayInfo replayInfo)
        {
            ReplayInfo = replayInfo;
            return base.VisitReplayInfo(replayInfo);
        }}
}
