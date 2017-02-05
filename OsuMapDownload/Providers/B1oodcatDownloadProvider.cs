using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using OsuMapDownload.Models;

namespace OsuMapDownload.Providers
{
    // ReSharper disable once InconsistentNaming
    public class B1oodcatDownloadProvider : BeatmapDownloadProvider
    {
        public static readonly string DOWNLOAD_URL = "http://bloodcat.com/osu/s/{0}";

        public override string GetUrl(MapsetDownload download)
        {
            return string.Format(DOWNLOAD_URL, download.ID);
        }

        public override WebRequest PrepareRequest(MapsetDownload download)
        {
           return WebRequest.Create(GetUrl(download));
        }

        public override string GetFileName(WebResponse response) {
            return new ContentDisposition(response.Headers["Content-Disposition"]).FileName;
        }

        public override void StartDownloadTask(Task downloadTask, MapsetDownload download) {
            downloadTask.Start();
        }
    }
}
