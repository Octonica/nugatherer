using System;
using System.Runtime.InteropServices;

namespace Octonica.NuGatherer
{
    public static class PathHelper
    {
        public static readonly StringComparer Comparer;

        static PathHelper()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Comparer = StringComparer.OrdinalIgnoreCase;
            else
                Comparer = StringComparer.Ordinal;
        }
    }
}
