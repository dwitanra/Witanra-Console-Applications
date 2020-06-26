using System.Collections.Generic;

namespace Witanra.ProcessManager
{
    internal class Settings
    {
        public string LogDirectory { get; set; }
        public int IntervalInSeconds { get; set; }
        public bool Loop { get; set; }
        public List<SettingProcess> Processes { get; set; }
    }
}