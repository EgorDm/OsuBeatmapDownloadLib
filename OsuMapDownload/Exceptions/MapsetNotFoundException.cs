using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload.Exceptions
{
    public class MapsetNotFoundException : Exception
    {
        public string Url { get; protected set; }

        public override string Message => $"Mapset could not be found! Download Url: {Url}";

        public MapsetNotFoundException(string url) : base()
        {
            Url = url;
        }
    }
}
