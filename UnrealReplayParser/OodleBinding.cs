using System;
using System.Runtime.InteropServices;
using System.Buffers;
using System.Threading;
using System.Collections.Concurrent;

namespace UnrealReplayParser
{
    public class ReducedMemory : IMemoryOwner<byte>
    {
        readonly IMemoryOwner<byte> _memoryOwner;
        readonly Range _range;

        public ReducedMemory( IMemoryOwner<byte> memoryOwner, Range range )
        {
            _memoryOwner = memoryOwner;
            _range = range;
        }

        public Memory<byte> Memory => _memoryOwner.Memory[_range];

        public void Dispose()
        {
            _memoryOwner.Dispose();
        }
    }
    public class OodleBinding
    {
        // Import Oodle decompression
        [DllImport( "oo2core_5_win64.dll" )]
        private static extern unsafe int OodleLZ_Decompress( void* buffer, long bufferSize, void* outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule );

        /// <summary>
        /// Buffer output may be bigger than what was decompressed
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <param name="uncompressedSize"></param>
        /// <returns></returns>
        public static IMemoryOwner<byte> Decompress( Memory<byte> inputBuffer, int uncompressedSize )
        {
            IMemoryOwner<byte> outputBuffer = MemoryPool<byte>.Shared.Rent( uncompressedSize );
            int decompressedCount;
            unsafe
            {
                using( MemoryHandle inputBufferPointer = inputBuffer.Pin() )
                using( MemoryHandle outputBufferPointer = outputBuffer.Memory.Pin() )
                {
                    decompressedCount = OodleLZ_Decompress( inputBufferPointer.Pointer, inputBuffer.Length, outputBufferPointer.Pointer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 );
                }
            }
            if( decompressedCount == uncompressedSize )
            {
                return new ReducedMemory( outputBuffer, new Range( 0, decompressedCount ) );
            }
            throw new Exception( "There was an error while decompressing." );
        }

        [DllImport( "oo2core_5_win64.dll" )]
        private static extern int OodleLZ_Compress( OodleFormat format, byte[] buffer, long bufferSize, byte[] outputBuffer, OodleCompressionLevel level, uint unused1, uint unused2, uint unused3 );
        public static byte[] Compress( byte[] buffer, int size, OodleFormat format, OodleCompressionLevel level )
        {
            uint compressedBufferSize = GetCompressionBound( (uint)size );
            byte[] compressedBuffer = new byte[compressedBufferSize];

            int compressedCount = OodleLZ_Compress( format, buffer, size, compressedBuffer, level, 0, 0, 0 );

            byte[] outputBuffer = new byte[compressedCount];
            Buffer.BlockCopy( compressedBuffer, 0, outputBuffer, 0, compressedCount );

            return outputBuffer;
        }

        private static uint GetCompressionBound( uint bufferSize )
        {
            return bufferSize + 274 * ((bufferSize + 0x3FFFF) / 0x40000);
        }

        public enum OodleFormat : uint
        {
            LZH,
            LZHLW,
            LZNIB,
            None,
            LZB16,
            LZBLW,
            LZA,
            LZNA,
            Kraken,
            Mermaid,
            BitKnit,
            Selkie,
            Akkorokamui
        }

        public enum OodleCompressionLevel : ulong
        {
            None,
            SuperFast,
            VeryFast,
            Fast,
            Normal,
            Optimal1,
            Optimal2,
            Optimal3,
            Optimal4,
            Optimal5
        }
    }
}
