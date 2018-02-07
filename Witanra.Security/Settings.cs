using System.Collections.Generic;

namespace Witanra.Security
{
    class Settings
    {
        public string TempDirectory { get; set; }
        public string DateFormat { get; set; }
        public string DestinationDirectory { get; set; }
        public List<NameDir> Directories { get; set; }
        public string LogDirectory { get; set; }
    }
}
