using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class CustomDisposeStream : Stream
    {
        readonly Stream _baseStream;
        readonly Action _dispose;

        public Stream BaseStream => _baseStream;

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanTimeout => _baseStream.CanTimeout;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }
        public override int ReadTimeout { get => _baseStream.ReadTimeout; set => _baseStream.ReadTimeout = value; }
        public override int WriteTimeout { get => _baseStream.WriteTimeout; set => _baseStream.WriteTimeout = value; }

        public CustomDisposeStream( Stream baseStream, Action dispose )
        {
            _baseStream = baseStream;
            _dispose = dispose;
        }

        public override bool Equals( object obj )
        {
            return _baseStream.Equals( obj );
        }

        public override int GetHashCode()
        {
            return _baseStream.GetHashCode();
        }

        public override string ToString()
        {
            return _baseStream.ToString();
        }

        public override object InitializeLifetimeService()
        {
            return _baseStream.InitializeLifetimeService();
        }

        public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return _baseStream.BeginRead( buffer, offset, count, callback, state );
        }

        public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return _baseStream.BeginWrite( buffer, offset, count, callback, state );
        }

        public override void Close()
        {
            _baseStream.Close();
        }

        public override void CopyTo( Stream destination, int bufferSize )
        {
            _baseStream.CopyTo( destination, bufferSize );
        }

        public override Task CopyToAsync( Stream destination, int bufferSize, CancellationToken cancellationToken )
        {
            return _baseStream.CopyToAsync( destination, bufferSize, cancellationToken );
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                _baseStream.Dispose();
                _dispose();
            }
        }

        public override ValueTask DisposeAsync()
        {
            return _baseStream.DisposeAsync();
        }

        public override int EndRead( IAsyncResult asyncResult )
        {
            return _baseStream.EndRead( asyncResult );
        }

        public override void EndWrite( IAsyncResult asyncResult )
        {
            _baseStream.EndWrite( asyncResult );
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override Task FlushAsync( CancellationToken cancellationToken )
        {
            return _baseStream.FlushAsync( cancellationToken );
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            return _baseStream.Read( buffer, offset, count );
        }

        public override int Read( Span<byte> buffer )
        {
            return _baseStream.Read( buffer );
        }

        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            return _baseStream.ReadAsync( buffer, offset, count, cancellationToken );
        }

        public override ValueTask<int> ReadAsync( Memory<byte> buffer, CancellationToken cancellationToken = default )
        {
            return _baseStream.ReadAsync( buffer, cancellationToken );
        }

        public override int ReadByte()
        {
            return _baseStream.ReadByte();
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            return _baseStream.Seek( offset, origin );
        }

        public override void SetLength( long value )
        {
            _baseStream.SetLength( value );
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            _baseStream.Write( buffer, offset, count );
        }

        public override void Write( ReadOnlySpan<byte> buffer )
        {
            _baseStream.Write( buffer );
        }

        public override Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            return _baseStream.WriteAsync( buffer, offset, count, cancellationToken );
        }

        public override ValueTask WriteAsync( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default )
        {
            return _baseStream.WriteAsync( buffer, cancellationToken );
        }

        public override void WriteByte( byte value )
        {
            _baseStream.WriteByte( value );
        }
    }
}
