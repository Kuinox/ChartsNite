using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.StreamHelpers;

namespace ReplayAnalyzer
{
    public class ChunkInfo : Stream
    {
        public ChunkType Type { get; }
        public readonly int SizeInBytes;
        public readonly long TypeOffset;
        public readonly long DataOffset;
        protected Stream Stream;
        public ChunkInfo(ChunkType chunkType, int sizeInBytes, long typeOffset, long dataOffset, Stream stream)
        {
            Type = chunkType;
            SizeInBytes = sizeInBytes;
            TypeOffset = typeOffset;
            DataOffset = dataOffset;
            Stream = stream;
        }

        public ChunkInfo(ChunkInfo info)
        {
            Type = info.Type;
            SizeInBytes = info.SizeInBytes;
            TypeOffset = info.TypeOffset;
            DataOffset = info.DataOffset;
            Stream = new SubStream(info.Stream, info.Stream.Length-info.Stream.Position, true);
        }

        public override void Flush() => Stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

        public override void SetLength(long value) => Stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => Stream.Write(buffer, offset, count);

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public sealed override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
           Stream.Dispose();
        }
    }

    public enum ChunkType : uint
    {
        Header,
        ReplayData,
        Checkpoint,
        Event,
        Unknown = 0xFFFFFFFF
    };
}
