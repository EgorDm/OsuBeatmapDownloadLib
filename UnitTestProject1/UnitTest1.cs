using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsuMapDownload;
using OsuMapDownload.Models;
using OsuMapDownload.Providers;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private const string SONGS_PATH = @"G:\Games\OsuTest\Songs";
        public const string TEMP_PATH = @"G:\Games\OsuTest\LibTempDL";

        [TestMethod]
        public void TestDownloadBloodcat() {
            TestDownloadOne(new B1oodcatDownloadProvider());
        }

        [TestMethod]
        public void TestDownloadOsu() {
            TestDownloadOne(new OsuDownloadProvider(UnitTestExperimental.USERNAME, UnitTestExperimental.PASSWORD));
        }

        public void TestDownloadOne(BeatmapDownloadProvider provider) {
            var download = new MapsetDownload(138554, UnitTest1.TEMP_PATH,
                provider);
            download.Start();
            while (download.Status != MapsetDownloadStatus.Completed && download.Status != MapsetDownloadStatus.Failed) {
                Debug.WriteLine(download.Progress + " with speed " + download.Speed);
                Thread.Sleep(100);
            }
            Debug.WriteLine("Download is: " + download.Status);
            if (download.Status == MapsetDownloadStatus.Failed) {
                Debug.WriteLine("Exception " + download.Error.ToString());
            }
        }
        
        [TestMethod]
        public void TestDownloadMultipleBloodcat() {
            TestDownloadMultiple(new B1oodcatDownloadProvider());
        }
        
        [TestMethod]
        public void TestDownloadMultipleOsu() {
            TestDownloadMultiple(new OsuDownloadProvider(UnitTestExperimental.USERNAME, UnitTestExperimental.PASSWORD));
        }

        public void TestDownloadMultiple(BeatmapDownloadProvider provider) {
            DownloadUtils.SetThreadCountMax();
            var downloads = new[] {
                new MapsetDownload(138554, TEMP_PATH, provider),
                new MapsetDownload(553711, TEMP_PATH, provider),
                new MapsetDownload(483147, TEMP_PATH, provider),
            };
            foreach (var mapsetDownload in downloads) {
                mapsetDownload.Start();
            }
            var finished = false;
            while (!finished) {
                finished = true;
                for (var i = 0; i < downloads.Length; i++) {
                    Debug.WriteLine($"Download {i} - Progress: {downloads[i].Progress} Speed: {downloads[i].Speed} kb/s");
                    if (finished) finished = downloads[i].Completed;
                }
                Thread.Sleep(100);
            }
            for (var i = 0; i < downloads.Length; i++) {
                Debug.WriteLine($"Download {i} Status: {downloads[i].Status}");
                if (downloads[i].Status == MapsetDownloadStatus.Failed) {
                    Debug.WriteLine("Exception " + downloads[i].Error.ToString());
                }
            }
        }

        [TestMethod]
        public void TestDownloadExtractBloodcat() {
            TestDownloadExtract(new B1oodcatDownloadProvider());
        }

        [TestMethod]
        public void TestDownloadExtractOsu() {
            TestDownloadExtract(new OsuDownloadProvider(UnitTestExperimental.USERNAME, UnitTestExperimental.PASSWORD));
        }

        public void TestDownloadExtract(BeatmapDownloadProvider provider) {
            var download = new MapsetExtractDownload(138554, UnitTest1.TEMP_PATH, provider);
            download.Start();
            while (download.Status != MapsetDownloadStatus.Completed && download.Status != MapsetDownloadStatus.Failed) {
                Debug.WriteLine(download.Progress + " with speed " + download.Speed);
                Thread.Sleep(100);
            }
            Debug.WriteLine("Download is: " + download.Status);
            if (download.Status == MapsetDownloadStatus.Failed) {
                Debug.WriteLine("Exception " + download.Error.ToString());
            }
            if (download.Completed && download.Status != MapsetDownloadStatus.Failed) {
                Debug.WriteLine("Map hashes");
                foreach (var hash in download.MapHashes) {
                    Debug.WriteLine(hash);
                }
            }
        }

        [TestMethod]
        public void WriteCookies() {
            var provider = new OsuDownloadProvider(UnitTestExperimental.USERNAME, UnitTestExperimental.PASSWORD);
            provider.CheckOrLogin();
            while (provider.LoginTask != null) {
                Thread.Sleep(100);
            }
            if (!provider.LoggedIn) {
                Debug.WriteLine("Failed. We are not logged in.");
                return;
            }
            var path = $"{TEMP_PATH}/cookies.txt";
            var uri = new Uri(OsuDownloadProvider.BASE_URL);
            Debug.WriteLine("Writing to " + path);
            using (var fs = new FileStream(path, FileMode.Create)) {
                DownloadUtils.SerializeCookies(provider.Cookies.GetCookies(uri), uri, fs);
            }
        }

        [TestMethod]
        public void ReadCookies() {
            var path = $"{TEMP_PATH}/cookies.txt";
            var uri = new Uri(OsuDownloadProvider.BASE_URL);
            CookieContainer container;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                container = DownloadUtils.DeserializeCookies(fs, uri);
            }
            var provider = new OsuDownloadProvider(UnitTestExperimental.USERNAME, UnitTestExperimental.PASSWORD, container);
            provider.CheckOrLogin();
            while (provider.LoginTask != null) {
                Thread.Sleep(100);
            }
            if (!provider.LoggedIn) {
                Debug.WriteLine("We are not logged in.");
            } else {
                Debug.WriteLine("We are logged in!");
            }
        }
    }
}