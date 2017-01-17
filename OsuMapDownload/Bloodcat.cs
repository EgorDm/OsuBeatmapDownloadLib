using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload
{
    public class Bloodcat
    {
        public static readonly string BASE_URL = "http://bloodcat.com/osu/s/";

        public static string GetDownloadLink(int mapsetId)
        {
            return $"{BASE_URL}{mapsetId}";
        }
    }
}
