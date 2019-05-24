using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser.UnrealObject
{
    public class NetFieldExport
    {
        NetFieldExport( bool exported, uint handle, uint compatibleChecksum, string name, string type )
        {
            Exported = exported;
            Handle = handle;
            CompatibleChecksum = compatibleChecksum;
            Name = name;
            Type = type;
        }
        public static NetFieldExport InitializeNotExported() => new NetFieldExport( false, 0, 0, "", "" );
        public static NetFieldExport InitializeExported( uint handle, uint compatibleChecksum, string name, string type ) => new NetFieldExport( true, handle, compatibleChecksum, name, type );

        public bool Exported { get; set; }
        public uint Handle { get; set; }
        public uint CompatibleChecksum { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
