using System;
using System.Diagnostics;
using System.Threading;

namespace Witanra.Shared
{
    public class ConsoleHelper
    {
        public static Stopwatch Start(string StartMessage)
        {
            WriteLine(StartMessage);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            return stopWatch;
        }

        public static void Stop(string EndMessage, Stopwatch stopwatch, int milliSecondsToWait = 0)
        {
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

            WriteLine($"Done! Took {elapsedTime}");

            WriteLine(EndMessage);
            WriteLine("");
            WriteLine("");
            WriteLine("");
            Thread.Sleep(milliSecondsToWait);
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
