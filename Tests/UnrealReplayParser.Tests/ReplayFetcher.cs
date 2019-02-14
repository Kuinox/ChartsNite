using FortniteReplayParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace UnrealReplayParser.Tests
{
    public class ReplayFetcher
    {
        public IEnumerable<(Type, string)> GetAllReplaysStreams() 
        {
            string path = Directory.GetCurrentDirectory();
            while (!Directory.Exists(path + Path.DirectorySeparatorChar + "Replays"))
            {
                path = Directory.GetParent(path).FullName;
            }
            path += Path.DirectorySeparatorChar + "Replays";
            string[] paths = Directory.GetFiles(path, "*.replay");

            return from x in ParserToTest()
                   from y in paths
                   select (x, y);
        }

        public virtual List<Type> ParserToTest()
        {
            return new List<Type>() { typeof(UnrealReplayVisitor), typeof(FortniteReplayVisitor) };
        }
    }
}
