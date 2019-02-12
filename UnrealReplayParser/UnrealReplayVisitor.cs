using System;
using System.IO;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayParser.Chunk;

namespace UnrealReplayParser
{
    public class UnrealReplayVisitor : IDisposable
    {
        const uint FileMagic = 0x1CA2E27F;

        protected readonly SubStreamFactory SubStreamFactory;
        public ReplayInfo Info { get; }
        protected UnrealReplayVisitor(ReplayInfo info, SubStreamFactory subStreamFactory)
        {
            Info = info;
            SubStreamFactory = subStreamFactory;
        }

        public static async Task<UnrealReplayVisitor> FromStream(SubStreamFactory subStreamFactory)
        {
            ReplayInfo replayInfo;
            using (BinaryReaderAsync binaryReader = new BinaryReaderAsync(subStreamFactory.BaseStream, true))
            {
                if (FileMagic != await binaryReader.ReadUInt32())
                {
                    throw new InvalidDataException("Invalid file. Probably not an Unreal Replay.");
                }
                uint fileVersion = await binaryReader.ReadUInt32();
                int lengthInMs = await binaryReader.ReadInt32();
                uint networkVersion = await binaryReader.ReadUInt32();
                uint changelist = await binaryReader.ReadUInt32();
                string friendlyName = await binaryReader.ReadString();
                bool bIsLive = await binaryReader.ReadUInt32() != 0;
                DateTime timestamp = DateTime.MinValue;
                if (fileVersion >= (uint)VersionHistory.HISTORY_RECORDED_TIMESTAMP)
                {
                    timestamp = DateTime.FromBinary(await binaryReader.ReadInt64());
                }
                bool bCompressed = false;
                if (fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION)
                {
                    bCompressed = await binaryReader.ReadUInt32() != 0;
                }
                if (binaryReader.IsError) throw new InvalidDataException(binaryReader.ErrorMessage);
                replayInfo = new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion);
            }
            return new UnrealReplayVisitor(replayInfo, subStreamFactory);
        }

        public virtual async Task<bool> Visit()
        {
            while (await VisitChunk()) ;
            return true;
        }

        public virtual async Task<bool> VisitChunk()
        {
            ChunkInfo chunk;
            bool headerFatal = false;
            bool headerError = false;
            using (Stream chunkHeader = await SubStreamFactory.Create(8, true))
            using (BinaryReaderAsync chunkHeaderBinaryReader = new BinaryReaderAsync(chunkHeader))
            {
                uint chunkType = await chunkHeaderBinaryReader.ReadUInt32();
                int sizeInBytes = await chunkHeaderBinaryReader.ReadInt32();
                chunk = new ChunkInfo(chunkType, sizeInBytes);
                if (chunkHeaderBinaryReader.Fatal)
                {
                    headerFatal = true;
                }
                if (chunkHeaderBinaryReader.IsError)
                {
                    headerError = true;
                }
            }
            if (headerFatal)
            {
                return await VisitIncompleteChunkHead(chunk);
            }
            if (headerError)
            {
                return await VisitInvalidChunk(SubStreamFactory.BaseStream, chunk);
            }

            using (SubStream subStream = await SubStreamFactory.Create(chunk.SizeInBytes, true))
            using (BinaryReaderAsync binaryReader = new BinaryReaderAsync(subStream))
            {
                bool validType = true;
                bool succeed = (ChunkType)chunk.Type switch
                {
                ChunkType.Header => await VisitHeaderChunk(binaryReader, chunk),
                ChunkType.Checkpoint => await VisitEventChunk(binaryReader, chunk, true),
                ChunkType.Event => await VisitEventChunk(binaryReader, chunk, false),
                ChunkType.ReplayData => await VisitReplayDataChunk(binaryReader, chunk),
                _ => validType = false
                };
                if(validType) return succeed;
                subStream.CancelFlush();
            }
            return await VisitInvalidChunk(SubStreamFactory.BaseStream, chunk);
        }

        /// <summary>
        /// Simply return true and does nothing else.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual Task<bool> VisitHeaderChunk(BinaryReaderAsync binaryReader, ChunkInfo chunk)
        {
            return Task.FromResult(true);
        }

        public virtual async Task<bool> VisitEventChunk(BinaryReaderAsync binaryReader, ChunkInfo chunk, bool isCheckpoint)
        {
            string id = await binaryReader.ReadString();
            string group = await binaryReader.ReadString();
            string metadata = await binaryReader.ReadString();
            uint time1 = await binaryReader.ReadUInt32();
            uint time2 = await binaryReader.ReadUInt32();
            int eventSizeInBytes = await binaryReader.ReadInt32();
            return !binaryReader.IsError && await VisitEventChunkContent(binaryReader, new EventInfo(chunk, id, group, metadata, time1, time2, eventSizeInBytes, isCheckpoint));
        }
        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public virtual Task<bool> VisitEventChunkContent(BinaryReaderAsync binaryReader, EventInfo eventInfo)
        {
            return Task.FromResult(true);
        }

        public virtual async Task<bool> VisitReplayDataChunk(BinaryReaderAsync binaryReader, ChunkInfo chunk)
        {
            int replaySizeInBytes;
            uint time1 = uint.MaxValue;
            uint time2 = uint.MaxValue;
            if (Info.FileVersion >= (uint)VersionHistory.HISTORY_STREAM_CHUNK_TIMES)
            {
                time1 = await binaryReader.ReadUInt32();
                time2 = await binaryReader.ReadUInt32();
                replaySizeInBytes = await binaryReader.ReadInt32();
            }
            else
            {
                replaySizeInBytes = chunk.SizeInBytes;
            }
            return !binaryReader.IsError && await VisitReplayDataChunkContent(binaryReader, new ReplayDataInfo(time1, time2, replaySizeInBytes, chunk));
        }

        public virtual Task<bool> VisitReplayDataChunkContent(BinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// The replay finished abruptly. Worst than that, it finished at the beginning of a chunk.
        /// </summary>
        /// <param name="chunkType">May have a bad value</param>
        /// <param name="sizeInBytes">May have a bad value</param>
        /// <returns>Always false.</returns>
        public virtual Task<bool> VisitIncompleteChunkHead(ChunkInfo chunk)
        {
            return Task.FromResult(false);
        }
        /// <summary>
        /// Does nothing, but you can try to recover from a corrupted/unsupported file here.
        /// Usually we can't read further, because we don't know the length of the actual block.
        /// You can try to detect the next block so the Visitor can finish his job properly
        /// </summary>
        /// <param name="subStreamFactory"></param>
        /// <param name="chunkType"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns>if the Visitor can safely continue is job, return always false if not overriden</returns>
        public virtual Task<bool> VisitInvalidChunk(Stream replayStream, ChunkInfo chunk)
        {
            return Task.FromResult(false);
        }
        public void Dispose()
        {
            SubStreamFactory.Dispose();
        }
    }
}
