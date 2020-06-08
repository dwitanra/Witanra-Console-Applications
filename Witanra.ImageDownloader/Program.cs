using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;

namespace Witanra.ImageDownloader
{
    class Program
    {
        private static ConsoleWriter _cw;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
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
                            if (!String.IsNullOrWhiteSpace(action.Source_URL_Username))
                            {
                                client.Credentials = new NetworkCredential(action.Source_URL_Username, action.Source_URL_Password);
                            }
                            client.DownloadFileAsync(new Uri(action.Source_URL), action.Destination_Folder + action.Destination_FileName);
                            //Console.WriteLine($"Saved {action.Destination_Folder + action.Destination_FileName}");
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
