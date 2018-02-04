using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.YouTubeUploader
{
    public class FileDetail
    {
        public string filename { get; set; }
        public long size { get; set; }
        public DateTime modified { get; set; }
        public string MD5 { get; set; }

        public FileDetail(string filename, string MD5)
        {
            FileInfo fileInfo = new FileInfo(filename);
            this.filename = filename;
            this.size = fileInfo.Length;
            this.modified = fileInfo.LastWriteTime;
            this.MD5 = MD5;
        }

        public bool IsMatch(string fileName)
        {
            FileDetail f = new FileDetail(filename, "");
            FileInfo fileInfo = new FileInfo(fileName);
            return (this.filename == f.filename && size == f.size && modified == f.modified);
        }
    }
}
