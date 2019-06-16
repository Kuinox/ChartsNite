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
    /// <summary>
    /// Archive representating data that we can access asynchronously.
    /// </summary>
    public abstract class ArchiveAsync
    {

        public ArchiveAsync( EngineNetworkVersionHistory engineNetVer )
        {
            EngineNetVer = engineNetVer;
        }

        public abstract ValueTask<int> ReadInt32Async( uint max );
        public abstract ValueTask<int> ReadInt32Async();

        public abstract ValueTask<long> ReadInt64Async();

        public abstract ValueTask<uint> ReadUInt32Async( uint max );
        public abstract ValueTask<uint> ReadUInt32Async();


        public abstract ValueTask<uint> ReadIntPackedAsync();

        public abstract ValueTask<byte> ReadByteAsync();

        public abstract ValueTask<ushort> ReadUInt16Async();
        public abstract ValueTask<short> ReadInt16Async();

        public abstract ValueTask<Memory<byte>> ReadBitsAsync( long amount );

        public abstract ValueTask<Memory<byte>> ReadBytesAsync( int amount );

        public EngineNetworkVersionHistory EngineNetVer { get; }

        public async ValueTask<string> ReadStringAsync()
        {
            int length = await ReadInt32Async();
            if( length == -2147483648 )//if we reverse this, it overflow
            {
                throw new InvalidDataException( "The size of the string has an invalid value" );
            }
            if( length == 0 )
            {
                return "";
            }
            bool isUnicode = length < 0;
            string value;
            if( isUnicode )
            {
                length = -length;
                value = Encoding.Unicode.GetString( (await ReadBytesAsync( length * 2 ))[0..Index.FromEnd( 2 )].Span );
            }
            else
            {
                value = Encoding.ASCII.GetString( (await ReadBytesAsync( length ))[0..Index.FromEnd( 1 )].Span );
            }
            return value;
        }

        public async ValueTask<T[]> ReadArrayAsync<T>( Func<ValueTask<T>> baseTypeParser )
        {
            int length = await ReadInt32Async();
            Debug.Assert( length >= 0 );
            T[] output = new T[length];
            for( int i = 0; i < length; i++ )
            {
                output[i] = await baseTypeParser();
            }
            return output;
        }
    }
}
