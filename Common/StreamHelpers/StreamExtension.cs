using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public static class StreamExtension
    {
        #region ReadBytes
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

        public static async Task<(bool success, byte[] value)> TryReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int toRead = count;
            while (toRead > 0)
            {
                int read = await stream.ReadAsync(buffer, count - toRead, count);
                if (read == 0)
                {
                    return (false, buffer);
                }
                toRead -= read;
            }
            return (true, buffer);
        }
        #endregion ReadBytes
        #region ReadNumbers
        public static async Task<(bool success, uint value)> TryReadUInt32(this Stream stream)
        {
            (bool success, byte[] value) = await stream.TryReadBytes(4);
            return (success, BitConverter.ToUInt32(value, 0));
        }
        public static async Task<(bool success, int value)> TryReadInt32(this Stream stream)
        {
            (bool success, byte[] value) = await stream.TryReadBytes(4);
            return (success, BitConverter.ToInt32(value, 0));
        }
        public static async Task<byte> ReadByteOnce(this Stream stream) => (await stream.ReadBytes(1))[0];
        public static async Task<uint> ReadUInt32(this Stream stream) => BitConverter.ToUInt32(await stream.ReadBytes(4), 0);

        public static async Task<float> ReadSingle(this Stream stream) => BitConverter.ToSingle(await stream.ReadBytes(4), 0);
        public static async Task<int> ReadInt32(this Stream stream) => BitConverter.ToInt32(await stream.ReadBytes(4), 0);
        public static async Task<short> ReadInt16(this Stream stream) => BitConverter.ToInt16(await stream.ReadBytes(2), 0);
        public static async Task<long> ReadInt64(this Stream stream) => BitConverter.ToInt64(await stream.ReadBytes(8), 0);
        #endregion ReadNumbers
        #region ReadString
        public static async Task<(bool success, string value)> TryReadString(this Stream stream)
        {
            (bool success, int length) = await stream.TryReadInt32();
            if (!success) return (false, "");
            bool isUnicode = length < 0;
            byte[] data;
            string value;
            if (isUnicode)
            {
                length = -length;
                (success, data) = await stream.TryReadBytes(length * 2);
                value = Encoding.Unicode.GetString(data);
            }
            else
            {
                (success, data) = await stream.TryReadBytes(length);
                value = Encoding.Default.GetString(data);
            }
            return (success, value.Trim(' ', '\0'));
        }

        public static async Task<string> ReadString(this Stream stream)
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
                if (length > 258)
                {
                    string dump;
                    if (stream.Length > length)
                    {
                        dump = Encoding.ASCII.GetString(await stream.ReadBytes(length));

                    }
                    else
                    {
                        dump = Encoding.ASCII.GetString(await stream.ReadBytes((int)(stream.Length - stream.Position)));
                    }
                    throw new InvalidDataException("string length too high, Stream DUMP: " + dump);
                }
                data = await stream.ReadBytes(length);
                value = Encoding.Default.GetString(data);
            }
            return value.Trim(' ', '\0');
        }
        #endregion ReadString
    }
}
