using ChartsNite.UnrealReplayParser;
using ChartsNite.UnrealReplayParser.StreamArchive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static UnrealReplayParser.DemoHeader;

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
        public static NetFieldExport InstantiateNotExported() => new NetFieldExport( false, 0, 0, "", "" );
        public static NetFieldExport InstantiateExported( uint handle, uint compatibleChecksum, string name, string type ) => new NetFieldExport( true, handle, compatibleChecksum, name, type );

        public bool Exported { get; set; }
        public uint Handle { get; set; }
        public uint CompatibleChecksum { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public static class NetFieldExportParsing
    {
        public static NetFieldExport ReadNetFieldExport( this Archive ar )
        {
            bool exported = ar.ReadByte() == 1;
            if( !exported ) return NetFieldExport.InstantiateNotExported();
            uint handle = ar.ReadIntPacked();
            uint checksum = ar.ReadUInt32();
            if( ar.EngineNetVer < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZATION )
            {
                string name = ar.ReadString();
                string type = ar.ReadString();
                return NetFieldExport.InstantiateExported( handle, checksum, name, type );
            }
            if( ar.EngineNetVer < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZE_FIX )
            {
                string exportName = ar.ReadString();
                return NetFieldExport.InstantiateExported( handle, checksum, exportName, "" );
            }
            var staticName = ar.ReadStaticName();
            return NetFieldExport.InstantiateExported( handle, checksum, staticName.Name, "" );
        }

        public static async ValueTask<NetFieldExport> ReadNetFieldExportAsync( this ArchiveAsync ar )
        {
            bool exported = await ar.ReadByteAsync() == 1;
            if( !exported ) return NetFieldExport.InstantiateNotExported();
            uint handle = await ar.ReadIntPackedAsync();
            uint checksum = await ar.ReadUInt32Async();
            if( ar.EngineNetVer < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZATION )
            {
                string name = await ar.ReadStringAsync();
                string type = await ar.ReadStringAsync();
                return NetFieldExport.InstantiateExported( handle, checksum, name, type );
            }
            if( ar.EngineNetVer < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZE_FIX )
            {
                string exportName = await ar.ReadStringAsync();
                return NetFieldExport.InstantiateExported( handle, checksum, exportName, "" );
            }
            StaticName staticName = await ar.ReadStaticNameAsync();
            return NetFieldExport.InstantiateExported( handle, checksum, staticName.Name, "" );
        }
    }
}
