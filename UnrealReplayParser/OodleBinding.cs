using System;
using System.Runtime.InteropServices;
using System.Buffers;

namespace UnrealReplayParser
{
    public class OodleBinding
    {
        // Import Oodle decompression
        [DllImport( "oo2core_5_win64.dll" )]
        private static extern unsafe int OodleLZ_Decompress( void* buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule );

        public static byte[] Decompress(Memory<byte> buffer, int uncompressedSize)//Maybe reuse the buffer ?
        {
            byte[] decompressedBuffer = new byte[uncompressedSize];
            int decompressedCount;
            unsafe
            {
                using( MemoryHandle bufferPointer = buffer.Pin() )
                {
                    decompressedCount = OodleLZ_Decompress( bufferPointer.Pointer, buffer.Length, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 );
                }
            }
            if( decompressedCount == uncompressedSize )
            {
                return decompressedBuffer;
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
