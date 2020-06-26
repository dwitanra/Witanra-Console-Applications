using System.Collections.Generic;

namespace Witanra.DirectoryCleanup
{
    internal class Settings
    {
        public string LogDirectory { get; set; }
        public List<DirectoryToCleanup> Directories { get; set; }
    }
}