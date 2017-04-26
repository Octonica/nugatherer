using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Build.Utilities;

namespace Octonica.NuGatherer
{
    internal class NuGetPackageInfo: INuGetPackageInfo
    {
        private readonly Dictionary<string, Pair> _allProperties;

        public string Id => _allProperties["id"].Value;

        public string Version => _allProperties["version"].Value;

        public string VersionOrigin => _allProperties["version"].Origin;

        public NuGetPackageInfo(string sourceFile, XElement element)
        {
            _allProperties = new Dictionary<string, Pair>(StringComparer.InvariantCulture);
            foreach (var attribute in element.Attributes())
                _allProperties.Add(attribute.Name.LocalName, new Pair(attribute.Value, sourceFile));
        }

        public void Merge(NuGetPackageInfo package, TaskLoggingHelper log)
        {
            List<string> diffs = null;
            foreach (var pair in package._allProperties)
            {
                Pair value;
                if (!_allProperties.TryGetValue(pair.Key, out value))
                {
                    _allProperties[pair.Key] = pair.Value;
                    continue;
                }

                if (StringComparer.InvariantCultureIgnoreCase.Equals(value.Value, pair.Value.Value))
                    continue;

                if (diffs == null)
                    diffs = new List<string>();

                diffs.Add($"Property: {pair.Key}; Current value (from {value.Origin}): {QuoteValue(value.Value)}; Conflicted value (from {pair.Value.Origin}): {QuoteValue(pair.Value.Value)}.");
            }

            if (diffs == null)
                return;

            var sb = new StringBuilder("Conflicts occured while merging package configurations.");
            sb = diffs.Aggregate(sb, (b, s) => b.AppendLine().Append('\t').Append(s));
            log.LogWarning(sb.ToString());
        }

        public void WriteTo(XElement element)
        {
            var ns = element.Name.NamespaceName;
            foreach (var pair in _allProperties)
            {
                element.Add(new XAttribute(XName.Get(pair.Key, ns), pair.Value.Value));
            }
        }

        private static string QuoteValue(string value)
        {
            if (value == null)
                return "NULL";
            return string.Concat("\"", value, "\"");
        }

        private struct Pair
        {
            public string Value { get; }

            public string Origin { get; }

            public Pair(string value, string origin)
            {
                Value = value;
                Origin = origin;
            }
        }
    }
}
