namespace Witanra.ProcessManager
{
    public class SettingProcess
    {
        public string Name { get; set; }
        public string Priority { get; set; }
        public bool StartIfMissing { get; set; }
        public bool StopIfFound { get; set; }
        public int StopTotalSeconds { get; set; }
        public string Path { get; set; }
    }
}