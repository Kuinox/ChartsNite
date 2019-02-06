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

    public class UnrealReplayParser : ChunkReader
    {
        protected UnrealReplayParser(ReplayInfo info, Stream stream) : base(info, stream)
        {
        }

        public UnrealReplayParser(ChunkReader reader) : base(reader)
        {
            
        }

        public override async Task<ChunkInfo?> ReadChunk()
        {
            ChunkInfo? chunk = await base.ReadChunk();
            if (chunk == null) return null;
            switch ((ChunkType) chunk.Type)
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
                    if (Info.FileVersion >= (uint) VersionHistory.HISTORY_STREAM_CHUNK_TIMES)
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
                case ChunkType.Unknown:
                    Console.WriteLine("Unknown chunk ???");
                    return chunk;
                default:
                    throw new ArgumentOutOfRangeException("Invalid Chunk Type.");
            }
        }
    }
}
