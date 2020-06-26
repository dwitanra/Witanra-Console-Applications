using System.Collections.Generic;

namespace Witanra.YouTubeDownloader.Models
{
    internal class Settings
    {
        public string LogDirectory { get; set; }
        public string TempDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public List<SettingDownload> Downloads { get; set; }
    }
}