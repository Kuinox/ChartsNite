using System.IO;

namespace ChartsNite.TestHelper
{
    public static class ReplayFetcher
    {
        public static string[] GetAllReplaysStreams()
        {
            string path = Directory.GetCurrentDirectory();
            while (!Directory.Exists(path + Path.DirectorySeparatorChar + "Replays"))
            {
                path = Directory.GetParent(path).FullName;
            }
            path += Path.DirectorySeparatorChar + "Replays";
            return Directory.GetFiles(path, "*.replay");
        }
    }
}
