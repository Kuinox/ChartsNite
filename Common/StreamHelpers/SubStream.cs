using System;
using System.Diagnostics;
using System.IO;

namespace Common.StreamHelpers
{
    public class SubStream : Stream
    {
        readonly Stream _stream;
        readonly bool _leaveOpen;
        readonly long _startPosition;
        readonly long _maxPosition;
        bool _disposed;
        public SubStream(Stream stream, long length, bool leaveOpen = false)
        {
            try
            {
                if (length > stream.Length) throw new InvalidOperationException();
            }
            catch (NotSupportedException e)
            {
                //What should i do ?
            }
            _stream = stream;
            _leaveOpen = leaveOpen;
            Length = length;
            _startPosition = stream.Position;
            _maxPosition = _startPosition + length;
        }

        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (Position + count > Length) throw new InvalidOperationException();
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)//TODO change seek long returned based on substream
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            switch (origin)
            {
                case SeekOrigin.Current:
                    if (offset + Position > Length || offset + Position < 0) throw new InvalidOperationException();
                    return _stream.Seek(offset, SeekOrigin.Current);
                case SeekOrigin.Begin:
                    if (offset < 0 || offset > Length) throw new InvalidOperationException();
                    return _stream.Seek(_startPosition + offset, SeekOrigin.Begin);
                case SeekOrigin.End:
                    if (Length + offset < 0 || Length + offset > Length) throw new InvalidOperationException();
                    return _stream.Seek(_maxPosition + offset, SeekOrigin.Begin);
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
            if (Position + count > Length) throw new InvalidOperationException();
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length { get; }

        public override long Position
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                return _stream.Position - _startPosition;
            }
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
                long posBefore = _stream.Position;
                long pos = Seek(Length, SeekOrigin.Begin);
                Console.WriteLine("Seek of" + (_stream.Position - posBefore) + " Length " + Length + "seekPos: " + pos);
            }
            else if (CanRead)
            {
                int toSkip = (int)(Length - Position);
                while (toSkip != 0)
                {
                    int read = Read(new byte[toSkip], 0, toSkip);
                    toSkip -= read;
                    if(read == 0) throw new InvalidDataException("End of stream when i should have skipped data.");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            Debug.Assert(Length == Position);
            if (!_leaveOpen) _stream.Dispose();
            _disposed = true;
        }
    }
}
