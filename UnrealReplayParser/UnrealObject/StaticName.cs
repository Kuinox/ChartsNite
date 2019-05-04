using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser.UnrealObject.Types
{
    public class StaticName
    {
        public StaticName(uint id, string name, bool hardCoded)
        {
            Id = id;
            Name = name;
            HardCoded = hardCoded;
        }

        public uint Id { get; }
        public string Name { get; }
        public bool HardCoded { get; set; }
    }
}
