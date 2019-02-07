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
        readonly long _maxPosition;
        readonly bool _isPositionAvailable;
        long _relativePosition;
        internal SubStream(Stream stream, long length, bool leaveOpen = false)
        {
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
            CheckPositionUpperStream();
        }

        public override void Flush()
        {
            if (Disposed) throw new ObjectDisposedException(GetType().Name);
            CheckPositionUpperStream();
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Disposed) throw new ObjectDisposedException(GetType().Name);
            CheckPositionUpperStream();
            int toRead = count;
            if (count + Position > Length)
            {
                toRead = (int) (Length - Position);
            }
            int read = _stream.Read(buffer, offset, toRead);
            _relativePosition += read;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Disposed) throw new ObjectDisposedException(GetType().Name);
            CheckPositionUpperStream();
            int toRead = count;
            if (count + Position > Length)
            {
                toRead = (int) (Length - Position);
            }
            int read = await _stream.ReadAsync(buffer, offset, toRead, cancellationToken);
            _relativePosition += read;
            return read;

        }

        public override long Seek(long offset, SeekOrigin origin)//TODO change seek long returned based on substream
        {
            if (Disposed) throw new ObjectDisposedException(GetType().Name);
            CheckPositionUpperStream();
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
            if (Disposed) throw new ObjectDisposedException(GetType().Name);
            if (_isPositionAvailable && Position + count > Length) throw new InvalidOperationException();
            _stream.Write(buffer, offset, count);
            _relativePosition += count;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        void CheckPositionUpperStream()
        {
            if (_isPositionAvailable && _startPosition + _relativePosition != _stream.Position) throw new InvalidOperationException("Upper stream Position changed");
        }

        public override long Position
        {
            get
            {
                if (Disposed) throw new ObjectDisposedException(GetType().Name);
                CheckPositionUpperStream();
                return _relativePosition;
            }
            set
            {
                if (Disposed) throw new ObjectDisposedException(GetType().Name);
                CheckPositionUpperStream();
                _stream.Position = value + _startPosition;
            }
        }

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;
            CheckPositionUpperStream();
            if (CanSeek)
            {
                Seek(Length, SeekOrigin.Begin);
            }
            else
            if (CanRead)
            {
                int toSkip = (int)(Length - Position);
                while (toSkip != 0)
                {
                    int read = Read(new byte[toSkip], 0, toSkip);
                    toSkip -= read;
                    if (read == 0) throw new InvalidDataException("End of stream when i should have skipped data.");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            Debug.Assert(Length == Position);
            if (!_leaveOpen) _stream.Dispose();
            Disposed = true;
        }
    }
}
