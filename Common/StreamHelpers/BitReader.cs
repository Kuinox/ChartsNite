using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common.StreamHelpers
{
    public class BitReader
    {
        Memory<byte> _data;
        byte _lastByteTruncatedBits;//TODO: throw exception if i readed too much bits.
        public BitReader( Memory<byte> data )
        {
            _data = data;
            _lastByteTruncatedBits = 0;

        }
        public bool this[long i] => ((
            _data.Span[(int)(i / 8)]    //We select the correct byte
            >> (byte)(7 - (i % 8)))  //we shift to left to get the correct bit.
            & 1) == 1;               //select the byte at the left and convert it to a bool

        /// <summary>
        /// Read a bit, and move the cursor forward
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">If you read when there is no more data available</exception>
        /// <returns>A bool representating the bit.</returns>
        public bool ReadBit()
        {
            if( IsPositionOutOfRange( BitPosition ) )
            {
                throw new IndexOutOfRangeException();
            }
            bool result = this[BitPosition];
            BitPosition++;
            return result;
        }

        public byte[] ReadBits( long amount )
        {
            var output = ReadBytesAt( BitPosition, (int)((amount + 7) / 8) );
            BitPosition += amount;
            return output;
        }

        /// <summary>
        /// Check if the position is in the <see cref="_data"/> and not in the truncated area
        /// </summary>
        /// <param name="positionInBit"></param>
        /// <returns> <see langword="true"/> if the position is out of range. </returns>
        bool IsPositionOutOfRange( long positionInBit )
        {
#if DEBUG
            return
                    positionInBit >> 3 >= _data.Length
                    || positionInBit >> 3 == _data.Length - 1//Is in the last byte
                    && (positionInBit % 8) + 1 > (8 - _lastByteTruncatedBits); //and is in the truncated part
#else
            return false;
#endif
        }

        /// <summary>
        /// Search in all the sequence, not from the cursor.
        /// Return the index of the bit at the beginning of the byte
        /// </summary>
        /// <param name="reverse"></param>
        /// <returns>The position in bit of the byte, or -1 if not found</returns>
        public long SearchByte( Predicate<byte> byteMatch, bool reverse = false )
        {
            long start = 0;
            long end = BitCount - 8;
            int add = 1;
            if( reverse )
            {
                start = BitCount - 8;
                end = 0;
                add = -1;
            }
            for( long i = start; i < end && !reverse || i > 0 && reverse; i += add )
            {
                if( byteMatch( ReadByteAtPosition( i ) ) ) return i;
            }
            return -1;
        }

        public void RemoveTrailingZeros()
        {
            long bitFoundIndex = SearchByte( b => (b & 1) == 1, true ) + 8;
            TruncateEnd( BitCount - bitFoundIndex );
        }

        /// <summary>
        /// Read a byte of 8 bits, move the cursor forward of 8.
        /// </summary>
        /// <returns>The next 8 bits representated in a byte.</returns>
        public byte ReadOneByte()
        {
            byte output = ReadByteAtPosition( BitPosition );
            BitPosition += 8;
            return output;
        }
        public byte ReadByteAtPosition( long position )
        {
#if DEBUG
            if( IsPositionOutOfRange( BitPosition + 7 ) )
            {
                throw new IndexOutOfRangeException();
            }
            if( position < 0 ) throw new ArgumentException();
#endif
            unchecked
            {
                int positionByteLevel = (int)(position / 8);
                byte positionBitLevel = (byte)(position % 8);
                if( positionBitLevel == 0 )
                {
                    return _data.Span[positionByteLevel];
                }
                return (byte)((byte)(_data.Span[positionByteLevel] << positionBitLevel) | (byte)(_data.Span[positionByteLevel + 1] >> (8 - positionBitLevel)));
            }
        }

        /// <summary>
        /// Read multiple bytes and advance the cursor of 8 per bytes read
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes( int count )
        {
            byte[] output = ReadBytesAt( BitPosition, count );
            BitPosition += count * 8;
            return output;
        }
        public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian( ReadBytes( 2 ) );
        public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian( ReadBytes( 4 ) );
        public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian( ReadBytes( 4 ) );
        public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian( ReadBytes( 8 ) );
        public float ReadSingle() => BitConverter.ToSingle( ReadBytes( 4 ), 0 );
        /// <summary>
        /// Read multiple bytes and advance the cursor of 8 per bytes read
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytesAt( long position, int count )
        {
            byte leftShift = (byte)(position % 8);
            int positionStart = (int)(position / 8);
            try
            {
                if( leftShift == 0 ) return _data[positionStart..(positionStart + count)].ToArray();
            }
            catch( ArgumentException )
            {
                throw new IndexOutOfRangeException();
            }

            byte[] output = new byte[count];
            byte nextByte = _data.Span[positionStart];
            byte rightShift = (byte)(8 - leftShift);
            for( int i = 0; i < count; i++ )
            {
                byte currentByte = nextByte;
                nextByte = _data.Span[positionStart + i];
                unchecked
                {
                    output[i] = (byte)((currentByte << leftShift) | (nextByte >> rightShift));
                }
            }
            return output;
        }

        public uint ReadSerialisedInt( int maxValue )
        {
            uint value = 0;
            long localPos = BitPosition;
            for( uint mask = 1; (value + mask) < maxValue; mask *= 2, localPos++ )
            {
                if( (_data.Span[(int)(localPos >> 3)] & (1 << (int)(localPos & 7))) > 0 )
                {
                    value |= mask;
                }
            }
            BitPosition = localPos;
            return value;
        }

        public uint ReadIntPacked()//TODO: it don't work
        {
            int shiftCount = 0;
            int src = (int)(BitPosition >> 3);
            byte bitcountUsedInByte = (byte)(BitPosition & 7);
            byte bitcountLeftInByte = (byte)(8 - bitcountUsedInByte);
            byte srcMaskByte0 = (byte)((1u << bitcountLeftInByte) - 1);
            byte srcMaskByte1 = (byte)((1u << bitcountUsedInByte) - 1);
            uint value = 0;
            int nextSrcIndex = bitcountUsedInByte != 0 ? 1 : 0;
            for( uint i = 0; i < 5; ++i, shiftCount += 7 )
            {
                BitPosition += 8;
                unchecked
                {
                    byte aByte = (byte)(((_data.Span[src] >> bitcountUsedInByte) & srcMaskByte0) | ((_data.Span[src + nextSrcIndex] & srcMaskByte1) << (bitcountUsedInByte & 7)));
                    bool nextByteIndicator = (aByte & 1) > 0;
                    uint byteAsWord = (byte)(aByte >> 1);
                    value = (byteAsWord << shiftCount) | value;
                    ++src;
                    if( nextByteIndicator )
                    {
                        break;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Truncate the data by the end.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"> Attempt to access truncated data</exception>
        /// <param name="bitCount"></param>
        public void TruncateEnd( long bitCount )
        {
            long byteCountToRemove = bitCount / 8;
            _lastByteTruncatedBits += (byte)(bitCount % 8);
            if( _lastByteTruncatedBits > 7 )
            {
                _lastByteTruncatedBits -= 8;
                byteCountToRemove++;
            }
            if( byteCountToRemove > 0 )
            {
                _data = _data.Slice( 0, (int)(_data.Length - byteCountToRemove) );
            }
        }

        public void ShiftLeft( long count )
        {
            long byteToShift = count / 8;
            byte[] newData = new byte[_data.Length];
            byte[] oldData = _data.ToArray();
            Array.Copy( oldData, byteToShift, newData, 0, _data.Length - byteToShift );
            _data = oldData;
            _data = newData;
            byte bitToShift = (byte)(count % 8);
            for( int i = 0; i + 1 < _data.Length; i++ )
            {
                _data.Span[i] <<= bitToShift;
                _data.Span[i] |= (byte)(_data.Span[i + 1] >> (8 - bitToShift));
            }
            _data.Span[_data.Length-1] <<= bitToShift;
        }

        public uint ReadUInt32( uint max )
        {
            uint value = 0;
            for( uint mask = 1; (value + mask) < max && mask > 0; mask *= 2, BitPosition++ )
            {
                if( BitPosition >= BitCount )
                {
                    throw new EndOfStreamException();
                }
                if( (_data.Span[(int)(BitPosition >> 3)] & (1 << ((int)(BitPosition & 7)))) > 0 )
                {
                    value |= mask;
                }
            }
            return value;
        }
        public void TruncateStart( long amount )
        {
            ShiftLeft( amount );
            TruncateEnd( amount );//We pushed all the bits to truncate at the end, so we have bitToShift bits to truncate at the end.
        }

        public long BitPosition { get; set; }
        public int BytePosition => (int)(BitPosition / 8);
        public int ByteCount => _data.Length;
        public long BitCount => 8 * _data.Length - _lastByteTruncatedBits;

        public bool AtEnd => BitCount == BitPosition;
    }
}
