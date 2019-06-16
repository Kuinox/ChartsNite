using ChartsNite.UnrealReplayParser;
using ChartsNite.UnrealReplayParser.StreamArchive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser.UnrealObject
{
    public class StaticName
    {
        public StaticName( uint id, string name, bool hardCoded )
        {
            Id = id;
            Name = name;
            HardCoded = hardCoded;
        }

        public uint Id { get; }
        public string Name { get; }
        public bool HardCoded { get; set; }


    }

    public class StaticNameParsing
    {
        public static StaticName ReadStaticName( Archive ar )
        {
            Span<byte> bits = ar.ReadBits( 1 );
            Debug.Assert( bits.Length == 1 );
            bool hardcoded = bits.ToArray()[0] > 0;
            if( hardcoded )
            {
                const uint MAX_NETWORKED_HARDCODED_NAME = 410;
                uint nameIndex;
                if( ar.EngineNetVer < DemoHeader.EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
                {
                    nameIndex = ar.ReadUInt32( MAX_NETWORKED_HARDCODED_NAME + 1 );
                }
                else
                {
                    nameIndex = ar.ReadIntPacked();
                }

                if( nameIndex >= MAX_NETWORKED_HARDCODED_NAME ) throw new InvalidDataException();
                return new StaticName( nameIndex, "", true ); //hard coded names in "UnrealNames.inl". Didn't searched it yet, i have no need of it right now.
            }
            else
            {
                string inString = ar.ReadString();
                int inNumber = ar.ReadInt32();
                return new StaticName( (uint)inNumber, inString, false );
            }
        }

        public static async ValueTask<StaticName> ReadStaticNameAsync( ArchiveAsync ar )
        {
            Memory<byte> bits = await ar.ReadBitsAsync( 1 );
            Debug.Assert( bits.Length == 1 );
            bool hardcoded = bits.ToArray()[0] > 0;
            if( hardcoded )
            {
                const uint MAX_NETWORKED_HARDCODED_NAME = 410;
                uint nameIndex;
                if( ar.EngineNetVer < DemoHeader.EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
                {
                    nameIndex = await ar.ReadUInt32Async( MAX_NETWORKED_HARDCODED_NAME + 1 );
                }
                else
                {
                    nameIndex = await ar.ReadIntPackedAsync();
                }

                if( nameIndex >= MAX_NETWORKED_HARDCODED_NAME ) throw new InvalidDataException();
                return new StaticName( nameIndex, "", true ); //hard coded names in "UnrealNames.inl". Didn't searched it yet, i have no need of it right now.
            }
            else
            {
                string inString = await ar.ReadStringAsync();
                int inNumber = await ar.ReadInt32Async();
                return new StaticName( (uint)inNumber, inString, false );
            }
        }
    }
}
