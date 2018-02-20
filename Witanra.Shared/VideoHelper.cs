using System;
using System.IO;

namespace Witanra.Shared
{
    public class VideoHelper
    {
        public static void MakeVideoFromImages(string SourceDirWithImages, string DestinationFile, string TempDir, bool DeleteSourceImages)
        {  
            try
            {                
                //Console.WriteLine($"Getting jpg files from {SourceDirWithImages}");
                string[] files = Directory.GetFiles(SourceDirWithImages, "*.jpg");
                if (files.Length > 0)
                {
                    TempDir = Path.Combine(TempDir, Path.GetFileNameWithoutExtension(DestinationFile));
                    DestinationFile = FileHelper.GetUniqueFilename(DestinationFile);

                    FileHelper.DeleteDirSafe(TempDir);
                    Directory.CreateDirectory(TempDir);

                    Array.Sort(files, StringComparer.InvariantCulture);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string oldFile = files[i];
                        string newFile = TempDir + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg";
                        //Console.WriteLine("Copying file from " + oldFile + " to " + newFile);
                        if (new FileInfo(oldFile).Length > 0)
                            File.Copy(oldFile, newFile);
                    }

                    Console.WriteLine("Saving Video to " + DestinationFile);
                    FileHelper.LaunchCommandLineApp(TempDir, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " + DestinationFile);

                    if (DeleteSourceImages)
                    {
                        foreach (var f in files)
                        {
                            File.Delete(f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to make video. Exception: {ex.Message}");
            }

            FileHelper.DeleteDirSafe(TempDir);

        }
    }
}
