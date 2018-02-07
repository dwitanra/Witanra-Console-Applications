using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.DirectoryCleanup
{
    class Settings
    {
        public string LogDirectory { get; set; }
        public List<DirectoryToCleanup> Directories { get; set; }
    }
}
