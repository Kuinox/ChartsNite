using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{

    public class DebugStream : Stream
    {

        public enum AllowedMethods
        {
            Sync =                  1 << 0,
            Async =                 1 << 1,
            Seek =                  1 << 3,
            SetLength =             1 << 4,
            Obselete =              1 << 5,
            ArrayBuffer =           1 << 6,
            SpanOrMemoryBuffer =    1 << 7,
            PositionRead =          1 << 8,
            LengthRead =            1 << 9,
            BeginOrEnd =            1 << 10,
            CopyTo =                1 << 11,
            MustDispose =           1 << 12,
            MustDisposeAsync =      1 << 13,
            AsyncMemory = SpanOrMemoryBuffer | Async,
            SyncSpan = SpanOrMemoryBuffer | Sync
        }

        readonly Stream _streamToDebug;
        private readonly AllowedMethods _allowedMethods;

        public DebugStream( Stream streamToDebug, AllowedMethods methods )
        {
            _streamToDebug = streamToDebug;
            _allowedMethods = methods;
        }
        bool _disposed;
        protected override void Dispose( bool disposing )
        {
            _disposed = true;
            _streamToDebug.Dispose();
        }

        bool _asyncDisposed;
        public override ValueTask DisposeAsync()
        {
            _asyncDisposed = true;
            return _streamToDebug.DisposeAsync();
        }

        ~DebugStream()
        {
            if(!_disposed && (_allowedMethods & AllowedMethods.MustDispose) > 0)
            {
                throw new InvalidOperationException( "You didn't Dispose the Stream" );
            }
            if( !_asyncDisposed && (_allowedMethods & AllowedMethods.MustDisposeAsync) > 0 )
            {
                throw new InvalidOperationException( "You didn't DisposeAsync the Stream" );
            }
        }

        void ThrowIfMethodNotAllowed( AllowedMethods methodType )
        {
#if DEBUG
            if( (methodType & _allowedMethods) == 0 )
            {
                throw new NotSupportedException( "A method of type not allowed was called" );
            }
#endif
        }

        /// <summary>
        /// The overload only call the base method, it's here if you want to breakpoint it.
        /// </summary>
        public override void Flush()
        {
            _streamToDebug.Flush();
        }

        /// <summary>
        /// The overload only call the base method, it's here if you want to breakpoint it.
        /// </summary>
        public override bool CanRead => _streamToDebug.CanRead;

        /// <summary>
        /// The overload only call the base method, it's here if you want to breakpoint it.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// The overload only call the base method, it's here if you want to breakpoint it.
        /// </summary>
        public override bool CanWrite => _streamToDebug.CanWrite;

        public override long Length
        {
            get
            {
                ThrowIfMethodNotAllowed( AllowedMethods.LengthRead );
                return _streamToDebug.Length;
            }
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync | AllowedMethods.ArrayBuffer );
            return _streamToDebug.Read( buffer, offset, count );
        }
        public override void Write( byte[] buffer, int offset, int count )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync | AllowedMethods.ArrayBuffer );
            _streamToDebug.Write( buffer, offset, count );
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Seek );
            return _streamToDebug.Seek( offset, origin );
        }

        public override void SetLength( long value )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.SetLength );
            _streamToDebug.SetLength( value );
        }

        public override long Position
        {
            get
            {
                ThrowIfMethodNotAllowed( AllowedMethods.PositionRead );
                return _streamToDebug.Position;
            }
            set
            {
                ThrowIfMethodNotAllowed( AllowedMethods.Seek );
                _streamToDebug.Position = value;
            }
        }

        public override IAsyncResult BeginRead( byte[] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.BeginOrEnd );
            return _streamToDebug.BeginRead( buffer, offset, count, callback, state );
        }

        public override IAsyncResult BeginWrite( byte[] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.BeginOrEnd );
            return _streamToDebug.BeginWrite( buffer, offset, count, callback, state );
        }

        public override bool CanTimeout => base.CanTimeout;

        public override Task CopyToAsync( Stream destination, int bufferSize, CancellationToken cancellationToken )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.CopyTo | AllowedMethods.Async);
            return _streamToDebug.CopyToAsync( destination, bufferSize, cancellationToken );
        }


        public override int EndRead( IAsyncResult asyncResult )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.BeginOrEnd );
            return _streamToDebug.EndRead( asyncResult );
        }

        public override void EndWrite( IAsyncResult asyncResult )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.BeginOrEnd );
            _streamToDebug.EndWrite( asyncResult );
        }


        public override Task FlushAsync( CancellationToken cancellationToken )
        {
            return _streamToDebug.FlushAsync( cancellationToken );
        }

        public override int Read( Span<byte> buffer )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync | AllowedMethods.SpanOrMemoryBuffer );
            return _streamToDebug.Read( buffer );
        }

        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Async | AllowedMethods.ArrayBuffer );
            return _streamToDebug.ReadAsync( buffer, offset, count, cancellationToken );
        }

        public override ValueTask<int> ReadAsync( Memory<byte> buffer, CancellationToken cancellationToken = default )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Async | AllowedMethods.SpanOrMemoryBuffer );
            return _streamToDebug.ReadAsync( buffer, cancellationToken );
        }

        public override int ReadByte()
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync );
            return _streamToDebug.ReadByte();
        }
 
        public override void Write( ReadOnlySpan<byte> buffer )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync | AllowedMethods.SpanOrMemoryBuffer );
            _streamToDebug.Write( buffer );
        }

        public override Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Async | AllowedMethods.ArrayBuffer );
            return _streamToDebug.WriteAsync( buffer, offset, count, cancellationToken );
        }

        public override ValueTask WriteAsync( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Async | AllowedMethods.SpanOrMemoryBuffer );
            return _streamToDebug.WriteAsync( buffer, cancellationToken );
        }

        public override void WriteByte( byte value )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync );
            _streamToDebug.WriteByte( value );
        }

        public override void CopyTo( Stream destination, int bufferSize )
        {
            ThrowIfMethodNotAllowed( AllowedMethods.Sync | AllowedMethods.CopyTo );
            _streamToDebug.CopyTo( destination, bufferSize );
        }
    }
}
