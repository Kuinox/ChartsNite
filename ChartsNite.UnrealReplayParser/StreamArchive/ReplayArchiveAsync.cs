using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;

namespace ChartsNite.UnrealReplayParser.StreamArchive
{
    public class ReplayArchiveAsync : ArchiveAsync, IAsyncDisposable, IDisposable
    {
        readonly BinaryReaderAsync _reader;
        private readonly bool _compressed;
        public ReplayArchiveAsync( Stream input, DemoHeader.EngineNetworkVersionHistory version, bool compressed, bool leaveOpen = false ) : base( version )
        {
            if( !input.CanRead ) throw new ArgumentException( "Can't read input stream." );
            _compressed = compressed;
            _reader = new BinaryReaderAsync( input, leaveOpen );
        }

        public void Dispose() => _reader.Dispose();

        public async ValueTask DisposeAsync() => await _reader.DisposeAsync();

        public override ValueTask<Memory<byte>> ReadBitsAsync( long amount ) => _reader.ReadBytesAsync( (int)((amount + 7) / 8) );

        public override ValueTask<byte> ReadByteAsync() => _reader.ReadByteAsync();

        public override ValueTask<Memory<byte>> ReadBytesAsync( int amount ) => _reader.ReadBytesAsync( amount );

        public override ValueTask<short> ReadInt16Async() => _reader.ReadInt16Async();

        public override ValueTask<int> ReadInt32Async( uint max ) => _reader.ReadInt32Async();//Directly on the replay the size is not used !

        public override ValueTask<int> ReadInt32Async() => _reader.ReadInt32Async();

        public override ValueTask<long> ReadInt64Async() => _reader.ReadInt64Async();

        public override async ValueTask<uint> ReadIntPackedAsync()
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

        public override ValueTask<ushort> ReadUInt16Async() => _reader.ReadUInt16Async();

        public override ValueTask<uint> ReadUInt32Async( uint max ) => _reader.ReadUInt32Async();

        public override ValueTask<uint> ReadUInt32Async() => _reader.ReadUInt32Async();

        /// <summary>
        /// Will uncompress if needed, then return the array of bytes.
        /// Will simply read the chunk of data and return it if it's not needed.
        /// I didn't test against not compressed replay, so it may fail.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="replayDataInfo"></param>
        /// <returns>Readable data uncompresed if it was needed</returns>
        public virtual async ValueTask<IMemoryOwner<byte>> UncompressData()
        {
            if( _compressed )
            {
                int decompressedSize = await ReadInt32Async();
                int compressedSize = await ReadInt32Async();
                Memory<byte> compressedBuffer = await ReadBytesAsync( compressedSize );
                return OodleBinding.Decompress( compressedBuffer, decompressedSize );
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
