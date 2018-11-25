using Common.StreamHelpers;
using ReplayAnalyzer;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

    public class ReplayReader : ChunkReader
    {
        protected ReplayReader(ReplayInfo info, Stream stream) : base(info, stream)
        {
        }

        protected ReplayReader(ChunkReader reader) : base(reader)
        {
            
        }

        public override async Task<ChunkInfo> ReadChunk()
        {
            ChunkInfo chunk = await base.ReadChunk();
            if (chunk == null) return null;
            switch ((ChunkType) chunk.Type)
            {
                case ChunkType.Header:
                    if (Info.HeaderChunk == null)
                    {
                        //following is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
                        uint fortniteMagicNumber = await chunk.Stream.ReadUInt32();
                        uint headerVersion = await chunk.Stream.ReadUInt32();
                        uint fortniteVersionUUID = await chunk.Stream.ReadUInt32();
                        uint seasonNumber = await chunk.Stream.ReadUInt32();
                        uint alwaysZero = await chunk.Stream.ReadUInt32();

                        int expectedSize = 130;

                        if (headerVersion >= 11)
                        {
                            expectedSize = 146;
                            long guidPart1 = await chunk.Stream.ReadInt64();
                            long guidPart2 = await chunk.Stream.ReadInt64();
                        }

                        short alwaysFour = await chunk.Stream.ReadInt16();
                        uint anotherUnknownNumber = await chunk.Stream.ReadUInt32();//want from 20 to 21 after a version upgrade
                        uint numberThatKeepValueAcrossReplays = await chunk.Stream.ReadUInt32();
                        string fortniteRelease = await chunk.Stream.ReadString();
                        uint alwaysOne = await chunk.Stream.ReadUInt32();
                        string mapPath = await chunk.Stream.ReadString();
                        uint alwaysZero2 = await chunk.Stream.ReadUInt32();
                        uint alwaysThree = await chunk.Stream.ReadUInt32();
                        uint alwaysOne2 = await chunk.Stream.ReadUInt32();
                        string subGame = await chunk.Stream.ReadString();
                        if(expectedSize != chunk.SizeInBytes) throw new InvalidDataException("Didnt expected more data");
                        //byte[] bytes = await chunk.Stream.ReadBytes(chunk.SizeInBytes);
                        //using (StreamWriter writer = File.AppendText("dump"))
                        //{
                        //    await writer.WriteLineAsync(BitConverter.ToString(bytes));
                        //}

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
