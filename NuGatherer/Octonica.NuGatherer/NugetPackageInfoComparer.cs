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
            var strComparer = StringComparer.OrdinalIgnoreCase;
            return strComparer.Equals(x.Id, y.Id) && strComparer.Equals(x.Version, y.Version);
        }

        public int GetHashCode(INuGetPackageInfo obj)
        {
            var strComparer = StringComparer.OrdinalIgnoreCase;
            unchecked
            {
                var hash = obj.Id == null ? 42 : strComparer.GetHashCode(obj.Id);
                hash *= 83833;
                hash ^= obj.Version == null ? 42 : strComparer.GetHashCode(obj.Version);
                return hash;
            }
        }
    }
}
