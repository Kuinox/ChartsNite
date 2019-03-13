using Common.StreamHelpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Tests
{
    public class BitReaderTest
    {
        static readonly bool[] _truthTableBits = new bool[] {
                true, false, true, false, true, true, false, true,
                true, true, true, true, true, true, true, false,
                true, false, false, false, false, false, true, false };
        static readonly byte[] _truthTableByte = new byte[] { 173, 254, 130 };
        [Test]
        public void ReadBitsCorrectly()
        {
            BitReader reader = new BitReader( _truthTableByte );
            for( int i = 0; i < _truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
        }

        [Test]
        public void ReadBytesCorrectly()
        {
            BitReader reader = new BitReader( _truthTableByte );
            reader.ReadBytes( (int)reader.WholeByteRemaining ).Should().BeEquivalentTo( _truthTableByte );
        }
        [Test]
        public void ReadMixedCorrectly()
        {
            BitReader reader = new BitReader( _truthTableByte );
            reader.ReadBit().Should().Be( true );
            reader.ReadOneByte().Should().Be( 86 );
            for( int i = 9; i < _truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
        }

        [Test]
        public void ShiftLeft()
        {
            BitReader reader = new BitReader( _truthTableByte );
            reader.ShiftLeft( 1 );
            for( int i = 1; i < _truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
            reader.ReadBit().Should().BeFalse();
        }

        [Test]
        public void ReadOutOfRangeShouldThrow()
        {
            BitReader reader = new BitReader( _truthTableByte );
            for( int i = 0; i < _truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>(()=> reader.ReadBit() );
            BitReader reader2 = new BitReader( _truthTableByte );
            for( int i = 0; i < _truthTableBits.Length-3; i++ )
            {
                reader2.ReadBit().Should().Be( _truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( ()=> reader.ReadBytes( 1 ));
        }
        [Test]
        public void TruncateStart()
        {
            BitReader reader = new BitReader( _truthTableByte );
            reader.TruncateStart(11);
            for( int i = 11; i < _truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>(() => reader.ReadBit());
        }

        [Test]
        public void TruncateEnd()
        {
            BitReader reader = new BitReader( _truthTableByte );
            reader.TruncateEnd( 9 );
            for( int i = 0; i < _truthTableBits.Length-9; i++ )
            {
                reader.ReadBit().Should().Be( _truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
        }


    }
}
