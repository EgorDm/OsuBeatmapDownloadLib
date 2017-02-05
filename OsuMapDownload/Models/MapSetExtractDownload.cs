using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuMapDownload.Models
{
    public class MapsetExtractDownload : MapsetDownload
    {
        public virtual List<string> MapHashes { get; set; }

        public MapsetExtractDownload(int id, string path, BeatmapDownloadProvider provider) : base(id, path, provider) {}

        protected override void AfterDownload() {
            base.AfterDownload();
            Status = MapsetDownloadStatus.Extracting;
            Extract();
        }

        /// <summary>
        /// Reads osz file as zip and loops through all the .osu files to get their hash
        /// hashes can be found in MapHashes property
        /// </summary>
        public virtual void Extract() {
            //Path to osz
            var path = $"{Path}/{FileName}";
            MapHashes = new List<string>();
            using (var archive = ZipFile.OpenRead(path)) {
                //Loop through each file in archive
                foreach (var entry in archive.Entries) {
                    // If not .osu file continue
                    if (!entry.FullName.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)) continue;
                    using (var mapStream = entry.Open()) {
                        MapHashes.Add(DownloadUtils.GetHashFromStream(mapStream));
                    }
                }
            }
        }
    }
}