using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static System.String;

namespace NetCorePublisher
{
    static class Program
    {
        private static string _targetPath = Empty;
        private static string _projectPath = Empty;
        private static bool _singleFile = true;
        private static string[] _publishTargets;

        static void Main(string[] args)
        {
            _publishTargets = new string[] {
                "win-x86",
                "win-x64",
                "win-arm",
                "osx-x64",
                "linux-x64",
                "linux-arm"
            };

            ReadOptions();

            PublishProject();

            ZipProjects();
        }

        private static void PublishProject()
        {
            foreach (var target in _publishTargets)            
                PublishProject(target);
        }

        private static void PublishProject(string target)
        {
            try
            {
                string output = Path.Combine(_targetPath, target);

                Console.WriteLine($"Publishing {target} into {output}");

                string singleFile = _singleFile ? "/p:PublishSingleFile=true" : "";
                RunCommand($"publish -r {target} -o {output} {singleFile}");

                Console.WriteLine($"Finished publishing {target}.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error publishing {target}. Message: {e.Message}");
            }
        }

        private static void RunCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $" {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _projectPath                    
                }
            };

            process.Start();

            process.WaitForExit();
        }

        private static void ZipProjects()
        {
            Parallel.ForEach(_publishTargets, target =>
            {
                try
                {
                    ZipProject(target);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error zipping {target}. Message: {e.Message}");
                }
            });
        }
        private static void ZipProject(string target)
        {

            string zipPathSource = Path.Combine(_targetPath, target);
            string zipPathOutput = Path.Combine(_targetPath, $"{target}.zip");

            Console.WriteLine($"Zipping {target} into {zipPathOutput}");

            ZipFile.CreateFromDirectory(zipPathSource, zipPathOutput);

            Console.WriteLine($"Finished zipping {target}.");
        }

        private static void ReadOptions()
        {

            Console.Write("Enter the path to publish projects:");
            _targetPath = HandlePath(Console.ReadLine());

            if (IsNullOrWhiteSpace(_targetPath))
            {
                throw new ArgumentNullException(_targetPath, "Invalid path");
            }

            Console.Write("Enter the project path:");
            _projectPath = HandlePath(Console.ReadLine());

            if (IsNullOrWhiteSpace(_projectPath))
            {
                throw new ArgumentNullException(_projectPath, "Invalid path");
            }

            Console.Write("Publish in single file (Y/N)?:");
            string singleFileOption = Console.ReadKey().KeyChar.ToString();

            if (singleFileOption.ToLower() == "n")
                _singleFile = false;

        }

        private static string HandlePath(string path)
        {
            return path?.Replace('\\', '/').Replace(":\\", "://");
        }
    }
}
