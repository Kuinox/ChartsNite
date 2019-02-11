using System;
using System.IO;
using System.Threading.Tasks;
using Common.StreamHelpers;

namespace UnrealReplayParser
{
    class UnrealReplayVisitor
    {
        const uint FileMagic = 0x1CA2E27F;

        readonly Stream _stream;
        readonly SubStreamFactory _subStreamFactory;
        public ReplayInfo Info { get; }
        protected UnrealReplayVisitor(ReplayInfo info, Stream stream)
        {
            Info = info;
            _stream = stream;
            _subStreamFactory = new SubStreamFactory(stream);
        }

        public static async Task<UnrealReplayVisitor> FromStream(Stream stream)
        {
            if (FileMagic != await stream.ReadUInt32())
            {
                throw new InvalidDataException("Invalid file. Probably not an Unreal Replay.");
            }
            uint fileVersion = await stream.ReadUInt32();
            int lengthInMs = await stream.ReadInt32();
            uint networkVersion = await stream.ReadUInt32();
            uint changelist = await stream.ReadUInt32();
            string friendlyName = await stream.ReadString();
            bool bIsLive = await stream.ReadUInt32() != 0;
            DateTime timestamp = DateTime.MinValue;
            if (fileVersion >= (uint)VersionHistory.HISTORY_RECORDED_TIMESTAMP)
            {
                timestamp = DateTime.FromBinary(await stream.ReadInt64());
            }
            bool bCompressed = false;
            if (fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION)
            {
                bCompressed = await stream.ReadUInt32() != 0;
            }
            return new UnrealReplayVisitor(new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion), stream);
        }

        public virtual async Task<bool> Visit()
        {
            while (await VisitChunk()) ;
            return true;
        }

        public virtual async Task<bool> VisitChunk()
        {
            (bool typeSuccess, uint chunkType) = await _stream.TryReadUInt32();
            (bool sizeSuccess, int sizeInBytes) = await _stream.TryReadInt32();
            if (!typeSuccess || !sizeSuccess)
            {
                VisitIncompleteChunk(typeSuccess, chunkType, sizeSuccess, sizeInBytes);
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

        public virtual bool VisitIncompleteChunk(bool readType, uint chunkType, bool readSize, int sizeInBytes)
        {
            return false;
        }
    }
}
