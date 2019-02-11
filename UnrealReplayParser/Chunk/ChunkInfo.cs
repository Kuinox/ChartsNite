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
        /// <summary>
        /// Hold information about the Chunk, please read <see cref="SubStream"/>
        /// </summary>
        /// <param name="chunkType"></param>
        /// <param name="sizeInBytes"></param>
        /// <param name="stream"></param>
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
            Stream = info.Stream;
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
