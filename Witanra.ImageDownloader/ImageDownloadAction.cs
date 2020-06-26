using System.Collections.Generic;

namespace Witanra.ImageDownloader
{
    public class ImageDownloadAction
    {
        public string Source_URL { get; set; }
        public List<string> Destination_FileNames { get; set; }
        public string Source_URL_Username { get; set; }
        public string Source_URL_Password { get; set; }
    }
}