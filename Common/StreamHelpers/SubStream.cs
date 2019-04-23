using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class SubStream : Stream
    {
        readonly Stream _stream;
        readonly bool _leaveOpen;
        readonly long _startPosition;
        readonly bool _isPositionAvailable;
        bool _dontFlush;
        long _relativePosition;
        public bool Disposed { get; private set; }
        /// <summary>
        /// Represent a part of a Base <see cref="Stream"/>. Provide some safety features:
        /// Avoid OverReading(If you try to read to much, the Read methods will return you 0 like a <see cref="Stream"/> when you reach the EndOfStream)
        /// Watch Base Stream Position(If the BaseStream Position is available, each call to this object will check if the Position of the BaseStream have moved)
        /// 
        /// </summary>
        /// <param name="stream">Base <see cref="Stream"/></param>
        /// <param name="length">Length of this SubStream</param>
        /// <param name="leaveOpen">if i should leave open the Base <see cref="Stream"/></param>
        internal SubStream( Stream stream, long length, bool canReadLength, bool canReadPosition, bool leaveOpen = false )
        {
            Length = length;
            _stream = stream;
            _leaveOpen = leaveOpen;
            _isPositionAvailable = canReadPosition;
            if(_isPositionAvailable)
            {
                if( canReadLength && stream.Position + length > stream.Length)
                {
                    throw new InvalidOperationException( "The substream length is bigger than the base stream" );
                }
                _startPosition = stream.Position;

            }
            Checks();
        }
        /// <summary>
        /// The Dispose does nothing after this.
        /// It's usefull when you 
        /// </summary>
        public void CancelSelfRepositioning() => _dontFlush = true;

        public override void Flush()
        {
            Checks();
            _stream.Flush();
        }

        #region Reads

        int ComputeAmountToRead( int count )
        {
            return count + Position <= Length ? count : (int)(Length - Position);
        }
        public override int Read( byte[] buffer, int offset, int count )
        {
            Checks();
            return ReadWithoutChecks( buffer, offset, count );
        }

        int ReadWithoutChecks( byte[] buffer, int offset, int count )
        {
            int toRead = ComputeAmountToRead( count );
            int read = _stream.Read( buffer, offset, toRead );
            _relativePosition += read;
            return read;
        }


        int ReadWithoutChecks( Span<byte> buffer )
        {
            int toRead = ComputeAmountToRead( buffer.Length );
            int read = _stream.Read( buffer[Range.EndAt( toRead )] );
            _relativePosition += read;
            return read;
        }

        public override int Read( Span<byte> buffer )
        {
            Checks();
            return ReadWithoutChecks( buffer );
        }

        public override int ReadByte()
        {
            Checks();
            if( ComputeAmountToRead( 1 ) == 0 ) return 0;
            int read = _stream.ReadByte();
            _relativePosition += read;
            return read;
        }

        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            Checks();
            return ReadWithoutChecksAsync( buffer, offset, count, cancellationToken );
        }

        public override ValueTask<int> ReadAsync( Memory<byte> buffer, CancellationToken cancellationToken = default )
        {
            Checks();
            return ReadWithoutChecksAsync( buffer, cancellationToken );
        }

        async Task<int> ReadWithoutChecksAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            int toRead = count + Position <= Length ? count : (int)(Length - Position);//if he ask to read too much bytes, we set toRead to the number of remaining bytes.
            int read = await _stream.ReadAsync( buffer, offset, toRead, cancellationToken );
            _relativePosition += read;
            return read;
        }

        async ValueTask<int> ReadWithoutChecksAsync( Memory<byte> memory, CancellationToken cancellationToken )
        {
            int toRead = memory.Length + Position <= Length ? memory.Length : (int)(Length - Position);
            int read = await _stream.ReadAsync( memory[Range.EndAt( toRead )], cancellationToken );
            _relativePosition += read;
            return read;
        }


        #endregion Reads

        public override long Seek( long offset, SeekOrigin origin )
        {
            Checks();
            return SeekWithoutChecks( offset, origin );
        }

        long SeekWithoutChecks( long offset, SeekOrigin origin )
        {
            long pos;
            switch( origin )
            {
                case SeekOrigin.Current:
                    if( offset + Position > Length || offset + Position < 0 )
                    {
                        throw new InvalidOperationException();
                    }
                    pos = _stream.Seek( offset, SeekOrigin.Current );
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                case SeekOrigin.Begin:
                    if( offset < 0 || offset > Length )
                    {
                        throw new InvalidOperationException();
                    }
                    pos = _stream.Seek( _startPosition + offset, SeekOrigin.Begin );
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                case SeekOrigin.End:
                    if( Length + offset < 0 || Length + offset > Length )
                    {
                        throw new InvalidOperationException();
                    }
                    pos = _stream.Seek( _startPosition + Length + offset, SeekOrigin.Begin );
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                default:
                    throw new NotSupportedException();
            }
        }


        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            Checks();
            _stream.Write( buffer, offset, count );
            _relativePosition += count;
            throw new NotImplementedException( "TODO." );
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        /// <summary>
        /// Check if the BaseStream position without calling the SubStream. Work only if you provied a Stream where we can get the Position
        /// </summary>
        /// <param name="ignoreDispose"></param>
        void Checks()
        {
            if( Disposed )
            {
                throw new ObjectDisposedException( GetType().Name );
            }
            if( _isPositionAvailable && _startPosition + _relativePosition != _stream.Position )
            {
                throw new InvalidOperationException( "Upper stream Position changed" );
            }
        }

        public override long Position
        {
            get
            {
                Checks();
                return _relativePosition;
            }
            set
            {
                Checks();
                _stream.Position = value + _startPosition;
            }
        }
        public override async ValueTask DisposeAsync()
        {
            if( Disposed )
            {
                return;
            }
            if( !_leaveOpen || _dontFlush )
            {
                await _stream.DisposeAsync();
                Disposed = true;
                return;
            }

            int toSkip = TrySyncDispose();
            while( toSkip != 0 )//We can't seek, so we 'burn' all the bytes to move the cursor to the position
            {
                int read = await ReadWithoutChecksAsync( new byte[toSkip], default );
                toSkip -= read;
                if( read == 0 )
                {
                    throw new EndOfStreamException( "Unexpected End of Stream." );
                }
            }
            Disposed = true;
        }
        /// <summary>
        /// Try to Dispose synchronously. If we can't Seek the cursor to the right position we return the amount of byte to move forward.
        /// </summary>
        /// <returns>the amount of byte to move forward</returns>
        int TrySyncDispose()
        {
            if( !CanRead && !CanSeek )
            {
                throw new NotImplementedException(); // we can't do this. so we throw an exception synchronously
            }
            int toSkip = (int)(Length - _relativePosition);//Dont use Position, because it check if the object is Disposed.
            if( toSkip == 0 )
            { //we have nothing to skip
                Disposed = true;
                return 0;
            }
            if( CanSeek )
            {
                SeekWithoutChecks( Length, SeekOrigin.Begin );
                Disposed = true;
                return 0;
            }
            return toSkip;
        }

        protected override void Dispose( bool disposing )
        {
            int toSkip = TrySyncDispose();
            while( toSkip != 0 )
            {
                int read = ReadWithoutChecks( new byte[toSkip], 0, toSkip );
                toSkip -= read;
                if( read == 0 )
                {
                    throw new EndOfStreamException( "Unexpected End of Stream." );
                }
            }
            Disposed = true;
        }
    }
}
