using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Witanra.Shared;

namespace Witanra.YouTubeUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            Console.WriteLine("==============================");

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");

            try
            {
                Console.WriteLine("Getting Video list...");
                var YouTubeVideos = YouTube.GetMyUploadsAsync("snippet").Result;
                Console.WriteLine($"Found {YouTubeVideos.Count} Videos.");
                //foreach (var playlistItem in YouTubeVideos)
                //{
                //    Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                //}

                Console.WriteLine("Getting Playlist list...");
                var YouTubePlaylist = YouTube.GetPlaylistsAsync("snippet").Result;


                Console.WriteLine($"Getting Files in {settings.folder}...");
                var files = new List<string>(Directory.GetFiles(settings.folder, "*.*", SearchOption.AllDirectories));

                Console.WriteLine($"Found {files.Count} Files.");

                for (int i = files.Count - 1; i >= 0; i--)
                {
                    if (!settings.fileTypes.Contains(Path.GetExtension(files[i])))
                    {
                        files.RemoveAt(i);
                    }
                }
                Console.WriteLine($"Found {files.Count} Video Files.");

                var fileCache = LoadFileCacheList(settings.cacheFile);

                for (int i = files.Count - 1; i >= 0; i--)
                {
                    var fileDetail = FindFileDetail(fileCache, files[i]);

                    if (fileDetail == null)
                    {
                        fileDetail = new FileDetail(files[i], GetMD5(files[i]));
                        fileCache.Add(fileDetail);
                    }
                    else
                    {
                        if (!fileDetail.IsMatch(files[i]))
                        {
                            fileCache.Remove(fileDetail);

                            fileDetail = new FileDetail(files[i], GetMD5(files[i]));                            
                            fileCache.Add(fileDetail);
                        }
                    }

                    foreach (var upload in YouTubeVideos)
                    {
                        if (upload.Snippet.Description.Contains(fileDetail.MD5))
                        {
                            files.RemoveAt(i);
                            break;
                        }
                    }
                }

                SaveFileCacheList(fileCache, settings.cacheFile);

                Console.WriteLine($"Found {files.Count} Video Files that need to be uploaded.");

                for (int i = 0; i < files.Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"Uploading {i + 1} of {files.Count} : {files[i]} ...");
                        FileInfo fileInfo = new FileInfo(files[i]);
                        FileDetail fileDetail = FindFileDetail(fileCache, files[i]);

                        //Console.WriteLine(ReplaceVariables(settings.title, fileInfo, settings.program_guid, fileDetail.MD5));
                        //Console.WriteLine(ReplaceVariables(settings.description, fileInfo, settings.program_guid, fileDetail.MD5));
                        //foreach (var s in settings.tags)
                        //    Console.WriteLine(ReplaceVariables(s, fileInfo, settings.program_guid, fileDetail.MD5));

                        Task<string> t = Task<string>.Run(() => YouTube.AddVideoAsync(
                            ReplaceVariables(settings.title, fileInfo, settings.program_guid, fileDetail.MD5),
                            ReplaceVariables(settings.description, fileInfo, settings.program_guid, fileDetail.MD5),
                            ReplaceVariables(settings.tags, fileInfo, settings.program_guid, fileDetail.MD5),
                            settings.category,
                            YouTube.PrivacyStatus_Private,
                            files[i],
                            ProgressChanged,
                            ResponseReceived
                            ).Result
                        );
                        t.Wait();
                        Console.WriteLine($"Uploaded video {t.Result}");


                        var playlistId = String.Empty;
                        var playlistTitle = ReplaceVariables(settings.playlistTitle, fileInfo, settings.program_guid, fileDetail.MD5);
                        Console.WriteLine($"Finding playlist {playlistTitle}...");

                        foreach (var playlist in YouTubePlaylist)
                        {
                            if (playlist.Snippet.Title == playlistTitle)
                            {
                                playlistId = playlist.Id;
                                break;
                            }
                        }

                        if (playlistId == String.Empty)
                        {
                            playlistId = YouTube.AddPlaylistAsync(playlistTitle, ReplaceVariables(settings.playlistDescription, fileInfo, settings.program_guid, fileDetail.MD5), settings.privacyStatus).Result;
                            YouTubePlaylist = YouTube.GetPlaylistsAsync("snippet").Result;
                        }

                        YouTube.AddVideoToPlaylist(playlistId, t.Result);
                        Console.WriteLine($"Video {t.Result} added to playlist {playlistId}...");
                    }

                    catch (AggregateException ex)
                    {
                        PrintAggregateException(ex);
                    }
                }
            }
            catch (AggregateException ex)
            {
                PrintAggregateException(ex);
            }        
        }

        private static void PrintAggregateException(AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static List<string> ReplaceVariables(List<string> str, FileInfo file, Guid guid, string MD5)
        {
            for (int i = 0; i < str.Count; i++)
            {
                str[i] = ReplaceVariables(str[i], file, guid, MD5);

            }
            return str;
        }

        private static string ReplaceVariables(string str, FileInfo file, Guid guid, string MD5)
        {
            str = str.Replace("{program_name}", System.AppDomain.CurrentDomain.FriendlyName);
            str = str.Replace("{program_version}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            str = str.Replace("{program_guid}", guid.ToString());

            str = str.Replace("{computer_name}", Environment.MachineName);

            str = str.Replace("{file}", file.Name);
            str = str.Replace("{filepath}", file.FullName);
            str = str.Replace("{filesize}", file.Length.ToString());
            str = str.Replace("{fileDateCreated}", file.CreationTime.ToString());
            str = str.Replace("{fileDateCreated_year}", file.CreationTime.ToString("yyyy"));
            str = str.Replace("{fileDateCreated_month}", file.CreationTime.ToString("MM"));
            str = str.Replace("{fileDateCreated_day}", file.CreationTime.ToString("dd"));
            str = str.Replace("{fileDateModified}", file.LastWriteTime.ToString());
            str = str.Replace("{fileDateModified__year}", file.LastWriteTime.ToString("yyyy"));
            str = str.Replace("{fileDateModified__month}", file.LastWriteTime.ToString("MM"));
            str = str.Replace("{fileDateModified_day}", file.LastWriteTime.ToString("dd"));
            str = str.Replace("{uploaded_date}", DateTime.Now.ToShortDateString());
            str = str.Replace("{uploaded_time}", DateTime.Now.ToShortTimeString());
            str = str.Replace("{uploaded_date_year}", DateTime.Now.ToString("yyyy"));
            str = str.Replace("{uploaded_date_month}", DateTime.Now.ToString("MM"));
            str = str.Replace("{uploaded_date_day}", DateTime.Now.ToString("dd"));
            str = str.Replace("{MD5}", MD5);

            return str;
        }

        private static List<FileDetail> LoadFileCacheList(string filename)
        {
            var result = new List<FileDetail>();

            try
            {
                result = JsonHelper.DeserializeFile<List<FileDetail>>(filename);
            }
            catch
            {
                Console.WriteLine("CacheList not valid, Regnerating...");
                result = new List<FileDetail>();
            }

            return result;
        }

        private static void SaveFileCacheList(List<FileDetail> list, string filename)
        {
            JsonHelper.SerializeFile(list, filename);
        }

        private static FileDetail FindFileDetail(List<FileDetail> fileCache, string filename)
        {
            FileDetail result = null;
            foreach (var f in fileCache)
            {
                if (f.filename == filename)
                {
                    result = f;
                    break;
                }
            }
            return result;
        }

        private static string GetMD5(string filename)
        {
            Console.WriteLine($"Generating MD5 for {filename} ...");
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine($"{progress.BytesSent} bytes sent.");
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{progress.Exception}");
                    break;
            }
        }

        private static void ResponseReceived(Video video)
        {
            Console.WriteLine($"Video id '{video.Id}' was successfully uploaded.");

        }
    }
}
