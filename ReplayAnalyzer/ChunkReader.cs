using Common.StreamHelpers;
using ReplayAnalyzer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UnrealReplayAnalyzer
{
    public class ChunkReader
    {
        const uint FileMagic = 0x1CA2E27F;
        readonly Stream _stream;
        protected bool StreamLengthAvailable;
        protected ChunkReader(ReplayInfo info, Stream stream)
        {
            Info = info;
            _stream = stream;
            try
            {
                long length = _stream.Length;
                StreamLengthAvailable = true;
            }
            catch (NotSupportedException)
            {
                StreamLengthAvailable = false;
            }
        }

        public ReplayInfo Info { get; }

        public static async Task<ChunkReader> FromStream(Stream stream)
        {
            if (FileMagic != await stream.ReadUInt32())
            {
                throw new InvalidDataException("Invalid file. Probably not a replayReader.");
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
                timestamp = new DateTime(await stream.ReadInt64());
            }
            bool bCompressed = false;
            if (fileVersion >= (uint)VersionHistory.HISTORY_COMPRESSION)
            {
                bCompressed = await stream.ReadUInt32() != 0;
            }
            return new ChunkReader(new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion), stream);
        }

        public virtual async Task<ChunkInfo> ReadChunk()
        {
            uint chunkType;
            try
            {
                chunkType = await _stream.ReadUInt32();
            }
            catch (EndOfStreamException)
            {
                return null;
            }
            int sizeInBytes = await _stream.ReadInt32();
            if (sizeInBytes < 0)
            {
                throw new InvalidDataException("Invalid chunk data.");
            }
            if (StreamLengthAvailable && _stream.Length < sizeInBytes)
            {
                throw new EndOfStreamException("Need more bytes that what is available.");
            }
            return new ChunkInfo(chunkType, sizeInBytes, new SubStream(_stream, sizeInBytes, true));
        }
    }
}
