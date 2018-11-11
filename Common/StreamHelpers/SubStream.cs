using System;
using System.IO;

namespace Common.StreamHelpers
{
    public class SubStream : Stream
    {
        readonly Stream _stream;
        readonly bool _disposeParent;
        readonly long _startPosition;
        long _subStreamPosition;
        bool _disposed;
        public SubStream(Stream stream, long length, bool disposeParent = false) //TODO: No seek, no length
        {
            if(length > stream.Length) throw new InvalidOperationException();
            _stream = stream;
            _disposeParent = disposeParent;
            Length = length;
            _startPosition = stream.Position;
            _subStreamPosition = 0;
        }

        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if( Position + offset < 0 ) throw new InvalidOperationException();
            int countToRead = count;
            if (count + offset > Length)
            {
                countToRead = (int)(Length - Position);
            }
            int actuallyRead = _stream.Read(buffer, offset, countToRead);
            _subStreamPosition += actuallyRead;
            return actuallyRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            switch (origin)
            {
                case SeekOrigin.Current:
                    if (_subStreamPosition+offset > Length) throw new InvalidOperationException();
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
            if (_subStreamPosition+count>Length) throw new InvalidOperationException();
            _stream.Write(buffer, offset, count);
            _subStreamPosition += count;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        public override long Position
        {
            get => _subStreamPosition;
            set
            {
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                _stream.Position = value + _startPosition;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (CanSeek)
            {
                Seek(Length, SeekOrigin.Begin);
            }
            else if(CanRead)
            {
                int toSkip = (int)(Length - _subStreamPosition);
                Read(new byte[toSkip], 0, toSkip);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (_disposeParent) _stream.Dispose();
            _disposed = true;
        }
    }
}
