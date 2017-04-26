using System.Collections.Generic;

namespace Octonica.NuGatherer
{
    internal class ProjectInfo
    {
        public string Path { get; }

        public HashSet<string> ReferencedFrom { get; }

        public ProjectInfo(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
            ReferencedFrom = new HashSet<string>(PathHelper.Comparer);
        }
    }
}
