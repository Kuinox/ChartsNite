using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;

namespace Common.StreamHelpers
{
    public class CustomBinaryReaderAsync : BinaryReaderAsync, IAsyncDisposable
    {
        readonly bool _leaveOpen;
        public CustomBinaryReaderAsync( Stream input, bool leaveOpen = false ) : base( input, leaveOpen )
        {
            if( input == null ) throw new ArgumentNullException( "input" );
            if( !input.CanRead ) throw new ArgumentException( "Can't read input stream." );
        }
        public async ValueTask<Memory<byte>> DumpRemainingBytesAsync()
        {
            return await ReadBytesAsync( (int)(BaseStream.Length - BaseStream.Position) );
        }

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
                value = Encoding.Unicode.GetString( (await ReadBytesAsync( length * 2 )).Span );
            }
            else
            {
                value = Encoding.ASCII.GetString( (await ReadBytesAsync( length )).Span );
            }
            return value.Trim( ' ', '\0' );
        }


        /// <summary>
        /// In UnrealEngine source code: void FArchive::SerializeIntPacked( uint32& Value )
        /// </summary>
        /// <returns></returns>
        public async ValueTask<uint> ReadIntPackedAsync()
        {
            uint value = 0;
            byte count = 0;
            bool more = true;

            while( more )
            {
                byte nextByte = await ReadByteAsync();
                more = (nextByte & 1) == 1;         // Check 1 bit to see if theres more after this
                nextByte >>= 1;           // Shift to get actual 7 bit value
                value += (uint)nextByte << (7 * count++); // Add to total value
            }
            return value;
        }

        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// Will simply read the chunk of data and return it if it's not needed.
        /// I didn't test against not compressed replay, so it may fail.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns>Readable data uncompresed if it was needed</returns>
        public virtual async ValueTask<CustomBinaryReader> UncompressData( )//TODO change what i return
        {
            int decompressedSize = await ReadInt32Async();
            int compressedSize = await ReadInt32Async();
            Memory<byte> compressedBuffer = await ReadBytesAsync( compressedSize );//TODO: Use Memory<T>
            return new CustomBinaryReader( new MemoryStream( OodleBinding.Decompress(compressedBuffer, decompressedSize)) );//TODO: is there nothing better ? https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream
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

        public async ValueTask DisposeAsync()
        {
            if( !_leaveOpen )
            {
                await BaseStream.DisposeAsync();
            }
        }
    }
}
