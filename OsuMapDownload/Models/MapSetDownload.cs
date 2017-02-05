using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OsuMapDownload.Exceptions;

namespace OsuMapDownload.Models
{
    public enum MapsetDownloadStatus
    {
        Waiting,
        Downloading,
        Extracting,
        Completed,
        Failed
    }

    public class MapsetDownload
    {
        private Exception _error;

        public int ID { get; set; }
        public string Path { get; set; }
        public virtual MapsetDownloadStatus Status { get; set; } = MapsetDownloadStatus.Waiting;
        public virtual float Progress { get; protected set; }
        public virtual float Speed { get; protected set; }
        public BeatmapDownloadProvider DownloadProvider { get; protected set; }
        protected string FileName { get; set; }

        //One bool to rule them all
        public bool Completed => Status == MapsetDownloadStatus.Completed || Status == MapsetDownloadStatus.Failed;

        public virtual Exception Error {
            get { return _error; }
            set {
                _error = value;
                Status = MapsetDownloadStatus.Failed;
            }
        }

        public MapsetDownload(int id, string path, BeatmapDownloadProvider provider) {
            ID = id;
            Path = path;
            DownloadProvider = provider;
        }

        public virtual Task Start() {
            var task = GetTask();
            DownloadProvider.StartDownloadTask(task, this);
            return task;
        }

        public virtual Task GetTask() {
            return new Task(() => {
                try {
                    Download();
                } catch (Exception e) {
                    Error = e;
                }
            });
        }

        protected virtual void BeforeDownload() {}

        protected virtual void Download() {
            Status = MapsetDownloadStatus.Downloading;
            BeforeDownload();

            var speedTracker = new Stopwatch();
            speedTracker.Start();

            var request = DownloadProvider.PrepareRequest(this);
            var response = request.GetResponse();
            try {
                FileName = DownloadUtils.RemoveIllegalCharacters(DownloadProvider.GetFileName(response));
            } catch (Exception) {
                throw new MapsetNotFoundException(request.RequestUri.AbsoluteUri);
            }

            DownloadUtils.CheckCreateDir(Path);
            try {
                using (var fileStream = File.Create($"{Path}/{FileName}")) {
                    using (var bodyStream = response.GetResponseStream()) {
                        // Allocate 8k buffer
                        var buffer = new byte[8192];
                        // Get files initial size to calculate the progress
                        var fileSize = response.ContentLength;
                        // Will show how much bytes we downloaded in total
                        var bytesDownloaded = 0;
                        int bytesRead;
                        do {
                            // Read data up to 8k from stream
                            bytesRead = bodyStream.Read(buffer, 0, buffer.Length);
                            // Write it
                            fileStream.Write(buffer, 0, bytesRead);
                            bytesDownloaded += bytesRead;
                            //Set progress. A percentage
                            Progress = bytesDownloaded/(float) fileSize;
                            // Calc dl speed in kb
                            Speed = bytesRead/(float) speedTracker.Elapsed.Seconds;
                        } while (bytesRead > 0);
                        //Close the streams. We dont need them
                    }
                }
            } catch (Exception e) {
                speedTracker.Stop();
                throw new MapsetDownloadInterrupedException(e);
            }

            AfterDownload();
            Status = MapsetDownloadStatus.Completed;
        }

        protected virtual void AfterDownload() {}

        public virtual void Reset(BeatmapDownloadProvider provider) {
            DownloadProvider = provider;
            Status = MapsetDownloadStatus.Waiting;
            Error = null;
            Progress = 0;
            Speed = 0;
        }
    }
}