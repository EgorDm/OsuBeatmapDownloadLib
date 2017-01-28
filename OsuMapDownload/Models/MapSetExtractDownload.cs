using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload.Models
{
    public class MapSetExtractDownload : MapSetDownload
    {
        /// <summary>
        /// Has the map been succesfully read as zip?
        /// </summary>
        public virtual bool Extracted => MapHashes != null && MapHashes.Count > 0;

        public virtual List<string> MapHashes { get; set; }

        public MapSetExtractDownload(string url, string path, string name = null) : base(url, path, name)
        {
        }

        /// <summary>
        /// Reads osz file as zip and loops through all the .osu files to get their hash
        /// hashes can be found in MapHashes property
        /// </summary>
        public virtual void Extract()
        {
            //Path to osz
            var path = $"{Path}/{Name}";
            MapHashes = new List<string>();
            using (var archive = ZipFile.OpenRead(path))
            {
                //Loop through each file in archive
                foreach (var entry in archive.Entries)
                {
                    // If not .osu file continue
                    if (!entry.FullName.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)) continue;
                    using (var mapStream = entry.Open())
                    {
                        MapHashes.Add(DownloadUtils.GetHashFromStream(mapStream));
                    }
                }
            }
        }

        public override void AfterComplete()
        {
            base.AfterComplete();
            Extract();
        }
    }
}