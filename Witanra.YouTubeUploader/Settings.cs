using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.YouTubeUploader
{
    class Settings
    {
        public string cacheFile { get; set; }
        public Guid program_guid { get; set; }
        public string folder { get; set; }
        public List<string> fileTypes { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public string category { get; set; }
        public string privacyStatus { get; set; }
        public string playlistTitle { get; set; }
        public string playlistDescription { get; set; }

        public int uploadLimit { get; set; }
    }
}
