using System;
using System.IO;
using System.Threading.Tasks;
using Common.StreamHelpers;

namespace UnrealReplayParser
{
    public class UnrealReplayVisitor
    {
        const uint FileMagic = 0x1CA2E27F;

        protected readonly BinaryReaderAsync BinaryReader;
        readonly SubStreamFactory _subStreamFactory;
        public ReplayInfo Info { get; }
        protected UnrealReplayVisitor(ReplayInfo info, BinaryReaderAsync binaryReader)
        {
            Info = info;
            BinaryReader = binaryReader;
            _subStreamFactory = new SubStreamFactory(binaryReader.Stream);
        }

        public static async Task<UnrealReplayVisitor> FromStream(Stream stream)
        {
            BinaryReaderAsync binaryReader = new BinaryReaderAsync(stream);
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
            return new UnrealReplayVisitor(new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion), binaryReader);
        }

        public virtual async Task<bool> Visit()
        {
            while (await VisitChunk()) ;
            return true;
        }

        public virtual async Task<bool> VisitChunk()
        {
            uint chunkType = await BinaryReader.ReadUInt32();
            int sizeInBytes= await BinaryReader.ReadInt32();
            if (BinaryReader.IsError)
            {
                return await VisitIncompleteChunk(chunkType, sizeInBytes);
            }
            ChunkInfo chunk = new ChunkInfo(chunkType, sizeInBytes, await _subStreamFactory.Create(sizeInBytes, true));
            return (ChunkType)chunkType switch
            {
                ChunkType.Header => await VisitHeaderChunk(chunk),
                ChunkType.Checkpoint => await VisitEventChunk(chunk, true),
                ChunkType.Event => await VisitEventChunk(chunk, false),
                ChunkType.ReplayData => await VisitReplayDataChunk(chunk),
                _ => await VisitInvalidChunkType(chunkType)
            };
        }

        /// <summary>
        /// return <see langword="false"/> we do
        /// </summary>
        /// <param name="chunkType"></param>
        /// <returns><see langword="false"/></returns>
        public virtual Task<bool> VisitInvalidChunkType(uint chunkType)
        {
            return Task.FromResult(false);
        }
        /// <summary>
        /// Simply return true and does nothing else.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public virtual Task<bool> VisitHeaderChunk(ChunkInfo chunk)
        {
            return Task.FromResult(true);
        }

        public virtual async Task<bool> VisitEventChunk(ChunkInfo chunk, bool isCheckpoint)
        {
            (bool idSuccess, string id) = await chunk.Stream.TryReadString();
            (bool groupSuccess, string group) = await chunk.Stream.TryReadString();
            (bool metadataSuccess, string metadata) = await chunk.Stream.TryReadString();
            (bool time1Success, uint time1) = await chunk.Stream.TryReadUInt32();
            (bool time2Success, uint time2) = await chunk.Stream.TryReadUInt32();
            (bool eventSizeSuccess, int eventSizeInBytes) = await chunk.Stream.TryReadInt32();
            return idSuccess
                && groupSuccess
                && metadataSuccess
                && time1Success
                && time2Success
                && eventSizeSuccess
                && await VisitEventChunkContent(new EventInfo(chunk, id, group, metadata, time1, time2, eventSizeInBytes, isCheckpoint));
        }
        /// <summary>
        /// We do nothing there
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public virtual Task<bool> VisitEventChunkContent(EventInfo eventInfo)
        {
            return Task.FromResult(true);
        }

        public virtual async Task<bool> VisitReplayDataChunk(ChunkInfo chunk)
        {
            int replaySizeInBytes;
            bool sizeSuccess = false;
            uint time1 = uint.MaxValue;
            bool time1Success = false;
            uint time2 = uint.MaxValue;
            bool time2Success = false;
            if (Info.FileVersion >= (uint)VersionHistory.HISTORY_STREAM_CHUNK_TIMES)
            {
                (time1Success, time1) = await chunk.Stream.TryReadUInt32();
                (time2Success, time2) = await chunk.Stream.TryReadUInt32();
                (sizeSuccess, replaySizeInBytes) = await chunk.Stream.TryReadInt32();
            }
            else
            {
                replaySizeInBytes = chunk.SizeInBytes;
            }
            return sizeSuccess
                && time1Success
                && time2Success
                && await VisitReplayDataChunk(new ReplayDataInfo(time1, time2, replaySizeInBytes, chunk));
        }

        public virtual Task<bool> VisitReplayDataChunkContent(ReplayDataInfo replayDataInfo)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> VisitIncompleteChunk(uint chunkType, int sizeInBytes)
        {
            return Task.FromResult(false);
        }
    }
}
