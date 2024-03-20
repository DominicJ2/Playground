namespace ProcessDPK
{
    using System;
    using System.Collections.Immutable;
    using System.Dynamic;
    using System.IO.Compression;

    internal class Program
    {
        static void Main(string[] args)
        {
            string path = null;
            string extractPath = null;
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ProcessDPK <path to DPK file> <path to extract manifest>");
                return;
            }
            else
            {
                path = args[0];
                extractPath = args[1];
            }

            // Make sure path exists and is a folder
            if (!System.IO.Directory.Exists(path))
            {
                Console.WriteLine($"Directory not found: {path}");
                return;
            }
            // Create the extract path if it does not exist
            if (!System.IO.Directory.Exists(extractPath))
            {
                System.IO.Directory.CreateDirectory(extractPath);
            }


            var depots = new System.Collections.Concurrent.ConcurrentBag<string>();
            Object depotsLock = new Object();
            Object logLock = new Object();
            var filesToProcess = new System.Collections.Concurrent.ConcurrentBag<string>();
            var logPath = System.IO.Path.Combine(extractPath, "..\\depots.txt");

            // Get All files in directory ending with .dpk asynchonously that were last modified in the last 30 days in parallel
            Console.WriteLine($"{DateTime.UtcNow}: Getting files");
            IEnumerable<string> directories = Directory.EnumerateDirectories(path, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            });

            Parallel.ForEach(directories, directory =>
            {
                IEnumerable<string> files;
                try
                {
                    files = System.IO.Directory.EnumerateFiles(directory, "*.dpk");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.UtcNow}: Error getting files in {directory}: {e.Message}");
                    return;
                }
                if (!files.Any())
                {
                    return;
                }

                Console.WriteLine($"{DateTime.UtcNow}: Processing {directory}");

                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        if (fileInfo.LastWriteTime > DateTime.Now.AddDays(-30))
                        {
                            var manifestPath = System.IO.Path.Combine(extractPath, $"{System.IO.Path.GetFileNameWithoutExtension(file)}_MANIFEST");
                            // If manifestPath already exists, log and continue
                            if (System.IO.File.Exists(manifestPath))
                            {
                                Console.WriteLine($"{DateTime.UtcNow}: Manifest already exists for {file}");
                                return;
                            }

                            var zip = new System.IO.Compression.ZipArchive(System.IO.File.OpenRead(file));
                            System.IO.Compression.ZipArchiveEntry? manifest = zip.GetEntry("_MANIFEST");
                            if (manifest == null)
                            {
                                Console.WriteLine($"{DateTime.UtcNow}: No manifest found for {file}");
                                zip.Dispose();
                                return;
                            }

                            manifest.ExtractToFile(manifestPath);
                            zip.Dispose();

                            // Open the manifest file and find the line starting with "Port:"
                            var port = System.IO.File.ReadLines(manifestPath).Where(line => line.StartsWith("Port:")).FirstOrDefault();
                            if (port == null)
                            {
                                Console.WriteLine($"{DateTime.UtcNow}: No port found for {file}");
                                return;
                            }
                            var depot = port.Split("Port:")[1].Trim().ToLowerInvariant();

                            // if depot is not already in the list, add it
                            if (!depots.Contains(depot))
                            {
                                bool added = false;
                                lock (depotsLock)
                                {
                                    if (!depots.Contains(depot))
                                    {
                                        depots.Add(depot);
                                        added = true;
                                    }
                                }
                                if (added)
                                {
                                    string message = $"{DateTime.UtcNow}: {depot} - {file}{Environment.NewLine}";
                                    lock (logLock)
                                    {
                                        System.IO.File.AppendAllText(logPath, message);
                                    }
                                    Console.WriteLine($"{DateTime.UtcNow}: Added {message}");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{DateTime.UtcNow}: Error processing {file}: {e.Message}");
                    }
                });
            });
        }

    }
}