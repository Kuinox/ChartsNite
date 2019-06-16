using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealReplayParser;

namespace ChartsNite.UnrealReplayParser.StreamArchive
{
    public class ChunkArchive : Archive
    {
        readonly MemoryReader _reader;


        public ChunkArchive(Memory<byte> memory, DemoHeader.EngineNetworkVersionHistory engineNetVer ) : base( engineNetVer )
        {
            _reader = new MemoryReader( memory, Endianness.Little );
        }

        public override int RemainingByte => throw new NotImplementedException();

        public override int Length => _reader.Length;
        public override int Offset => _reader.Offset;

        public override Span<byte> ReadBits( long amount ) => _reader.ReadBytes( (int)((amount + 7) / 8) ).Span;

        public override byte ReadByte() => _reader.ReadOneByte();

        public override Span<byte> ReadBytes( int amount ) => _reader.ReadBytes( amount ).Span;

        public override int ReadInt32() => _reader.ReadInt32();

        
        public override ushort ReadUInt16() => _reader.ReadUInt16();

        public override uint ReadUInt32( uint max ) => _reader.ReadUInt32();

        public override uint ReadUInt32() => _reader.ReadUInt32();

        public override long ReadInt64() => _reader.ReadInt64();

        public override float ReadSingle() => _reader.ReadSingle();

        /// <summary>
        /// In UnrealEngine source code: void FArchive::SerializeIntPacked( uint32& Value )
        /// </summary>
        /// <returns></returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override uint ReadIntPacked()
        {   
            uint value = 0;
            byte count = 0;
            bool more = true;

            while( more )
            {
                byte nextByte = _reader.ReadOneByte();
                more = (nextByte & 1) == 1;         // Check 1 bit to see if theres more after this
                nextByte >>= 1;           // Shift to get actual 7 bit value
                value += (uint)nextByte << (7 * count++); // Add to total value
            }
            return value;
        }


    }
}
