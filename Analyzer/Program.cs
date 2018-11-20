using FortniteReplayAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //await ParseReplay(@"UnsavedReplay-2018.10.28-23.50.48.replay");
            //Console.WriteLine(await ReadString());
            // const string saveName =;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            foreach (string s in Directory.GetFiles("Replays\\", "*.replay"))
            {
                try
                {
                    await ParseReplay(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Done. Elapsed Milliseconds: " + elapsedMs);
            Console.ReadKey();
        }

        static async Task ParseReplay(string saveName)
        {
            Console.WriteLine("______________________________________" + saveName);
            using (FileStream saveFile = File.OpenRead(saveName))
            using (var replayStream = await FortniteReplayStream.FortniteReplayFromStream(saveFile))
            {
                while (replayStream.Position < replayStream.Length)
                {
                    using (var chunkInfo = await replayStream.ReadChunk())
                    {
                        if (!(chunkInfo is KillEventChunk kill)) continue;
                        //if (kill.PlayerKilling == "Kuinox_" || kill.PlayerKilled == "Kuinox_" || kill.PlayerKilled == "DexterNeo" || kill.PlayerKilling == "DexterNeo")
                        {
                            Console.WriteLine(kill.Weapon + " " + kill.VictimState + " Killer: " + kill.PlayerKilling + " Killed: " + kill.PlayerKilled + "time: " + kill.Time1 + "state: " + kill.VictimState);
                        }
                    }
                }
            }
        }

        public static async Task<byte[]> ReadBytes(Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            if (await stream.ReadAsync(buffer, 0, count) != count) throw new InvalidDataException("Did not read the expected number of bytes.");
            return buffer;
        }


        //public async Task<byte[]> ReadToEnd() => await ReadBytes((int)(_stream.Length - _stream.Position));
        public static async Task<byte> ReadByteOnce(Stream stream) => (await ReadBytes(stream, 1))[0];
        public static async Task<uint> ReadUInt32(Stream stream) => BitConverter.ToUInt32(await ReadBytes(stream, 4));
        public static async Task<int> ReadInt32(Stream stream) => BitConverter.ToInt32(await ReadBytes(stream, 4));
        public static async Task<long> ReadInt64(Stream stream) => BitConverter.ToInt64(await ReadBytes(stream, 8));

        public static async Task<string> ReadString()
        {
            List<byte> test = new List<byte> { 0xFB, 0xFF, 0xFF, 0xFF, 0x53, 0x00, 0x61, 0x00, 0xEF, 0x00, 0x2E, 0x00, 0x00, 0x00 };
            Stream stream = new MemoryStream(test.ToArray());
            var length = await ReadInt32(stream);
            var isUnicode = length < 0;
            byte[] data;
            string value;

            if (isUnicode)
            {
                length = -length;
                data = await ReadBytes(stream, length * 2);
                value = Encoding.Unicode.GetString(data);
            }
            else
            {
                data = await ReadBytes(stream, length);
                value = Encoding.Default.GetString(data);
            }
            return value.Trim(' ', '\0');
        }
    }
}