using System.Collections.Generic;

namespace Witanra.Security
{
    internal class Settings
    {
        public string LogDirectory { get; set; }
        public string TempDirectory { get; set; }
        public List<string> DateFormats { get; set; }
        public string SourceDataDirectory { get; set; }
        public string DestinationSummaryDirectory { get; set; }
        public int MinImagesToMakeVideo { get; set; }
        public bool ExcludeToday { get; set; }
        public bool DeleteImages { get; set; }
    }
}