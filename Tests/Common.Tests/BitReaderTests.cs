using Common.StreamHelpers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Tests
{
    public class BitReaderTest
    {

        [Test]
        public void ReadBitsCorrectly()
        {
            bool[] truthTable = new bool[] { true, false, true, true, false, true, false, true, false, true, true, true, true, true, true, true, false, true, false, false, false, false, false, true };
            BitReader reader = new BitReader( new byte[] { 173, 254, 130 } );
            for( int i = 0; i < truthTable.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTable[i] );
            }
        }

        [Test]
        public void ReadBytesCorrectly()
        {
            byte[] truthTable = new byte[] { 173, 254, 130 };
            BitReader reader = new BitReader( truthTable );
            reader.ReadBytes( truthTable.Length ).Should().BeEquivalentTo( truthTable );
        }
        [Test]
        public void ReadMixedCorrectly()
        {
            bool[] truthTable = new bool[] { true, false, true, true, false, true, false, true, false, true, true, true, true, true, true, true, false, true, false, false, false, false, false, true };
            BitReader reader = new BitReader( new byte[] { 173, 254, 130 } );
            reader.ReadBit().Should().Be( true );
            reader.ReadOneByte().Should().Be( 86 );
            for( int i = 9; i < truthTable.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTable[i] );
            }
        }

        [Test]
        public void ReadOutOfRangeShouldThrow()
        {
            bool[] truthTable = new bool[] { true, false, true, true, false, true, false, true, false, true, true, true, true, true, true, true, false, true, false, false, false, false, false, true };
            BitReader reader = new BitReader( new byte[] { 173, 254, 130 } );
            for( int i = 0; i < truthTable.Length; i++ )
            {
                reader.ReadBit().Should().Be( truthTable[i] );
            }
            Assert.Throws<IndexOutOfRangeException>(()=> reader.ReadBit() );
            BitReader reader2 = new BitReader( new byte[] { 173, 254, 130 } );
            for( int i = 0; i < truthTable.Length-3; i++ )
            {
                reader2.ReadBit().Should().Be( truthTable[i] );
            }
            Assert.Throws<IndexOutOfRangeException>( ()=> reader.ReadBytes( 1 ));
        }
        [Test]
        public void TruncateCorrectly()
        {
            bool[] truthTable = new bool[]
            {
                true, false, true, true, false, true, false, true,
                false, true, true, true, true, true, true, true,
                false, true, false, false, false, false, false, true
            };
            BitReader reader = new BitReader( new byte[] { 173, 254, 130 } );
            reader.TruncateStart(11);
            reader.TruncateEnd(9);
            for( int i = 11; i < truthTable.Length-9; i++ )
            {
                reader.ReadBit().Should().Be(truthTable[i]);
            }
            Assert.Throws<IndexOutOfRangeException>(() => reader.ReadBit());
        } 
    }
}
