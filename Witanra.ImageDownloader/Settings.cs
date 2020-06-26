using System.Collections.Generic;

namespace Witanra.ImageDownloader
{
    internal class Settings
    {
        public string LogDirectory { get; set; }
        public bool Loop { get; set; }
        public int IntervalInSeconds { get; set; }

        public List<ImageDownloadAction> ImageDownloadActions { get; set; }
    }
}