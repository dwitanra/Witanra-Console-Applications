using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.YouTubeUploader
{
    class Settings
    {
        public string CacheFile { get; set; }
        public Guid Program_Guid { get; set; }
        public string Directory { get; set; }
        public List<string> FileNameIgnore { get; set; }
        public List<string> FileTypes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Category { get; set; }
        public string PrivacyStatus { get; set; }
        public string PlaylistTitle { get; set; }
        public string PlaylistDescription { get; set; }

        public int UploadLimitCount { get; set; }
        public long UploadLimitSize { get; set; }
        public int UploadFileTryCount { get; set; }

        public string LogDirectory { get; set; }
    }
}
