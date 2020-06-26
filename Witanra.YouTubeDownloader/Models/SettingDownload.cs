using System.Collections.Generic;

namespace Witanra.YouTubeDownloader.Models
{
    internal class SettingDownload
    {
        public string Name;

        public MediaType MediaType;
        public string DownloadDirectory { get; set; }
        public bool CleanDownloadDirectory { get; set; }
        public string FileNameTemplate { get; set; }
        public List<string> YouTubeURLQueries { get; set; }
    }
}