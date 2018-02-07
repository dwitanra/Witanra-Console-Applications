namespace Witanra.DirectoryCleanup
{
    class DirectoryToCleanup
    {
        public string Directory { get; set; }
        public long TargetDirSize { get; set; }
        public int MinDaysOld { get; set; }
        public bool DoDelete { get; set; }
    }
}
