using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AutoResizer
{
    internal static class Program
    {
        private static FileSystemWatcher Watcher { get; set; }
        private static string InputDir { get; set; }

        private static string OutputDir
        {
            get { return string.Format(@"{0}output\", Regex.Match(Application.ExecutablePath, @"(.*\\)[^\\]+$").Groups[1].Value); }
        }

        private static string ProgDir
        {
            get { return string.Format(@"{0}ffmpeg\", Regex.Match(Application.ExecutablePath, @"(.*\\)[^\\]+$").Groups[1].Value); }
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(@"Usage: autoresizer <inputdir>");
            }
            else
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine(@"Usage: autoresizer <inputdir>");
                }
                else
                {
                    InputDir = args[0];
                    Watcher = new FileSystemWatcher
                        {
                            Path = InputDir,
                            Filter = "*.*",
                            IncludeSubdirectories = false,
                            NotifyFilter = NotifyFilters.LastAccess |
                                           NotifyFilters.LastWrite |
                                           NotifyFilters.FileName |
                                           NotifyFilters.DirectoryName
                        };

                    Watcher.Created += FileSystemWatcher_EHandler;
                    Watcher.Changed += FileSystemWatcher_EHandler;

                    Watcher.EnableRaisingEvents = true;
                    Console.WriteLine(@"Type quit to end...");
                    bool doThis = true;
                    do
                    {
                        Thread.Sleep(1);
                        Console.Write(@"autoresizer> ");
                        var readline = Console.ReadLine();
                        Debug.Assert(readline != null, "readline != null");
                        if (!string.IsNullOrWhiteSpace(readline) && readline.Trim().Equals("quit"))
                        {
                            doThis = false;
                        }
                    } while (doThis) ;
                }
            }
        }

        private static void FileSystemWatcher_EHandler(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            var filename = Regex.Match(InputDir + e.FullPath, @".*\\([^\\]+$)").Groups[1].Value;
            Console.WriteLine("converting : " + filename);
            var command = string.Format("{2}ffmpeg.exe -i \"{3}{0}\" -s 80x80 \"{1}t_{0}\"", filename, OutputDir, ProgDir, InputDir);
            Process process = new Process
                {
                    StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = string.Format(@"cmd"),
                            Arguments = string.Format(@"/c {0}", command)
                        }
                };
            process.Start();
            process.WaitForExit();
            Console.Write(@"autoresizer> ");
        }
    }
}