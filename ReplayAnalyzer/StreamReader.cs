using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ReplayAnalyzer;

namespace UnrealReplayAnalyzer
{
    enum ChunkType : uint
    {
        Header,
        ReplayData,
        Checkpoint,
        Event,
        Unknown = 0xFFFFFFFF
    };
    class StreamReader : ChunkReader
    {
        protected StreamReader(ReplayInfo info, Stream stream) : base(info, stream)
        {
        }

        public override async Task<ChunkInfo> ReadChunk()
        {
            ChunkInfo chunk = await base.ReadChunk();

            switch ((ChunkType)chunk.Type)
            {
                case ChunkType.Header:
                    if (Info.HeaderChunk == null)
                    {
                        Info.HeaderChunk = chunk;
                    }
                    else
                    {
                        throw new InvalidDataException("Multiple headers found");
                    }
                    return chunk;
                case ChunkType.Checkpoint:
                case ChunkType.Event:
                    {
                        string id = await ReadString();
                        string group = await ReadString();
                        string metadata = await ReadString();
                        uint time1 = await ReadUInt32();
                        uint time2 = await ReadUInt32();
                        long eventDataOffset = Position;
                        EventInfo eventInfo = new EventInfo(chunk, -1, id, group, metadata, time1, time2, sizeInBytes,
                            eventDataOffset);
                        if (eventInfo.EventSizeInBytes < 0 ||
                            eventInfo.EventDataOffset + eventInfo.EventSizeInBytes > Length)
                        {
                            throw new InvalidDataException(
                                "ReadReplayInfo: Invalid event size: " + eventInfo.EventSizeInBytes);
                        }

                        return eventInfo;
                    }

                case ChunkType.ReplayData:
                    {
                        long replaySizeInBytes;
                        uint time1 = uint.MaxValue;
                        uint time2 = uint.MaxValue;
                        if (Info.FileVersion >= (uint)VersionHistory.HISTORY_STREAM_CHUNK_TIMES)
                        {
                            time1 = await ReadUInt32();
                            time2 = await ReadUInt32();
                            replaySizeInBytes = await ReadInt32();

                        }
                        else
                        {
                            replaySizeInBytes = sizeInBytes;
                        }

                        long replayDataOffset = Position;

                        return new ReplayDataInfo(-1, time1, time2, sizeInBytes, replayDataOffset, -1, chunk);
                    }
                case ChunkType.Unknown:
                    Console.WriteLine("Unknown chunk ???");
                    return null;
                default:
                    Console.WriteLine("POSITION: "+_stream.Position);
                    throw new ArgumentOutOfRangeException("Invalid ChunkType");
        }
    }
}
