using System.Collections.Generic;

namespace Witanra.OrganizeByDate
{
    internal class Settings
    {
        public string DateFormat { get; set; }
        public string LogDirectory { get; set; }
        public List<DirectoryPair> DirectoryPairs { get; set; }
    }
}