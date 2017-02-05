using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OsuMapDownload.Models;

namespace OsuMapDownload
{
    public abstract class BeatmapDownloadProvider
    {
        public abstract string GetUrl(MapsetDownload download);

        public abstract WebRequest PrepareRequest(MapsetDownload download);

        public abstract string GetFileName(WebResponse response);

        public abstract void StartDownloadTask(Task downloadTask, MapsetDownload download);
    }
}
