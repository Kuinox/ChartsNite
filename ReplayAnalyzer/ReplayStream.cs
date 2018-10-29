using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReplayAnalyzer
{
    public class ReplayStream : Stream
    {
        readonly Stream _stream;

        const uint FileMagic = 0x1CA2E27F;

        protected ReplayStream(Stream stream)
        {
            _stream = stream;
        }

        protected ReplayStream(ReplayStream stream)
        {
            _stream = stream._stream;
            Info = stream.Info;
        }

        public ReplayInfo Info { get; private set; }

        public static async Task<ReplayStream> FromStream(Stream stream)
        {
            ReplayStream replayStream = new ReplayStream(stream);
            long totalSize = stream.Length;
            if (FileMagic != await replayStream.ReadUInt32())
            {
                throw new InvalidDataException("Invalid file. Probably not a replayStream.");
            }

            uint fileVersion = await replayStream.ReadUInt32();

            var lengthInMs = await replayStream.ReadInt32();
            var networkVersion = await replayStream.ReadUInt32();
            var changelist = await replayStream.ReadUInt32();
            var friendlyName = await replayStream.ReadString();
            var bIsLive = await replayStream.ReadUInt32() != 0;
            DateTime timestamp = DateTime.MinValue;
            if (fileVersion >= (uint)VersionHistory.HISTORY_RECORDED_TIMESTAMP)
            {
                timestamp = new DateTime(await replayStream.ReadInt64());
            }

            bool bCompressed = false;

            if (fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION)
            {
                bCompressed = await replayStream.ReadUInt32() != 0;
            }

            replayStream.Info = new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion);
            return replayStream;
        }

        public virtual async Task<ChunkInfo> ReadChunk()
        {
            long typeOffset = Position;
            ChunkType chunkType = (ChunkType)await ReadUInt32();
            int sizeInBytes = await ReadInt32();

            long dataOffset = Position;
            ChunkInfo chunk = new ChunkInfo(chunkType, sizeInBytes, typeOffset, dataOffset, new SubStream(this, sizeInBytes));
            if (chunk.SizeInBytes < 0 || chunk.DataOffset + chunk.SizeInBytes > Length)
            {
                throw new InvalidDataException("Invalid chunk data.");
            }

            switch (chunkType)
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
                        EventInfo eventInfo = new EventInfo(chunk ,- 1, id, group, metadata, time1, time2, sizeInBytes,
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<byte[]> ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            if (await _stream.ReadAsync(buffer, 0, count) != count) throw new InvalidDataException("Did not read the expected number of bytes.");
            return buffer;
        }


        //public async Task<byte[]> ReadToEnd() => await ReadBytes((int)(_stream.Length - _stream.Position));
        public async Task<byte> ReadByteOnce() => (await ReadBytes(1))[0];
        public async Task<uint> ReadUInt32() => BitConverter.ToUInt32(await ReadBytes(4));
        public async Task<int> ReadInt32() => BitConverter.ToInt32(await ReadBytes(4));
        public async Task<long> ReadInt64() => BitConverter.ToInt64(await ReadBytes(8));
        public async Task<string> ReadString()
        {
            var length = await ReadInt32();
            var isUnicode = length < 0;
            byte[] data;
            string value;

            if (isUnicode)
            {
                length = -length;
                data = await ReadBytes(length * 2);
                value = Encoding.Unicode.GetString(data);
            }
            else
            {
                data = await ReadBytes(length);
                value = Encoding.Default.GetString(data);
            }
            return value.Trim(' ', '\0');
        }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public override void SetLength(long value) => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
