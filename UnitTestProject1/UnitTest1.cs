using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsuMapDownload;
using OsuMapDownload.Models;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private const string SONGS_PATH = @"G:\Games\OsuTest\Songs";
        private const string TEMP_PATH = @"G:\Games\OsuTest\TempDL";

        [TestMethod]
        public void TestAsyncDownload()
        {
            var download = new MapSetDownload("http://bloodcat.com/osu/s/138554", TEMP_PATH);
            var task = download.CreateTask();
            task.Start();
            while (!task.IsCompleted)
            {
                Debug.WriteLine(download.Progress + " with speed " + download.Speed);
                Thread.Sleep(100);
            }
            Debug.WriteLine(download.Completed);
            Debug.WriteLine(download.Failed);
        }

        [TestMethod]
        public void TestAsyncDownloadMultiple()
        {
            DownloadUtils.SetThreadCountMax();
            var download = new MapSetDownload("http://bloodcat.com/osu/s/138554", TEMP_PATH);
            var task = download.CreateTask();
            var download2 = new MapSetDownload("http://bloodcat.com/osu/s/553711", TEMP_PATH);
            var task2 = download2.CreateTask();
            var download3 = new MapSetDownload("http://bloodcat.com/osu/s/483147", TEMP_PATH);
            var task3 = download3.CreateTask();
            task.Start();
            task2.Start();
            task3.Start();
            while (!task.IsCompleted || !task2.IsCompleted || !task3.IsCompleted)
            {
                Debug.WriteLine($"Download 1 - Progress: {download.Progress} Speed: {download.Speed} kb/s");
                Debug.WriteLine($"Download 2 - Progress: {download2.Progress} Speed: {download2.Speed} kb/s");
                Debug.WriteLine($"Download 3 - Progress: {download3.Progress} Speed: {download3.Speed} kb/s");
                Thread.Sleep(100);
            }
            if (download.Failed)
            {
                Debug.WriteLine(download.Error.ToString());
            }
            Debug.WriteLine($"Download 1 Completed: {download.Completed} or Failed: {download.Failed}");
            Debug.WriteLine($"Download 2 Completed: {download2.Completed} or Failed: {download2.Failed}");
            Debug.WriteLine($"Download 3 Completed: {download3.Completed} or Failed: {download3.Failed}");
        }

        [TestMethod]
        public void TestAsyncDownloadExtract()
        {
            var download = new MapSetExtractDownload("http://bloodcat.com/osu/s/138554", TEMP_PATH);
            var task = download.CreateTask();
            task.Start();
            while (!task.IsCompleted)
            {
                Debug.WriteLine(download.Progress + " with speed " + download.Speed);
                Thread.Sleep(100);
            }
            Debug.WriteLine(download.Completed);
            Debug.WriteLine(download.Failed);
            if (download.Completed && download.Extracted)
            {
                Debug.WriteLine("Map hashes");
                foreach (var hash in download.MapHashes)
                {
                    Debug.WriteLine(hash);
                }
            }
        }

    }
}
