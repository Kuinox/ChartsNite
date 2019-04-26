using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.StreamHelpers
{
    public class BitReader
    {
        byte[] _data;
        byte _lastByteTruncatedBits;//TODO: throw exception if i readed too much bits.
        public BitReader( byte[] data )
        {
            _data = data;
            _lastByteTruncatedBits = 0;
        }
        public bool this[long i] => ((
            _data[i / 8]//We select the correct byte
            >> (byte)(7 - (i % 8)))//we shift to left to get the correct bit.
            & 1) == 1;//select the byte at the left and convert it to a bool


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
                if( byteMatch( ReadOneByteAt( i ) ) ) return i;
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
            byte output = ReadOneByteAt( BitPosition );
            BitPosition += 8;
            return output;
        }
        public byte ReadOneByteAt( long position )
        {
            if( IsPositionOutOfRange( BitPosition + 7 ) )
            {
                throw new IndexOutOfRangeException();
            }
            if( position < 0 ) throw new ArgumentException();
            unchecked
            {
                int bytePosition = (int)(position / 8);
                byte bitPositionInByte = (byte)(position % 8);
                if( bitPositionInByte == 0 )
                {
                    return _data[bytePosition];
                }
                byte firstByteShifted = (byte)(_data[bytePosition] << bitPositionInByte);
                byte secondByteShifted = (byte)(_data[bytePosition + 1] >> (8 - bitPositionInByte));
                return (byte)(firstByteShifted | secondByteShifted);
            }
        }

        /// <summary>
        /// Read multiple bytes and advance the cursor of 8 per bytes read
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes( int count )
        {
            byte[] output = new byte[count];
            for( int i = 0; i < count; i++ )
            {
                output[i] = ReadOneByte();
            }
            return output;
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
                byte[] newData = new byte[_data.Length - byteCountToRemove];
                Array.Copy( _data, 0, newData, 0, newData.Length );
                _data = newData;
            }
        }

        public void ShiftLeft( long count )
        {
            long byteToShift = count / 8;
            byte[] newData = new byte[_data.Length];
            Array.Copy( _data, byteToShift, newData, 0, _data.Length - byteToShift );
            _data = newData;
            byte bitToShift = (byte)(count % 8);
            for( int i = 0; i + 1 < _data.Length; i++ )
            {
                _data[i] <<= bitToShift;
                _data[i] |= (byte)(_data[i + 1] >> (8 - bitToShift));
            }
            _data[^1] <<= bitToShift;
        }


        public void TruncateStart( long amount )
        {
            ShiftLeft( amount );
            TruncateEnd( amount );//We pushed all the bits to truncate at the end, so we have bitToShift bits to truncate at the end.
        }

        public long BitPosition { get; set; }

        public long BitCount => 8 * _data.Length - _lastByteTruncatedBits;
    }
}
