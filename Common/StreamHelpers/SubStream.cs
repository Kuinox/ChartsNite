using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class SubStream : Stream
    {
        bool _asyncDisposed;
        readonly SemaphoreSlim _disposingOrCheckingSemaphore;
        readonly Stream _stream;
        readonly bool _leaveOpen;
        readonly long _startPosition;
        readonly long _maxPosition;
        readonly bool _isPositionAvailable;
        bool _dontFlush;
        long _relativePosition;
        /// <summary>
        /// Please read the stream to end or use <see cref="DisposeAsync"/> if you want to be fully <see langword="async"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="leaveOpen"></param>
        internal SubStream(Stream stream, long length, bool leaveOpen = false)
        {
            _disposingOrCheckingSemaphore = new SemaphoreSlim(1);
            bool isLengthAvailable;
            try
            {
                long temp = stream.Length;
                isLengthAvailable = true;
            }
            catch (NotSupportedException)
            {
                isLengthAvailable = false;
            }
            if (isLengthAvailable && length > stream.Length) throw new InvalidOperationException();
            try
            {
                long temp = stream.Position;
                _isPositionAvailable = true;
            }
            catch (NotSupportedException)
            {
                _isPositionAvailable = false;
            }
            _stream = stream;
            _leaveOpen = leaveOpen;
            Length = length;
            if (!_isPositionAvailable) return;
            _startPosition = stream.Position;
            _maxPosition = _startPosition + length;
            Checks();
        }
        /// <summary>
        /// Notify to skip all the remaining bytes when Disposing.
        /// </summary>
        public void CancelFlush() => _dontFlush = true;

        public override void Flush()
        {
            Checks();
            _stream.Flush();
        }

        #region Reads    
        public override int Read(byte[] buffer, int offset, int count)
        {
            Checks();
            return ReadWithoutChecks(buffer, offset, count);
        }

        int ReadWithoutChecks(byte[] buffer, int offset, int count)
        {
            int toRead = count;
            if (count + Position > Length)
            {
                toRead = (int)(Length - Position);
            }
            int read = _stream.Read(buffer, offset, toRead);
            _relativePosition += read;
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Checks();
            return ReadWithoutChecksAsync(buffer, offset, count, cancellationToken);
        }

        async Task<int> ReadWithoutChecksAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int toRead = count;
            if (count + Position > Length)
            {
                toRead = (int)(Length - Position);
            }
            int read = await _stream.ReadAsync(buffer, offset, toRead, cancellationToken);
            _relativePosition += read;
            return read;
        }
        #endregion Reads

        public override long Seek(long offset, SeekOrigin origin)
        {
            Checks();
            return SeekWithoutChecks(offset, origin);
        }

        long SeekWithoutChecks(long offset, SeekOrigin origin)
        {
            long pos;
            switch (origin)
            {
                case SeekOrigin.Current:
                    if (offset + Position > Length || offset + Position < 0) throw new InvalidOperationException();
                    pos = _stream.Seek(offset, SeekOrigin.Current);
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                case SeekOrigin.Begin:
                    if (offset < 0 || offset > Length) throw new InvalidOperationException();
                    pos = _stream.Seek(_startPosition + offset, SeekOrigin.Begin);
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                case SeekOrigin.End:
                    if (Length + offset < 0 || Length + offset > Length) throw new InvalidOperationException();
                    pos = _stream.Seek(_maxPosition + offset, SeekOrigin.Begin);
                    _relativePosition = pos - _startPosition;
                    return _relativePosition;
                default:
                    throw new NotSupportedException();
            }
        }


        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Checks();
            _stream.Write(buffer, offset, count);
            _relativePosition += count;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        /// <summary>
        /// Check if the BaseStream position without calling the SubStream. Work only if you provied a Stream where we can get the Position
        /// </summary>
        /// <param name="ignoreDispose"></param>
        void Checks(bool ignoreDispose = false)
        {
            if (!ignoreDispose && Disposed) throw new ObjectDisposedException(GetType().Name);
            if (!ignoreDispose && _asyncDisposed) throw new ObjectDisposedException(GetType().Name);
            if ( _isPositionAvailable && _startPosition + _relativePosition != _stream.Position) throw new InvalidOperationException("Upper stream Position changed");
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
        /// <summary>
        /// Run a DisposeAsync, when completed the next call to<see cref="SubStreamFactory.Create(long, bool)"/> will return immediatly.
        /// </summary>
        /// <returns></returns>
        public async Task DisposeAsync()
        {
            if (!Disposed) throw new InvalidOperationException("Dispose should had be called");
            if (_asyncDisposed || _dontFlush) return;//it mean we already "disposed" the stream, so we return this synchrounously
            if (!_leaveOpen) return; //if we dispose parent stream there is no point to read the rest of the stream.
            if (!CanRead && !CanSeek) throw new NotImplementedException(); // we can't do this. so we throw an exception synchronously
            await _disposingOrCheckingSemaphore.WaitAsync();
            int toSkip = (int)(Length - _relativePosition);
            if (toSkip == 0 || _asyncDisposed) { //we have nothing to skip, or we have awaited another Task that was "disposing"
                SafeExit();
                return;
            }
            if (CanSeek)
            {
                SeekWithoutChecks(Length, SeekOrigin.Begin);
                SafeExit();
                return;//yay ! we could do everything synchronously
            }
            toSkip = (int)(Length - Position); //Read this first: https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.read
            while (toSkip != 0)
            {
                int read = await ReadWithoutChecksAsync(new byte[toSkip], 0, toSkip, default);
                toSkip -= read;
                if (read == 0) throw new EndOfStreamException("Unexpected End of Stream.");
            }
            SafeExit();

            void SafeExit()
            {
                _asyncDisposed = true;
                _disposingOrCheckingSemaphore.Release();
            }

        }

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;
            Checks();
            Disposed = true;
            Task.Run(DisposeAsync);
            if (!_leaveOpen) _stream.Dispose();
            
        }
    }
}
