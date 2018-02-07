using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.DirectoryCleanup
{
    class Settings
    {
        public string Directory { get; set; }
        public long TargetDirSize { get; set; }
        public int MinDaysOld { get; set; }
        public bool DoDelete { get; set; }
    }
}
