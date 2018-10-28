using System;
using ReplayAnalyzer;
using System.IO;
using System.Threading.Tasks;

namespace Analyzer
{
	class Program
	{
		static async Task Main(string[] args)
		{
			const string saveName = @"UnsavedReplay-2018.06.02-15.14.56";

			using (var saveFile = File.OpenRead(saveName + ".replay"))
            using (var replayStream = await ReplayStream.FromStream(saveFile))
            {
                while (replayStream.Position < replayStream.Length)
                {
                    using (var chunkInfo = await replayStream.ReadChunk())
                    using (var reader = new StreamReader(chunkInfo))
                    {
                        if (chunkInfo.Type == ChunkType.Event)
                        {
                            EventInfo info = (EventInfo)chunkInfo;
                            Console.WriteLine(info.Group);
                            Console.WriteLine(info.Id);
                            Console.WriteLine(info.Metadata);
                            //Console.WriteLine(reader.ReadToEnd());
                        }
                        //replayStream.Seek(chunkInfo.SizeInBytes + chunkInfo.DataOffset, SeekOrigin.Begin);
                    }
                        
                }
			}
		}
	}
}