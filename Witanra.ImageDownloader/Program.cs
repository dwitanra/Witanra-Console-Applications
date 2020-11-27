using System;
using System.IO;
using System.Net;
using System.Threading;
using Witanra.Shared;

namespace Witanra.ImageDownloader
{
    internal class Program
    {
        private static ConsoleWriter _cw;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            var settingsFile = "settings.json";
            if (args != null && args.Length > 0)
            {
                settingsFile = args[0];
            }

            var settings = JsonHelper.DeserializeFile<Settings>(settingsFile);
            _cw.LogDirectory = settings.LogDirectory;

            Console.WriteLine($"Found { settings.ImageDownloadActions.Count} Image Download Actions in Settings");

            while (true)
            {
                foreach (var action in settings.ImageDownloadActions)
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            byte[] data = null;
                            if (!String.IsNullOrWhiteSpace(action.Source_URL_Username))
                            {
                                client.Credentials = new NetworkCredential(action.Source_URL_Username, action.Source_URL_Password);
                            }
                            client.DownloadDataCompleted +=
                                delegate (object sender, DownloadDataCompletedEventArgs e)
                                {
                                    data = e.Result;
                                    foreach (var filename in action.Destination_FileNames)
                                    {
                                        var actualFileName = ReplaceVariables(filename);
                                        var directory = Path.GetDirectoryName(actualFileName);
                                        Directory.CreateDirectory(directory);
                                        File.WriteAllBytes(actualFileName, data);
                                        Console.WriteLine($"Saved {actualFileName}");
                                    }
                                };
                            var uri = new Uri(action.Source_URL);
                            Console.WriteLine($"Downloading {uri.Host}...");
                            client.DownloadDataAsync(uri);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Image Download Action {action.Source_URL} was not processes successfully. Exception: {ex.Message}");
                    }
                }

                Console.WriteLine($"Waiting {settings.IntervalInSeconds} seconds...");
                Thread.Sleep(settings.IntervalInSeconds * 1000);

                if (!settings.Loop)
                {
                    Console.WriteLine($"Not Looping");
                    break;
                }
                _cw.SaveToDisk();
            }

            CloseWait();
        }

        private static string ReplaceVariables(string StringVariable)
        {
            var now = DateTime.Now;
            StringVariable = StringVariable.Replace("{year}", now.ToString("yyyy"));
            StringVariable = StringVariable.Replace("{month}", now.ToString("MM"));
            StringVariable = StringVariable.Replace("{day}", now.ToString("dd"));
            StringVariable = StringVariable.Replace("{hour}", now.ToString("HH"));
            StringVariable = StringVariable.Replace("{minute}", now.ToString("mm"));
            StringVariable = StringVariable.Replace("{second}", now.ToString("ss"));
            StringVariable = StringVariable.Replace("{date}", now.ToString("yyyyMMdd-HHmmssffff"));

            return StringVariable;
        }

        private static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 5 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(5000);
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