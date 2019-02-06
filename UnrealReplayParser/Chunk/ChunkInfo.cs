using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.StreamHelpers;

namespace UnrealReplayParser
{
    public class ChunkInfo : IDisposable
    {
        public readonly uint Type;
        public readonly int SizeInBytes;
        public readonly Stream Stream;
        public ChunkInfo(uint chunkType, int sizeInBytes, SubStream stream)
        {
            Type = chunkType;
            SizeInBytes = sizeInBytes;
            Stream = stream;
        }

        protected ChunkInfo(ChunkInfo info)
        {
            Type = info.Type;
            SizeInBytes = info.SizeInBytes;
            Stream = new SubStream(info.Stream, info.Stream.Length-info.Stream.Position, true);
        }

        protected void Dispose(bool disposing)
        {
           Stream.Dispose();
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
