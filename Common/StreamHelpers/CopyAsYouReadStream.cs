using System;
using System.IO;

namespace Common.StreamHelpers
{
    public class CopyAsYouReadStream : Stream
    {
        readonly Stream _streamToRead;
        readonly Stream _streamToWrite;

        public CopyAsYouReadStream(Stream streamToRead, Stream streamToWrite)
        {
            if (!streamToRead.CanRead || !streamToWrite.CanWrite) throw new ArgumentException();
            _streamToRead = streamToRead;
            _streamToWrite = streamToWrite;
        }

        public override void Flush()
        {
            _streamToRead.Flush();
            _streamToWrite.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int numberOfByteRead = _streamToRead.Read(buffer, offset, count);
            _streamToWrite.Write(buffer, offset, count);
            return numberOfByteRead;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _streamToRead.Length;

        public override long Position
        {
            get => _streamToRead.Position;
            set => throw new NotSupportedException();
        }
    }
}
