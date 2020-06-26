using System.Collections.Generic;

namespace Witanra.Security
{
    internal class Settings
    {
        public string TempDirectory { get; set; }
        public List<string> DateFormats { get; set; }
        public string DestinationDirectory { get; set; }
        public List<NameDir> Directories { get; set; }
        public string LogDirectory { get; set; }
    }
}