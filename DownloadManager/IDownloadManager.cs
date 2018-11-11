using System.IO;

namespace ChartsNite.DownloadManager
{
    public interface IDownloadManager
    {
        Stream GetDownload(string id);
    }
}
