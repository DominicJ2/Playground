namespace ProcessFiles
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class FileProcessor
    {
        private const string packageRegex = @"<Package>\s*<Id>(.*)</Id>\s*<Version>(.*)</Version>\s*</Package>";

        public void ProcessPackages(string pathToProcess)
        {
            if (string.IsNullOrWhiteSpace(pathToProcess))
            {
                throw new ArgumentException("You must pass a path to process", nameof(pathToProcess));
            }

            if (!System.IO.Directory.Exists(pathToProcess))
            {
                throw new ArgumentException($"Path to process does not exist: [{pathToProcess}]", nameof(pathToProcess));
            }

            IEnumerable<string> files = System.IO.Directory.EnumerateFiles(pathToProcess);

            ConcurrentBag<PackageInformation> packages = new ConcurrentBag<PackageInformation>();
            
            Parallel.ForEach(files, async (file) =>
            {
                string content = await System.IO.File.ReadAllTextAsync(file);
                Match t = Regex.Match(content, packageRegex);
                if (!t.Success)
                {
                    Console.Error.WriteLine($"Failed to match content in file: [{file}]");
                    return;
                }

                if (t.Groups.Count != 3)
                {
                    Console.Error.WriteLine($"Only {t.Groups.Count} found, expect 3");
                    return;
                }

                string packageId = t.Groups[1].Value;
                string packageVersion = t.Groups[2].Value;

                packages.Add(new PackageInformation(packageId, packageVersion));
            });

            var uniquePackagesSorted = packages.Distinct().OrderBy(p => p.PackageId).ThenBy(p => p.PackageVersion);

            foreach (var packageInformation in uniquePackagesSorted)
            {
                Console.WriteLine($"Package ID: {packageInformation.PackageId}\tPackage Version: {packageInformation.PackageVersion}");
            }
        }
    }
}
