using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.StreamHelpers
{
    public class BitReader
    {
        byte[] _data;
        byte _bitPositionInCurrentByte;
        long _currentBytePosition;
        byte _lastByteTruncatedBits;//TODO: throw exception if i readed too much bits.
        public BitReader( byte[] data )
        {
            _data = data;
            _bitPositionInCurrentByte = 0;
            _lastByteTruncatedBits = 0;
            _currentBytePosition = 0;
        }
        byte CurrentByte => _data[_currentBytePosition];
        public bool this[long i] => ((
            _data[i / 8]//We select the correct byte
            >> (byte)(7-(i % 8)))//we shift to left to get the correct bit.
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
            bool isLastByte =  (int)Math.Ceiling( (double)positionInBit / 8 ) == _data.Length;
            bool isBitTruncated = (positionInBit % 8)+1 > (8 - _lastByteTruncatedBits);
            return isLastByte && isBitTruncated;
        }

        /// <summary>
        /// Read a byte of 8 bits, move the cursor forward of 8.
        /// </summary>
        /// <returns>The next 8 bits representated in a byte.</returns>
        public byte ReadOneByte()
        {
            if( IsPositionOutOfRange( BitPosition + 7 ) )
            {
                throw new IndexOutOfRangeException();
            }
            unchecked
            {
                byte currentByteShifted = (byte)(CurrentByte >> _bitPositionInCurrentByte);
                byte newByteShifted = 0;
                if( _currentBytePosition < _data.Length - 1 )//We avoid reading outside the array.
                {
                    newByteShifted = (byte)(_data[_currentBytePosition + 1] << (8 - _bitPositionInCurrentByte));
                }
                byte output = (byte)(currentByteShifted | newByteShifted);
                _currentBytePosition++;
                return output;
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
        public void TruncateEnd( int bitCount )
        {
            int byteCountToRemove = bitCount / 8;
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

        public void ShiftLeft (int count )
        {
            long byteToShift = count/8;
            byte[] newData = new byte[_data.Length];
            Array.Copy(_data, byteToShift, newData, 0, _data.Length-byteToShift);
            _data = newData;
            byte bitToShift = (byte)(count % 8);
            for( int i = 0; i + 1 < _data.Length; i++ )
            {
                _data[i] <<= bitToShift;
                _data[i] |= (byte)(_data[i + 1] >> (8 - bitToShift));
            }
            _data[^1] <<= bitToShift;
        }

        
        public void TruncateStart( int amount )
        {
            ShiftLeft( amount );
            TruncateEnd( amount );//We pushed all the bits to truncate at the end, so we have bitToShift bits to truncate at the end.
        }

        public long BitPosition
        {
            get => _bitPositionInCurrentByte + _currentBytePosition * 8;
            set
            {
                _bitPositionInCurrentByte = (byte)(value % 8);
                _currentBytePosition = value / 8;
            }
        }

        public long WholeByteRemaining => _data.Length - _currentBytePosition;
        public long BitRemaining => 8 - _bitPositionInCurrentByte + WholeByteRemaining * 8 - _lastByteTruncatedBits;
    }
}
