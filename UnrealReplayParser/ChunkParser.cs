﻿using Common.StreamHelpers;
using UnrealReplayParser;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UnrealReplayParser
{
    public class ChunkReader : IDisposable
    {
        const uint FileMagic = 0x1CA2E27F;
        readonly Stream _stream;
        readonly SubStreamFactory _subStreamFactory;
        readonly bool _streamLengthAvailable;//TODO Use this
        protected ChunkReader(ReplayInfo info, Stream stream)
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

        protected ChunkReader(ChunkReader reader)
        {
            Info = reader.Info;
            _stream = reader._stream;
            _streamLengthAvailable = reader._streamLengthAvailable;
            _subStreamFactory = reader._subStreamFactory;
        }

        public ReplayInfo Info { get; }

        public static async Task<ChunkReader> FromStream(Stream stream)
        {
            if (FileMagic != await stream.ReadUInt32())
            {
                //throw new InvalidDataException("Invalid file. Probably not an Unreal Replay.");
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
            return new ChunkReader(new ReplayInfo(lengthInMs, networkVersion, changelist, friendlyName, timestamp, 0,
                bIsLive, bCompressed, fileVersion), stream);
        }
              
        /// <summary>
        /// Return <see cref="null"/> if it couldn't read the chunk length.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ChunkInfo?> ReadChunk() //TODO see if it's possible to avoid null.
        {
            (bool success, uint chunkType) = await _stream.TryReadUInt32(); //TODO WTF is happening here
            if(!success) return null;
            int sizeInBytes = await _stream.ReadInt32();
            if (sizeInBytes < 0)
            {
                throw new InvalidDataException("Invalid chunk data.");
            }
            if (_streamLengthAvailable && _stream.Length < sizeInBytes)
            {
                throw new EndOfStreamException("Need more bytes that what is available.");
            }
            return new ChunkInfo(chunkType, sizeInBytes, _subStreamFactory.Create(sizeInBytes, true));
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
