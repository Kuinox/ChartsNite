using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.StreamHelpers
{
    public class DebugStream : Stream
    {
        readonly Stream _streamToDebug;

        public DebugStream(Stream streamToDebug)
        {
            _streamToDebug = streamToDebug;
        }

        public override void Flush()
        {
            _streamToDebug.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _streamToDebug.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _streamToDebug.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _streamToDebug.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _streamToDebug.Write(buffer, offset, count);
        }

        public override bool CanRead => _streamToDebug.CanRead;

        public override bool CanSeek => _streamToDebug.CanSeek;

        public override bool CanWrite => _streamToDebug.CanWrite;

        public override long Length => _streamToDebug.Length;

        public override long Position
        {
            get => _streamToDebug.Position;
            set => _streamToDebug.Position = value;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
