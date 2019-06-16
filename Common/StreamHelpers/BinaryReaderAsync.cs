///From https://github.com/dotnet/coreclr/blob/52aff202cd382c233d903d432da06deffaa21868/src/System.Private.CoreLib/shared/System/IO/BinaryReader.cs
///Adapted to use Async
/*============================================================
**
** 
** 
**
**
** Purpose: Wraps a stream and provides convenient read functionality
** for strings and primitive types.
**
**
============================================================*/

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public class BinaryReaderAsync : IDisposable, IAsyncDisposable
    {
        readonly Stream _stream;
        readonly Memory<byte> _buffer;

        // Performance optimization for Read() w/ Unicode.  Speeds us up by ~40% 
        readonly bool _leaveOpen;
        bool _disposed;

        public BinaryReaderAsync( Stream input, bool leaveOpen = false )
        {
            if( input == null )
            {
                throw new ArgumentNullException( nameof( input ) );
            }
            if( !input.CanRead )
            {
                throw new ArgumentException( "Stream not readable" );
            }
            _stream = input;
            int minBufferSize = 16;  // max bytes per one char
            if( minBufferSize < 16 )
            {
                minBufferSize = 16;
            }
            _buffer = new byte[minBufferSize];
            // _charBuffer and _charBytes will be left null.
            _leaveOpen = leaveOpen;
        }

        public virtual Stream BaseStream
        {
            get
            {
                return _stream;
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if( !_disposed )
            {
                if( disposing && !_leaveOpen )
                {
                    _stream.Close();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose( true );
        }

        /// <remarks>
        /// Override Dispose(bool) instead of Close(). This API exists for compatibility purposes.
        /// </remarks>
        public virtual void Close()
        {
            Dispose( true );
        }

        private void ThrowIfDisposed()
        {
            if( _disposed )
            {
                throw new IOException( "File not open" );//idk what was doing "Error.FileNotOpen" so i throw this instead
            }
        }


        public virtual async ValueTask<byte> ReadByteAsync() => (await InternalReadAsync( 1 )).Span[0];
        public virtual async ValueTask<short> ReadInt16Async() => BinaryPrimitives.ReadInt16LittleEndian( (await InternalReadAsync( 2 )).Span );

        public virtual async ValueTask<ushort> ReadUInt16Async() => BinaryPrimitives.ReadUInt16LittleEndian( (await InternalReadAsync( 2 )).Span );

        public virtual async ValueTask<int> ReadInt32Async() => BinaryPrimitives.ReadInt32LittleEndian( (await InternalReadAsync( 4 )).Span );
        public virtual async ValueTask<uint> ReadUInt32Async() => BinaryPrimitives.ReadUInt32LittleEndian( (await InternalReadAsync( 4 )).Span );
        public virtual async ValueTask<long> ReadInt64Async() => BinaryPrimitives.ReadInt64LittleEndian( (await InternalReadAsync( 8 )).Span );
        public virtual async ValueTask<ulong> ReadUInt64Async() => BinaryPrimitives.ReadUInt64LittleEndian( (await InternalReadAsync( 8 )).Span );
        public virtual async ValueTask<float> ReadSingleAsync() => BitConverter.Int32BitsToSingle( BinaryPrimitives.ReadInt32LittleEndian( (await InternalReadAsync( 4 )).Span ) );
        public virtual async ValueTask<double> ReadDoubleAsync() => BitConverter.Int64BitsToDouble( BinaryPrimitives.ReadInt64LittleEndian( (await InternalReadAsync( 8 )).Span ) );


        public virtual Task<int> ReadAsync( byte[] buffer, int index, int count )
        {
            if( buffer == null )
            {
                throw new ArgumentNullException( nameof( buffer ) );
            }
            if( index < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( index ) );
            }
            if( count < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( count ) );
            }
            if( buffer.Length - index < count )
            {
                throw new ArgumentException();
            }
            ThrowIfDisposed();
            return _stream.ReadAsync( buffer, index, count );
        }

        public virtual ValueTask<int> Read( Memory<byte> buffer )
        {
            ThrowIfDisposed();
            return _stream.ReadAsync( buffer );
        }
        public virtual async ValueTask<Memory<byte>> ReadBytesAsync( int count )
        {
            if( count < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( count ) );
            }
            ThrowIfDisposed();

            if( count == 0 )
            {
                return Array.Empty<byte>();
            }

            if( count <= _buffer.Length )
            {
                return await InternalReadAsync( count );
            }

            var result = new Memory<byte>(new byte[count]);
            int numRead = 0;
            do
            {
                int n = await _stream.ReadAsync( result[numRead..result.Length] );
                if( n == 0 ) break;
                numRead += n;
                count -= n;
            } while( count > 0 );

            if( numRead + count != result.Length )
            {
                // Trim array.  This should happen on EOF & possibly net streams.
                return result[0..numRead];
            }
            return result;
        }



        async ValueTask<Memory<byte>> InternalReadAsync( int numBytes )
        {
            Debug.Assert( numBytes <= _buffer.Length );
            ThrowIfDisposed();

            int bytesRead = 0;
            do
            {
                int n = await _stream.ReadAsync( _buffer[bytesRead..numBytes] );
                if( n == 0 ) throw new EndOfStreamException();
                bytesRead += n;
            } while( bytesRead < numBytes );
            Debug.Assert( bytesRead == numBytes );
            return _buffer[0..numBytes];
        }

        // FillBuffer is not performing well when reading from MemoryStreams as it is using the public Stream interface.
        // We introduced new function InternalRead which can work directly on the MemoryStream internal buffer or using the public Stream
        // interface when working with all other streams. This function is not needed anymore but we decided not to delete it for compatibility
        // reasons. More about the subject in: https://github.com/dotnet/coreclr/pull/22102
        protected virtual async ValueTask FillBufferAsync( int numBytes )
        {
            if( numBytes < 0 || numBytes > _buffer.Length )
            {
                throw new ArgumentOutOfRangeException( nameof( numBytes ) );
            }

            int bytesRead = 0;
            ThrowIfDisposed();

            do
            {
                int n = await _stream.ReadAsync( _buffer[Range.StartAt( bytesRead )] );
                if( n == 0 ) throw new EndOfStreamException();
                bytesRead += n;
            } while( bytesRead < numBytes );
        }

        public async ValueTask DisposeAsync()
        {
            if( !_disposed )
            {
                if( !_leaveOpen )
                {
                    await _stream.DisposeAsync();
                }
                _disposed = true;
            }
        }
    }
}
