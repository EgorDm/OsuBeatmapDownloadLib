using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload.Exceptions
{
    public class MapsetDownloadInterrupedException : Exception
    {
        public MapsetDownloadInterrupedException(Exception innerException) : base("Download has been interruped.", innerException) {}
    }
}