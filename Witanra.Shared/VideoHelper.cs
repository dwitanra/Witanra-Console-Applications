using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witanra.Shared
{
    public class VideoHelper
    {
        public static void MakeVideoFromImages(string SourceDirWithImages, string DestinationFile, string TempDir)
        {
            var sw = ConsoleHelper.Start("START Making Video");

            Console.WriteLine($"SourceDirWithImages:{SourceDirWithImages}");
            Console.WriteLine($"DestinationFile:{DestinationFile}");
            Console.WriteLine($"TempDir:{TempDir}");

            TempDir = Path.Combine(TempDir, Path.GetFileNameWithoutExtension(DestinationFile));
            DestinationFile = FileHelper.GetUniqueFilename(DestinationFile);

            try
            {
                Directory.Delete(TempDir, true);
            }
            catch
            {

            }
            Directory.CreateDirectory(TempDir);
            string[] files = Directory.GetFiles(SourceDirWithImages, "*.jpg");
            Array.Sort(files, StringComparer.InvariantCulture);
            for (int i = 0; i < files.Length; i++)
            {
                string oldFile = files[i];
                string newFile = TempDir + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg";
                //Console.WriteLine("Copying file from " + oldFile + " to " + newFile);
                if (new FileInfo(oldFile).Length > 0)
                    File.Copy(oldFile, newFile);
            }

            Console.WriteLine("Saving Video " + DestinationFile);
            FileHelper.LaunchCommandLineApp(TempDir, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " + DestinationFile);

            try
            {
                Directory.Delete(TempDir, true);
            }
            catch
            {

            }

            ConsoleHelper.Stop("END Making Video", sw, 0);
        }
    }
}
