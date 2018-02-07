using System.Collections.Generic;

namespace Witanra.Security
{
    class Settings
    {
        public string TempDir { get; set; }
        public string DateFormat { get; set; }
        public string DestinationDir { get; set; }
        public List<NameDir> Folders { get; set; }
        public string LogDirectory { get; set; }
    }
}
