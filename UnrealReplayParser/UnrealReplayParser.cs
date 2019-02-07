using Common.StreamHelpers;
using UnrealReplayParser;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser
{
    public enum ChunkType : uint
    {
        Header,
        ReplayData,
        Checkpoint,
        Event,
        Unknown = 0xFFFFFFFF
    };

    public class UnrealReplayParser : IDisposable
    {
        const uint FileMagic = 0x1CA2E27F;
        readonly Stream _stream;
        readonly SubStreamFactory _subStreamFactory;
        readonly bool _streamLengthAvailable;//TODO Use this
        protected UnrealReplayParser(ReplayInfo info, Stream stream)
        {
            Info = info;
            _stream = stream;
            _subStreamFactory = new SubStreamFactory(stream);
            try
            {
                long length = _stream.Length;
                _streamLengthAvailable = true;
            }
            catch (NotSupportedException)
            {
                _streamLengthAvailable = false;
            }
        }

        protected UnrealReplayParser(UnrealReplayParser replayParser)
        {
            Info = replayParser.Info;
            _stream = replayParser._stream;
            _subStreamFactory = replayParser._subStreamFactory;
            _streamLengthAvailable = replayParser._streamLengthAvailable;
        }

        public static async Task<UnrealReplayParser> FromStream(Stream stream)
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
                //timestamp = new DateTime(await stream.ReadInt64());
            }
            bool bCompressed = false;
            if (fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION)
            {
                bCompressed = await stream.ReadUInt32() != 0;
            }
            return new UnrealReplayParser(new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion), stream);
        }

        public ReplayInfo Info { get; }
        public ChunkInfo? HeaderChunk { get; set; }

        public virtual async Task<ChunkInfo?> ReadChunk()
        {
            (bool success, uint chunkType) = await _stream.TryReadUInt32(); //TODO WTF is happening here
            if (!success) return null;
            int sizeInBytes = await _stream.ReadInt32();
            if (sizeInBytes < 0)
            {
                throw new InvalidDataException("Invalid chunk data.");
            }
            if (_streamLengthAvailable && _stream.Length < sizeInBytes)
            {
                throw new EndOfStreamException("Need more bytes that what is available.");
            }
            ChunkInfo chunk = new ChunkInfo(chunkType, sizeInBytes, _subStreamFactory.Create(sizeInBytes, true));
            switch ((ChunkType)chunkType)
            {
                case ChunkType.Header:
                    if (HeaderChunk == null)
                    {
                        HeaderChunk = chunk;
                    }
                    else
                    {
                        throw new InvalidDataException("Multiple headers found");
                    }

                    return chunk;
                case ChunkType.Checkpoint:
                case ChunkType.Event:
                    {
                        string id = await chunk.Stream.ReadString();
                        string group = await chunk.Stream.ReadString();
                        string metadata = await chunk.Stream.ReadString();
                        uint time1 = await chunk.Stream.ReadUInt32();
                        uint time2 = await chunk.Stream.ReadUInt32();
                        int eventSizeInBytes = await chunk.Stream.ReadInt32();
                        return new EventInfo(chunk, -1, id, group, metadata, time1, time2, eventSizeInBytes);
                    }

                case ChunkType.ReplayData:
                    {
                        int replaySizeInBytes;
                        uint time1 = uint.MaxValue;
                        uint time2 = uint.MaxValue;
                        if (Info.FileVersion >= (uint)VersionHistory.HISTORY_STREAM_CHUNK_TIMES)
                        {
                            time1 = await chunk.Stream.ReadUInt32();
                            time2 = await chunk.Stream.ReadUInt32();
                            replaySizeInBytes = await chunk.Stream.ReadInt32();
                        }
                        else
                        {
                            replaySizeInBytes = chunk.SizeInBytes;
                        }
                        return new ReplayDataInfo(time1, time2, replaySizeInBytes, chunk);
                    }
                default:
                    throw new ArgumentOutOfRangeException("Invalid Chunk Type.");
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
