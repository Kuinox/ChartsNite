using System;
using System.IO;

namespace ReplayAnalyzer
{
    class SubStream : Stream
    {
        readonly Stream _stream;
        readonly bool _disposeParent;
        readonly long _startPosition;
        bool _disposed;
        public SubStream(Stream stream, long length, bool disposeParent = false)
        {
            _stream = stream;
            _disposeParent = disposeParent;
            Length = length;
            _startPosition = stream.Position;
        }

        bool IsValidAction(long offset, int count)
        {
            return Position + offset > 0 &&
                Position + offset + count <= Length;
        }   

        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if( Position+offset < 0 ) throw new InvalidOperationException();
            int countToRead = count;
            if (count + offset > Length)
            {
                countToRead = (int)(Length - Position);
            }
            return _stream.Read(buffer, offset, countToRead);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            switch (origin)
            {
                case SeekOrigin.Current:
                    if (!IsValidAction(offset, 0)) throw new InvalidOperationException();
                    return _stream.Seek(offset, SeekOrigin.Current);
                case SeekOrigin.Begin:
                    if(offset<0 || offset > Length) throw new InvalidOperationException();
                    return _stream.Seek(_startPosition + offset, SeekOrigin.Begin);
                case SeekOrigin.End:
                    if(Length+offset<0 || Length+offset>Length) throw new InvalidOperationException();
                    return _stream.Seek(_startPosition + Length + offset, SeekOrigin.Begin);
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
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (!IsValidAction(offset, count)) throw new InvalidOperationException();
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        public override long Position
        {
            get => _stream.Position - _startPosition;
            set
            {
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                _stream.Position = value + _startPosition;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            Seek(Length, SeekOrigin.Begin);
            if (_disposeParent) _stream.Dispose();
            _disposed = true;
        }
    }
}
