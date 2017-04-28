using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Octonica.NuGatherer
{
    internal class ProjectInfo
    {
        private readonly ProjectCollection _collection;
        private Project _project;

        public string FilePath { get; }

        public HashSet<string> ReferencedFrom { get; }

        public ProjectInfo(string path, ProjectCollection collection)
        {
            FilePath = Path.GetFullPath(path);
            ReferencedFrom = new HashSet<string>(PathHelper.Comparer);
            _collection = collection;
        }

        public Project GetOrLoadProject()
        {
            if (_project != null)
                return _project;

            _project = _collection.LoadProject(FilePath);
            return _project;
        }

        public List<NuGetPackageInfo> GetPackages(TaskLoggingHelper log)
        {
            var proj = GetOrLoadProject();
            var restoreStyle = proj.GetPropertyValue("RestoreProjectStyle");
            switch (restoreStyle)
            {
                case "PackageReference":
                    return GetPackagesFromProject(log);

                default:
                    return GetPackagesFromConfig(log);
            }
        }

        private List<NuGetPackageInfo> GetPackagesFromProject(TaskLoggingHelper log)
        {
            var proj = GetOrLoadProject();

            var packageRefs = proj.GetItems("PackageReference");
            var result = new List<NuGetPackageInfo>(packageRefs.Count);
            foreach (var packageRef in packageRefs)
            {
                var include = packageRef.EvaluatedInclude;
                var version = packageRef.GetMetadataValue("Version");

                var packageInfo = new NuGetPackageInfo(FilePath, include, version);
                result.Add(packageInfo);
            }

            if (result.Count == 0)
                return null;

            return result;
        }

        private List<NuGetPackageInfo> GetPackagesFromConfig(TaskLoggingHelper log)
        {
            var directory = Path.GetDirectoryName(FilePath);
            Debug.Assert(directory != null);

            var file = Path.Combine(directory, "packages.config");
            if (!File.Exists(file))
                return null;

            XDocument doc;
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    doc = XDocument.Load(fs);
                }
                catch
                {
                    log.LogError("The file '{0}' is not a valid xml file.", file);
                    return null;
                }
            }

            var root = doc.Root;
            if (root == null || root.Name.LocalName != "packages")
                return null;

            var result = new List<NuGetPackageInfo>();
            foreach (var package in root.Elements(XName.Get("package", root.Name.NamespaceName)))
                result.Add(new NuGetPackageInfo(file, package));

            if (result.Count == 0)
                return null;

            return result;
        }
    }
}
