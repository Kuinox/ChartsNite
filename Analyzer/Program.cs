using FortniteReplayAnalyzer;
using ReplayAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();
            string[] files = Directory.GetFiles("Replays\\", "*.replay");
            var filesSorted = files.OrderByDescending(File.GetLastWriteTime);
            foreach (string s in filesSorted)
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
          //  Console.WriteLine("______________________________________" + saveName);
            using (FileStream saveFile = File.OpenRead(saveName))
            using (FortniteReplayReader replayStream = await FortniteReplayReader.FortniteReplayFromStream(saveFile))
            {
                ChunkInfo chunkInfo;
                do
                {
                    using (chunkInfo = await replayStream.ReadChunk())
                    {
                        if (!(chunkInfo is KillEventChunk kill)) continue;
                        //if (kill.PlayerKilling == "Kuinox_" || kill.PlayerKilled == "Kuinox_" || kill.PlayerKilled == "DexterNeo" || kill.PlayerKilling == "DexterNeo")
                        {
                            //Console.WriteLine(kill.Weapon + " " + kill.VictimState + " Killer: " + kill.PlayerKilling +
                            //                  " Killed: " + kill.PlayerKilled + "time: " + kill.Time1 + "state: " +
                            //                  kill.VictimState);
                        }
                    }
                } while (chunkInfo != null);
            }
        }
    }
}