using FortniteReplayParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace UnrealReplayParser.Tests
{
    public class ReplayFetcher
    {
        public IEnumerable<string> GetAllReplaysPath()
        {
            string path = Directory.GetCurrentDirectory();
            while (!Directory.Exists(path + Path.DirectorySeparatorChar + "Replays"))
            {
                path = Directory.GetParent(path).FullName;
            }
            path += Path.DirectorySeparatorChar + "Replays";
           return Directory.GetFiles(path, "*.replay");
        }
        public IEnumerable<(Type, string)> GetAllReplaysStreamsWithAllParsers() 
        {
            return from x in ParserToTest()
                   from y in GetAllReplaysPath()
                   select (x, y);
        }

        public virtual List<Type> ParserToTest()
        {
            return new List<Type>() { typeof(UnrealReplayVisitor), typeof(FortniteReplayVisitor) };
        }
    }
}
