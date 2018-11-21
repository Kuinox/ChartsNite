using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.StreamHelpers;

namespace ReplayAnalyzer
{
    public class ChunkInfo
    {
        public readonly uint Type;
        public readonly int SizeInBytes;
        public Stream Stream;
        public ChunkInfo(uint chunkType, int sizeInBytes, Stream stream)
        {
            Type = chunkType;
            SizeInBytes = sizeInBytes;
            Stream = stream;
        }

        public ChunkInfo(ChunkInfo info)
        {
            Type = info.Type;
            SizeInBytes = info.SizeInBytes;
            Stream = new SubStream(info.Stream, info.Stream.Length-info.Stream.Position, true);
        }

        protected void Dispose(bool disposing)
        {
           Stream.Dispose();
        }
    }
}
