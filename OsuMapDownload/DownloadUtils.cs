using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
