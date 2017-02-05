using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload.Exceptions
{
    public class DownloadProviderNotReadyException : Exception
    {
        public DownloadProviderNotReadyException(string message = "DownloadProvider is not ready.") : base(message) {}
    }
}
