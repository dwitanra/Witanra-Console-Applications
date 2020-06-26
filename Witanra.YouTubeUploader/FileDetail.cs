using System;
using System.IO;

namespace Witanra.YouTubeUploader
{
    public class FileDetail
    {
        public string Filename { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public string MD5 { get; set; }
        public int TryCount { get; set; }

        public FileDetail()
        {
        }

        public FileDetail(FileInfo fileInfo, string MD5)
        {
            this.Filename = fileInfo.FullName;
            this.Size = fileInfo.Length;
            this.Modified = fileInfo.LastWriteTime;
            this.Created = fileInfo.CreationTime;
            this.MD5 = MD5;
        }

        public bool IsMatch(FileInfo fileInfo)
        {
            FileDetail f = new FileDetail(fileInfo, "");
            return (this.Filename == f.Filename && Size == f.Size && Modified == f.Modified && Created == f.Created);
        }
    }
}