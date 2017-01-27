using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuMapDownload
{
    public class DownloadUtils
    {
        public static void SetThreadCountMax()
        {
            //Set max amount of threads working
            ThreadPool.SetMaxThreads(100, 10);
            //Set max amount of connections. Default was 2; thats not much
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000000;
        }

        /// <summary>
        /// Calculate md5 hash from a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetHashFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return GetHashFromStream(stream);
            }
        }

        /// <summary>
        /// Calculate md5 hash from a stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetHashFromStream(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
            }
        }
    }
}