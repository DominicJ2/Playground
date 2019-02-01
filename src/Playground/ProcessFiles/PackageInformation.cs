namespace ProcessFiles
{
    public class PackageInformation
    {
        public PackageInformation(string packageId, string packageVersion)
        {
            PackageId = packageId;
            PackageVersion = packageVersion;
        }

        public string PackageId { get; }
        public string PackageVersion { get; }
    }
}
