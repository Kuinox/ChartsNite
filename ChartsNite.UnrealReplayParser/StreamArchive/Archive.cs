using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;
using UnrealReplayParser.UnrealObject;
using static UnrealReplayParser.DemoHeader;

namespace ChartsNite.UnrealReplayParser.StreamArchive
{
    public abstract class Archive
    {

        public Archive( EngineNetworkVersionHistory engineNetVer )
        {
            EngineNetVer = engineNetVer;
        }

        public abstract int Length { get; }
        public abstract int Offset { get; }


        public abstract float ReadSingle();

        public abstract long ReadInt64();

        public abstract int ReadInt32();

        public abstract uint ReadUInt32( uint max );
        public abstract uint ReadUInt32();

        public abstract uint ReadIntPacked();

        public abstract byte ReadByte();
        public abstract ushort ReadUInt16();
        public abstract Span<byte> ReadBits( long amount );

        public abstract Span<byte> ReadBytes( int amount );

        public abstract int RemainingByte { get; }

        public EngineNetworkVersionHistory EngineNetVer { get; }

        public string ReadString()
        {
            int length = ReadInt32();
            if( length == -2147483648 )//if we reverse this, it overflow
            {
                throw new InvalidDataException( "The size of the string has an invalid value" );
            }
            if( length == 0 ) return "";
            bool isUnicode = length < 0;
            if( isUnicode )
            {
                length = -length;

                return Encoding.Unicode.GetString( ReadBytes( length * 2 - 2 ) );
            }
            else
            {
                return Encoding.ASCII.GetString( ReadBytes( length - 1 ) );
            }
        }

        public T[] ReadArray<T> (Func<T> baseTypeParser)
        {
            int length = ReadInt32();
            Debug.Assert( length >= 0 );
            T[] output = new T[length];
            for( int i = 0; i < length; i++ )
            {
                output[i] = baseTypeParser();
            }
            return output;
        }
       
    }
}
