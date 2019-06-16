using Common.StreamHelpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Tests
{
    public class BitReaderTest
    {
        static bool[] TruthTableBits() => new bool[] {
                true, false, true, false, true, true, false, true,
                true, true, true, true, true, true, true, false,
                true, false, false, false, false, false, true, false };
        static byte[] TruthTableByte() => new byte[] { 173, 254, 130 };
        [Test]
        public void ReadBitsCorrectly()
        {
            byte[] truthTable = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTable );
            for( int i = 0; i < truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
        }

        [Test]
        public void ReadBytesCorrectly()
        {
            byte[] truthTable = TruthTableByte();
            BitReader reader = new BitReader( truthTable );
            reader.ReadBytes( truthTable.Length ).Should().BeEquivalentTo( truthTable );
        }
        [Test]
        public void ReadMixedCorrectly()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            reader.ReadBit().Should().Be( true );
            reader.ReadOneByte().Should().Be( 91 );
            for( int i = 9; i < truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
        }

        [Test]
        public void ShiftLeft()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            reader.ShiftLeft( 1 );
            for( int i = 1; i < truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            reader.ReadBit().Should().BeFalse();
        }

        [Test]
        public void ReadOutOfRangeShouldThrow()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            for( int i = 0; i < truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
            BitReader reader2 = new BitReader( truthTableByte );
            for( int i = 0; i < truthTableBits.Length - 3; i++ )
            {
                reader2.ReadBit().Should().Be( truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBytes( 1 ) );
        }
        [Test]
        public void TruncateStart()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            reader.TruncateStart( 11 );
            for( int i = 11; i < truthTableBits.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
        }

        [Test]
        public void TruncateEnd()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            reader.TruncateEnd( 9 );
            for( int i = 0; i < truthTableBits.Length - 9; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
        }

        [Test]
        public void FindByte()
        {
            BitReader reader = new BitReader( new byte[] { 147, 78, 168, 0 } );
            reader.SearchByte( b => b == 80, true ).Should().Be( 17 );
        }

        [Test]
        public void RemoveTrailingZeros()
        {
            byte[] truthTableByte = TruthTableByte();
            bool[] truthTableBits = TruthTableBits();
            BitReader reader = new BitReader( truthTableByte );
            reader.RemoveTrailingZeros();
            int i;
            for( i = 0; i < truthTableBits.Length-1; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            i.Should().Be( truthTableBits.Length - 1 );
        }

        [Test]
        public void RemoveTrailingZerosDoesNotTruncateWhereThereIsNoTrailingZero()
        {
            bool[] truthTableBits = TruthTableBits();
            byte[] truthTable = new byte[] { 173, 254, 131 };
            BitReader reader = new BitReader( truthTable );
            for( int i = 0; i < truthTableBits.Length-1; i++ )
            {
                reader.ReadBit().Should().Be( truthTableBits[i] );
            }
            reader.ReadBit().Should().BeTrue();
            Assert.Throws<IndexOutOfRangeException>( () => reader.ReadBit() );
        }
    }
}
