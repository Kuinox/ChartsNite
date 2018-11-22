﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public static class StreamExtension
    {

        public static async Task<byte[]> ReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int toRead = count;
            while (toRead > 0)
            {
                int read = await stream.ReadAsync(buffer, count - toRead, count);//TODO AWAIT EVERYWHERE
                if (read == 0)
                {
                    throw new EndOfStreamException("Did not read the expected number of bytes.");
                }
                toRead -= read;
            }
            return buffer;
        }
        public static async Task<byte> ReadByteOnce( this Stream stream) => (await stream.ReadBytes(1))[0];
        public static async Task<uint> ReadUInt32( this Stream stream) => BitConverter.ToUInt32(await stream.ReadBytes(4), 0);
        public static async Task<int> ReadInt32( this Stream stream) => BitConverter.ToInt32(await stream.ReadBytes(4),0);
        public static async Task<long> ReadInt64( this Stream stream) => BitConverter.ToInt64(await stream.ReadBytes(8),0);
        public static async Task<string> ReadString( this Stream stream)
        {
            int length = await stream.ReadInt32();
            bool isUnicode = length < 0;
            byte[] data;
            string value;

            if (isUnicode)
            {
                length = -length;
                data = await stream.ReadBytes(length * 2);
                value = Encoding.Unicode.GetString(data);
            }
            else
            {
                data = await stream.ReadBytes(length);
                value = Encoding.Default.GetString(data);
            }
            return value.Trim(' ', '\0');
        }
    }
}
