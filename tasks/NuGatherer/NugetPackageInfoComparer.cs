using System;
using System.Collections.Generic;

namespace Octonica.NuGatherer
{
    public class NuGetPackageInfoComparer : IEqualityComparer<INuGetPackageInfo>
    {
        public static readonly NuGetPackageInfoComparer Instance = new NuGetPackageInfoComparer();

        private NuGetPackageInfoComparer()
        {
        }

        public bool Equals(INuGetPackageInfo x, INuGetPackageInfo y)
        {
            var strComparer = StringComparer.InvariantCultureIgnoreCase;
            return strComparer.Equals(x.Id, y.Id) && strComparer.Equals(x.Version, y.Version);
        }

        public int GetHashCode(INuGetPackageInfo obj)
        {
            unchecked
            {
                var hash = obj.Id?.GetHashCode() ?? 42;
                hash *= 83833;
                hash ^= obj.Version?.GetHashCode() ?? 42;
                return hash;
            }
        }
    }
}
