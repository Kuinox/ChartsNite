using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class SubStream : Stream
    {
        public enum ForceState
        {
            Nothing,
            ForceAsync,
            ForceSync
        }

        readonly Stream _stream;
        readonly bool _leaveOpen;
        readonly ForceState _forceState;
        readonly long _startPosition;
        readonly long _maxPosition;
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
        internal SubStream( Stream stream, long length, bool leaveOpen = false, ForceState forceState = ForceState.Nothing )
        {
            if(!Enum.IsDefined(typeof(ForceState), forceState))
            {
                throw new ArgumentException( "Invalid Enum as argument." );
            }
            ///Even if some stream have the property CanSeek set to false, they may have their Length or Position available.
            ///In this case, i will use these properties to increase the safety.
            try //We test if the length is available. I have no other way to know if i can read the Length.
            {
                if( length > stream.Length )
                {
                    throw new InvalidOperationException( "Available length is smaller than asked length." );
                }
            }
            catch( NotSupportedException )
            {
                //We add extra safety if we can read stream.Length. If we can't, not a big deal and we can ignore safely.
            }

            try //We test if the Position is available. I have no other way to know if i can read the Position.
            {
                long temp = stream.Position;
                _isPositionAvailable = true;
            }
            catch( NotSupportedException )
            {
                _isPositionAvailable = false;
            }
            _stream = stream;
            _leaveOpen = leaveOpen;
            _forceState = forceState;
            Length = length;
            if( !_isPositionAvailable )//TODO: When the Position is not available, this may fail
            {
                return;
            }
            _startPosition = stream.Position;
            _maxPosition = _startPosition + length;
            Checks();
        }
        /// <summary>
        /// Make the Dispose not move the BaseStream Position
        /// </summary>
        public void CancelSelfRepositioning() => _dontFlush = true;

        public override void Flush()
        {
            Checks();
            _stream.Flush();
        }

        #region Reads    
        public override int Read( byte[] buffer, int offset, int count )
        {
            Checks();
            if(_forceState == ForceState.ForceAsync)
            {
                throw new InvalidOperationException( "You asked to do Async only." );
            }
            return ReadWithoutChecks( buffer, offset, count );
        }

        int ReadWithoutChecks( byte[] buffer, int offset, int count )
        {
            int toRead = count;
            if( count + Position > Length )
            {
                toRead = (int)(Length - Position);
            }
            int read = _stream.Read( buffer, offset, toRead );
            _relativePosition += read;
            return read;
        }

        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            Checks();
            if( _forceState == ForceState.ForceSync )
            {
                throw new InvalidOperationException( "You asked to do Sync only." );
            }
            return ReadWithoutChecksAsync( buffer, offset, count, cancellationToken );
        }

        async Task<int> ReadWithoutChecksAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            int toRead = count;
            if( count + Position > Length )
            {
                toRead = (int)(Length - Position);
            }
            int read = await _stream.ReadAsync( buffer, offset, toRead, cancellationToken );
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
                    pos = _stream.Seek( _maxPosition + offset, SeekOrigin.Begin );
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
            if( _forceState == ForceState.ForceSync )
            {
                throw new InvalidOperationException( "You asked to do Sync only." );
            }
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

            //https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.read
            int toSkip = TrySyncDispose();

            while( toSkip != 0 )
            {
                int read = await ReadWithoutChecksAsync( new byte[toSkip], 0, toSkip, default );
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
            if( _forceState == ForceState.ForceAsync )
            {
                throw new InvalidOperationException( "You asked to do Async only." );
            }
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
