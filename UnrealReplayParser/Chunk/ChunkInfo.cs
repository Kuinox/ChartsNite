using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public class ChunkInfo
    {
        public readonly ChunkType ChunkType;
        public readonly int SizeInBytes;
        /// <summary>
        /// Hold information about the Chunk, please read <see cref="SubStream"/>
        /// </summary>
        /// <param name="chunkType"></param>
        /// <param name="sizeInBytes"></param>
        /// <param name="stream"></param>
        public ChunkInfo(ChunkType chunkType, int sizeInBytes)
        {
            ChunkType = chunkType;
            SizeInBytes = sizeInBytes;
        }

        protected ChunkInfo(ChunkInfo info)
        {
            ChunkType = info.ChunkType;
            SizeInBytes = info.SizeInBytes;
        }
    }
}
