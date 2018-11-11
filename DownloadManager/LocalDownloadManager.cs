using System.IO;

namespace ChartsNite.DownloadManager
{
    class LocalDownloadManager : IDownloadManager
    {
        public Stream GetDownload(string id)
        {
            return File.OpenRead("replays/" + id);
        }
    }
}
