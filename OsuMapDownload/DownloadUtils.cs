using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        public static bool ResponseContains(WebResponse haystack, string needle)
        {
            using (var responseStream = haystack.GetResponseStream())
            {
                if (responseStream == null) return false;
                using (var reader = new StreamReader(responseStream))
                {
                    var responseText = reader.ReadToEnd();
                    //Debug.WriteLine(responseText);
                    return responseText.Contains(needle);
                }
            }
        }

        public static void CheckCreateDir(string directory) {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }

        public static string RemoveIllegalCharacters(string filename) {
            filename = filename.Replace('*', '-');
            var invalid = new string(Path.GetInvalidFileNameChars());
            foreach (var c in invalid) {
                filename = filename.Replace(c.ToString(), "");
            }
            return filename;
        }
    }
}