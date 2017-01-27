using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OsuMapDownload.Models
{
    public class MapSetDownload
    {
        private Stopwatch _speedTracker { get; set; }

        /// <summary>
        /// Url of the download
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Location where the OSZ file should be downloaded to/temporarily saved.
        /// This does not include the file name
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// File name to give the OSZ file. Note: this also includes the extension
        /// It can be left empty but if you are not using bloodcat there is a chance that the name would not me found
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Are we done with downloading?
        /// </summary>
        public virtual bool Completed => Task != null && Task.IsCompleted;

        /// <summary>
        /// Has something happened with the download? If yes it will be set to an instance of the exception
        /// </summary>
        public virtual Exception Error { get; set; }

        // <summary>
        /// Has something happened with the download?
        /// </summary>
        public virtual bool Failed => Error != null;

        /// <summary>
        /// Has the map been succesfully extracted? If we didnt even start it will be always false
        /// </summary>
        public virtual bool Extracted { get; set; }

        /// <summary>
        /// How far are we with download? In %
        /// </summary>
        public virtual float Progress { get; set; }

        /// <summary>
        /// Download speed; In kb/s;
        /// </summary>
        public virtual float Speed { get; set; }

        /// <summary>
        /// If the download has begun it will be set to an instance of task running. Not very interesting
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Create the download model; Has to be started with CreateTask().Start()
        /// </summary>
        /// <param name="url">Download url</param>
        /// <param name="path">Path osz will temporarily placed in</param>
        /// <param name="name">Name of the file. Can be left empty.</param>
        public MapSetDownload(string url, string path, string name = null)
        {
            _speedTracker = new Stopwatch();
            Url = url;
            Path = path;
            Name = name;
        }

        /// <summary>
        /// Create task which should be run async to ONLY download
        /// </summary>
        /// <returns></returns>
        public Task CreateTask()
        {
            Task = new Task(() =>
            {
                try
                {
                    StartDownload();
                }
                catch (Exception e)
                {
                    Error = e;
                }
            });
            return Task;
        }

        /// <summary>
        /// Create task which should be run async to download and EXTRACT the beatmap
        /// </summary>
        /// <param name="songsPath">Osu songs folder. Folder where the map should be extracted</param>
        /// <returns></returns>
        public Task CreateTask(string songsPath)
        {
            Task = new Task(() =>
            {
                try
                {
                    StartDownload();
                    Extract(songsPath);
                }
                catch (Exception e)
                {
                    Error = e;
                }
            });
            return Task;
        }

        /// <summary>
        /// Downloads a map to the path with given name. If name is not set it will try to figure out its original name which is SOMETIMES set in headers.
        /// This method will start download sinchronized with current thread. So it is adviced to use CreateTask method to run it async.
        /// If download fails parameter Failed will be set to true otherwise Completed will be true.
        /// 
        /// If you are running this not ascync you have to catch the exceptions yourself ¯\_(ツ)_/¯ 
        /// </summary>
        public void StartDownload()
        {
            //Start timer to use it in calculation of download speed
            _speedTracker.Start();

            //Debug.WriteLine($"Creating request for url {Url}");
            //Create request
            var request = WebRequest.Create(Url);
            var response = request.GetResponse();

            //If name is not set; check if it is set in header otherwise throw an error.
            if (Name == null) Name = new ContentDisposition(response.Headers["Content-Disposition"]).FileName;

            //Debug.WriteLine($"Creating osz at {Path}/{Name}");
            //Create a stream to write a file. If path does not exists; Create it.
            if (!Directory.Exists(Path))
            {
                var di = Directory.CreateDirectory(Path);
            }
            using (var fileStream = File.Create($"{Path}/{Name}"))
            {
                //Create a stream to download a map
                using (var bodyStream = response.GetResponseStream())
                {
                    // Allocate 8k buffer
                    var buffer = new byte[8192];
                    // Get files initial size to calculate the progress
                    var fileSize = response.ContentLength;
                    // Will show how much bytes we downloaded in total
                    var bytesDownloaded = 0;
                    int bytesRead;
                    do
                    {
                        // Read data up to 8k from stream
                        bytesRead = bodyStream.Read(buffer, 0, buffer.Length);
                        // Write it
                        fileStream.Write(buffer, 0, bytesRead);
                        bytesDownloaded += bytesRead;
                        //Set progress. A percentage
                        Progress = bytesDownloaded/(float) fileSize*100f; //TODO: move into getters
                        // Calc dl speed in kb
                        Speed = bytesRead/(float) _speedTracker.Elapsed.Seconds; //TODO: is it even useful?
                    } while (bytesRead > 0);
                    //Close the streams. We dont need them
                }
            }
        }

        public void Extract(string songsPath)
        {
            //Check if path exists; If it does delete&overwrite; Remove the dots from dirname ofcourse. Osu does it too
            var destination = $"{songsPath}/{MakeOsuFolderName(Name)}";
            if (!Directory.Exists(destination))
            {
                var di = Directory.CreateDirectory(destination);
            }
            else
            {
                Directory.Delete(destination, true);
            }
            //Extract into our path
            ZipFile.ExtractToDirectory($"{Path}/{Name}", destination);
            Extracted = true;
        }

        /// <summary>
        /// Removes the extension and all the dots from osz file name.
        /// </summary>
        /// <param name="originalOsz"></param>
        /// <returns></returns>
        public static string MakeOsuFolderName(string originalOsz)
        {
            return originalOsz.Remove(originalOsz.LastIndexOf(".", StringComparison.Ordinal)).Replace(".", "");
        }
    }
}