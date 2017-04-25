namespace Octonica.NuGatherer.Tests
{
    internal class TestNuGetPackageInfo : INuGetPackageInfo
    {
        public string Id { get; }

        public string Version { get; }

        public TestNuGetPackageInfo(string id, string version)
        {
            Id = id;
            Version = version;
        }
    }
}
