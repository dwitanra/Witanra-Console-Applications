using System;
using System.IO;
using System.Text;

namespace Witanra.Shared
{
    public class ConsoleWriter : TextWriter
    {
        private TextWriter _originalOut;
        private StringBuilder _sb;
        private string _logDirectory;
        private string _consoleDateTimeFormat;
        private string _logFileDateTimeFormat;
        public bool DoStart { get; set; }

        public string LogDirectory
        {
            get
            {
                return _logDirectory;
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _logDirectory = value;
                }
            }
        }

        public string ConsoleDateTimeFormat
        {
            get
            {
                return _consoleDateTimeFormat;
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _consoleDateTimeFormat = value;
                }
            }
        }

        public string LogFileDateTimeFormat
        {
            get
            {
                return _logFileDateTimeFormat;
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _logFileDateTimeFormat = value;
                }
            }
        }

        public ConsoleWriter(bool DoIntroduction = true)
        {
            //set defaults
            LogDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ConsoleDateTimeFormat = "HH:mm:ss.ffff";
            //LogFileDateTimeFormat = "-yyyyMMdd-HHmmss-ffff";
            LogFileDateTimeFormat = "-yyyyMMdd";

            _originalOut = Console.Out;
            _sb = new StringBuilder();

            if (DoIntroduction)
            {
                var s1 = $"{System.AppDomain.CurrentDomain.FriendlyName} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
                var s2 = "";
                for (int i = 0; i < s1.Length; i++)
                    s2 += '=';
                WriteLine(s1);
                WriteLine(s2);

                WriteLine($"Application Path: {System.Reflection.Assembly.GetEntryAssembly().Location}");
                WriteLine($"Right now it is: {DateTime.Now.ToString("F") }");
                WriteLine("");
            }
        }

        public override Encoding Encoding
        {
            get { return new ASCIIEncoding(); }
        }

        public override void WriteLine(string message)
        {
            var s = $"{DateTime.Now.ToString(_consoleDateTimeFormat)} {message}";
            _originalOut.WriteLine(s);
            _sb.AppendLine(s);
        }

        public override void Write(string message)
        {
            _originalOut.Write(message);
            _sb.Append(message);
        }

        public void SaveToDisk()
        {
            try
            {
                if (_sb.Length > 0)
                {
                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);

                    var f = Path.Combine(LogDirectory, AppDomain.CurrentDomain.FriendlyName + DateTime.Now.ToString(_logFileDateTimeFormat) + ".log");

                    Console.WriteLine($"Console Output saved to: {f}");
                    File.AppendAllText(f, _sb.ToString());
                    _sb.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}