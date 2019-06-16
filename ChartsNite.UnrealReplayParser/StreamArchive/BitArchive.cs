using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealReplayParser;

namespace ChartsNite.UnrealReplayParser.StreamArchive
{
    class BitArchive : Archive
    {
        BitReader _bitReader;
        public BitArchive( DemoHeader.EngineNetworkVersionHistory engineNetVer ) : base( engineNetVer )
        {

        }

        public override int Length => _bitReader.ByteCount;

        public override int Offset => _bitReader.BytePosition;

        public override int RemainingByte => throw new NotImplementedException();

        public override Span<byte> ReadBits( long amount ) => _bitReader.ReadBits( amount );

        public override byte ReadByte() => _bitReader.ReadOneByte();

        public override Span<byte> ReadBytes( int amount ) => _bitReader.ReadBytes( amount );

        public override int ReadInt32() => _bitReader.ReadInt32();

        public override long ReadInt64() => _bitReader.ReadInt64();

        public override uint ReadIntPacked() => _bitReader.ReadIntPacked();

        public override float ReadSingle() => _bitReader.ReadSingle();

        public override ushort ReadUInt16() => _bitReader.ReadUInt16();

        public override uint ReadUInt32() => _bitReader.ReadUInt32();

        public override uint ReadUInt32( uint max ) => _bitReader.ReadUInt32( max );
    }
}
