using System;
#if DOTNETCORE
using System.Runtime.InteropServices;
#endif

namespace Octonica.NuGatherer
{
    public static class PathHelper
    {
        public static readonly StringComparer Comparer;

        static PathHelper()
        {
#if DOTNETCORE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Comparer = StringComparer.OrdinalIgnoreCase;
            else
                Comparer = StringComparer.Ordinal;
#elif DOTNETFRAMEWORK
            Comparer = StringComparer.OrdinalIgnoreCase;
#else
#error Target framework is not defined.
#endif
        }
    }
}
