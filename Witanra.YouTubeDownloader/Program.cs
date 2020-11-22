using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;
using Witanra.YouTubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Witanra.YouTubeDownloader
{
    internal class Program
    {
        private static ConsoleWriter _cw;
        private static YoutubeClient _youtube;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            _youtube = new YoutubeClient();
            Console.SetOut(_cw);

            var settings = JsonConvert.DeserializeObject<Settings>(String.Join("", File.ReadAllLines("settings.json")));
            _cw.LogDirectory = settings.LogDirectory;

            Console.WriteLine($"Found { settings.Downloads.Count} Download Items in Settings");
            foreach (var download in settings.Downloads)
            {
                if (download.CleanDownloadDirectory)
                {
                    Directory.Delete(download.DownloadDirectory, true);
                }
                foreach (var youuTubeURLQueries in download.YouTubeURLQueries)
                {
                    var query = ParseQuery(youuTubeURLQueries);
                    if (query != null)
                    {
                        try
                        {
                            Console.WriteLine($"Getting {query.Type} {query.Value}...");
                            var executedQuery = ExecuteQueryAsync(query).GetAwaiter().GetResult();
                            Console.WriteLine($"Got {executedQuery.Query.Type} {executedQuery.Title}");
                            Save_Videos(executedQuery, settings, download);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Item was not processed successfully. Exception: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static void Save_Videos(ExecutedQuery executedQuery, Settings settings, SettingDownload settingDownload)
        {
            Directory.CreateDirectory(settingDownload.DownloadDirectory);

            int videoOrder = 1;
            foreach (var video in executedQuery.Videos)
            {
                var fileName = GetFileName(executedQuery, video, settingDownload, videoOrder);
                if (!File.Exists(fileName))
                {
                    try
                    {
                        Save_Media(video.Id, settings, settingDownload, fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Save_Media. Exception: {ex.Message}");
                    }
                }
                videoOrder++;
            }
        }

        private static string GetFileName(ExecutedQuery executedQuery, Video video, SettingDownload settingDownload, int videoOrder)
        {
            var result = settingDownload.FileNameTemplate;
            result = result.Replace("{playlistName}", FileHelper.GetSafeFilename(executedQuery.Title));
            result = result.Replace("{playlistNumber}", FileHelper.GetSafeFilename(videoOrder.ToString()));
            result = result.Replace("{videoTitle}", FileHelper.GetSafeFilename(video.Title));
            result = Path.Combine(settingDownload.DownloadDirectory, result);

            return result;
        }

        private static void Save_Media(string videoId, Settings settings, SettingDownload settingDownload, string filename)
        {
            var cacheDir = Path.Combine(
                settings.CacheDirectory,
                videoId);
            Directory.CreateDirectory(cacheDir);

            var streamManifest = _youtube.Videos.Streams.GetManifestAsync(videoId).GetAwaiter().GetResult();

            AudioOnlyStreamInfo audioStreamInfo;
            VideoOnlyStreamInfo videoStreamInfo;

            SelectMediaStreamInfoSet(streamManifest, settings, settingDownload, out audioStreamInfo, out videoStreamInfo);

            var audiofilename = Path.Combine(
                settings.CacheDirectory,
                videoId,
                $"{audioStreamInfo.AudioCodec}_({audioStreamInfo.Bitrate}).{audioStreamInfo.Container.Name}"
                );

            if (
                !File.Exists(audiofilename) &&
                (
                    settingDownload.MediaType == MediaType.AudioVideo ||
                    settingDownload.MediaType == MediaType.Audio
                )
               )
            {
                DownloadMediaStream(audioStreamInfo, audiofilename, settings);
            }

            var videoFilename = Path.Combine(
                settings.CacheDirectory,
                videoId,
                $"{videoStreamInfo.VideoQuality}_{videoStreamInfo.Resolution.Width}x{videoStreamInfo.Resolution.Height}@{videoStreamInfo.Framerate}_{videoStreamInfo.VideoCodec}_({videoStreamInfo.Bitrate}).{videoStreamInfo.Container.Name}"
                );

            if (
                !File.Exists(videoFilename) &&
                (
                    settingDownload.MediaType == MediaType.AudioVideo ||
                    settingDownload.MediaType == MediaType.Video
                )
               )
            {
                DownloadMediaStream(videoStreamInfo, videoFilename, settings);
            }

            try
            {
                if (settingDownload.MediaType == MediaType.AudioVideo)
                {
                    var transcode = (audioStreamInfo.Container != videoStreamInfo.Container);
                    var tempFileName = Path.Combine(settings.TempDirectory, Guid.NewGuid().ToString());

                    var args = new List<string>();
                    // Set input files
                    args.Add($"-i \"{videoFilename}\"");
                    args.Add($"-i \"{audiofilename}\"");

                    // Set output format
                    args.Add($"-f {videoStreamInfo.Container.Name}");

                    // Skip transcoding if it's not required
                    if (!transcode)
                        args.Add("-c copy");

                    // Optimize mp4 transcoding
                    if (transcode && string.Equals(videoStreamInfo.Container.Name, "mp4", StringComparison.OrdinalIgnoreCase))
                        args.Add("-preset ultrafast");

                    // Set max threads
                    args.Add($"-threads {Environment.ProcessorCount}");

                    // Disable stdin so that the process will not hang waiting for user input
                    args.Add("-nostdin");

                    // Trim streams to shortest
                    args.Add("-shortest");

                    // Overwrite files
                    args.Add("-y");

                    // Set output file
                    args.Add($"\"{tempFileName}\"");
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    Console.WriteLine($"Saving Video to {filename}...");
                    FileHelper.LaunchCommandLineApp(cacheDir, "ffmpeg.exe", string.Join(" ", args));
                    File.Move(tempFileName, filename);
                }

                if (settingDownload.MediaType == MediaType.Audio)
                {
                    File.Move(audiofilename, filename);
                }

                if (settingDownload.MediaType == MediaType.Video)
                {
                    File.Move(videoFilename, filename);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Media Move/Transcode Exception: {ex.Message}");
            }
        }

        private static void SelectMediaStreamInfoSet(StreamManifest streamManifest, Settings settings, SettingDownload settingDownload, out AudioOnlyStreamInfo audioStreamInfo, out VideoOnlyStreamInfo videoStreamInfo)
        {
            //Todo make a better selection process
            //by largest container bitrate

            audioStreamInfo = streamManifest
                        .GetAudioOnly()
                .Where(s => s.Container == Container.Mp4)
                .OrderByDescending(s => s.Bitrate)
                .First();

            videoStreamInfo = streamManifest
                    .GetVideoOnly()
               .Where(s => s.Container == Container.Mp4)
               .OrderByDescending(s => s.VideoQuality)
               .ThenByDescending(s => s.Framerate)
               .First();

            if (settingDownload.MediaType == MediaType.Audio)
            {
                audioStreamInfo = streamManifest
                        .GetAudioOnly()
                .OrderByDescending(s => s.Bitrate)
                .First();
            }

            if (settingDownload.MediaType == MediaType.Video)
            {
                videoStreamInfo = streamManifest
                     .GetVideoOnly()
                .Where(s => s.Container == Container.Mp4)
                .OrderByDescending(s => s.VideoQuality)
                .ThenByDescending(s => s.Framerate)
                .First();
            }
        }

        private static void DownloadMediaStream(IStreamInfo streamInfo, string filename, Settings settings)
        {
            var tempFileName = Path.Combine(settings.TempDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
            Directory.CreateDirectory(Path.GetDirectoryName(filename));

            Console.WriteLine($"Downloading Media to {filename}...");
            _youtube.Videos.Streams.DownloadAsync(streamInfo, tempFileName).GetAwaiter().GetResult();
            File.Move(tempFileName, filename);
        }

        private static Query ParseQuery(string query)
        {
            query = query.Trim();

            var playlistId = TryParsePlaylistId(query);
            if (playlistId != null)
            {
                return new Query(QueryType.Playlist, playlistId.Value);
            }

            var videoId = TryParseVideoId(query);
            if (videoId != null)
            {
                return new Query(QueryType.Video, videoId.Value);
            }

            var channelId = TryParseChannelId(query);
            if (channelId != null)
            {
                return new Query(QueryType.Channel, channelId);
            }

            var userName = TryParseUserName(query);
            if (userName != null)
            {
                return new Query(QueryType.User, userName.Value);
            }

            // Search
            {
                return new Query(QueryType.Search, query);
            }
        }

        private static async Task<ExecutedQuery> ExecuteQueryAsync(Query query)
        {
            // Video
            if (query.Type == QueryType.Video)
            {
                var video = await _youtube.Videos.GetAsync(query.Value);

                return new ExecutedQuery(query, video.Title, new[] { video });
            }

            // Playlist
            if (query.Type == QueryType.Playlist)
            {
                var playlist = await _youtube.Playlists.GetAsync(query.Value);
                var videos = await _youtube.Playlists.GetVideosAsync(query.Value).BufferAsync();

                return new ExecutedQuery(query, playlist.Title, videos);
            }

            // Channel
            if (query.Type == QueryType.Channel)
            {
                var channel = await _youtube.Channels.GetAsync(query.Value);
                var videos = await _youtube.Channels.GetUploadsAsync(query.Value).BufferAsync();

                return new ExecutedQuery(query, $"Channel uploads: {channel.Title}", videos);
            }

            // User
            if (query.Type == QueryType.User)
            {
                var channel = await _youtube.Channels.GetByUserAsync(query.Value);
                var videos = await _youtube.Channels.GetUploadsAsync(channel.Id).BufferAsync();
                var title = query.Value;

                return new ExecutedQuery(query, title, videos);
            }

            // Search
            if (query.Type == QueryType.Search)
            {
                var videos = await _youtube.Search.GetVideosAsync(query.Value).BufferAsync(200);
                return new ExecutedQuery(query, $"Search: {query.Value}", videos);
            }

            throw new ArgumentException($"Could not parse query '{query}'.", nameof(query));
        }

        private static VideoId? TryParseVideoId(string query)
        {
            query = query.Trim();
            try
            {
                return new VideoId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static PlaylistId? TryParsePlaylistId(string query)
        {
            try
            {
                return new PlaylistId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static ChannelId? TryParseChannelId(string query)
        {
            try
            {
                return new ChannelId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static UserName? TryParseUserName(string query)
        {
            try
            {
                // Only URLs
                if (!query.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !query.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return null;

                return new UserName(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception:" + e.ExceptionObject.ToString());
            CloseWait();
            Environment.Exit(1);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _cw.SaveToDisk();
        }
    }
}