using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Octonica.NuGatherer
{
    public class NuGathererTask : Task
    {
        public string PropertiesFile { get; set; }

        [Required]
        public ITaskItem[] RootProjects { get; set; }

        public string PackagesOutFile { get; set; }

        public override bool Execute()
        {
#if _DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            var baseDirectory = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            if (baseDirectory == null)
                Log.LogWarning("Can't resolve the root directory for the project.");

            var properies = PrepareProperties(baseDirectory);
            using (var collection = new ProjectInfoCollection(properies))
            {
                var projectPaths = new Queue<string>();
                foreach (var projFile in RootProjects)
                {
                    var filePath = GetFullPath(projFile, baseDirectory);
                    projectPaths.Enqueue(filePath);
                }

                var nugetPackages = new Dictionary<string, List<NuGetPackageInfo>>(StringComparer.InvariantCultureIgnoreCase);
                while (projectPaths.Count > 0)
                {
                    var projectInfo = collection.LoadProjectInfo(projectPaths.Dequeue());
                    if (nugetPackages.ContainsKey(projectInfo.Path))
                        continue;

                    var directory = Path.GetDirectoryName(projectInfo.Path);
                    Debug.Assert(directory != null);

                    var packages = GetPackagesInternal(directory);
                    nugetPackages.Add(projectInfo.Path, packages);

                    var project = collection.Load(projectInfo);
                    var references = project.GetItems("ProjectReference");
                    foreach (var reference in references)
                    {
                        var dir = reference.Project?.DirectoryPath;
                        if (string.IsNullOrWhiteSpace(dir))
                            dir = directory;

                        var refProjPath = GetFullPath(reference.EvaluatedInclude, dir);

                        var refProjInfo = collection.LoadProjectInfo(refProjPath);
                        refProjInfo.ReferencedFrom.Add(projectInfo.Path);

                        projectPaths.Enqueue(refProjPath);
                    }
                }

                if (!collection.ValidateNuGetPackages(nugetPackages, Log))
                    return false;

                var consolidatedPackages = Consolidate(nugetPackages.SelectMany(p => p.Value ?? Enumerable.Empty<NuGetPackageInfo>()));
                WriteConfigFile(baseDirectory, consolidatedPackages);
            }

            return true;
        }

        private Dictionary<string, string> PrepareProperties(string baseDirectory)
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCulture);

            if (string.IsNullOrWhiteSpace(PropertiesFile))
                return result;

            var propertiesFilePath = GetFullPath(PropertiesFile, baseDirectory);
            if (!File.Exists(propertiesFilePath))
            {
                Log.LogWarning(
                    $"The properties' file specified but not found.{Environment.NewLine}Path: \"{PropertiesFile}\"{Environment.NewLine}Evaluated full path: \"{propertiesFilePath}\"");
                return result;
            }

            using (var collection = new ProjectCollection(new Dictionary<string, string>()))
            {
                var project = collection.LoadProject(propertiesFilePath);
                foreach (var evaluatedProperty in project.AllEvaluatedProperties)
                {
                    if (evaluatedProperty.IsGlobalProperty || evaluatedProperty.IsEnvironmentProperty || evaluatedProperty.IsReservedProperty)
                        continue;

                    result[evaluatedProperty.Name] = evaluatedProperty.EvaluatedValue;
                }
            }

            return result;
        }

        public IEnumerable<INuGetPackageInfo> GetPackages(string directory)
        {
            return GetPackagesInternal(directory);
        }

        private List<NuGetPackageInfo> GetPackagesInternal(string directory)
        {
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
                    Log.LogError("The file '{0}' is not a valid xml file.", file);
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

        private ICollection<NuGetPackageInfo> Consolidate(IEnumerable<NuGetPackageInfo> packages)
        {
            var uniquePackages = new Dictionary<NuGetPackageInfo, NuGetPackageInfo>(NuGetPackageInfoComparer.Instance);

            foreach (var package in packages)
            {
                NuGetPackageInfo existingPackage;
                if (uniquePackages.TryGetValue(package, out existingPackage))
                    existingPackage.Merge(package, Log);
                else
                    uniquePackages.Add(package, package);
            }

            return uniquePackages.Values;
        }

        private void WriteConfigFile(string baseDirectory, ICollection<NuGetPackageInfo> consolidatedPackages)
        {
            if (string.IsNullOrWhiteSpace(PackagesOutFile))
                return;

            var path = GetFullPath(PackagesOutFile, baseDirectory);

            var dir = Path.GetDirectoryName(path);
            Debug.Assert(dir != null);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var root = new XElement("packages");
            foreach (var package in consolidatedPackages)
            {
                var element = new XElement("package");
                package.WriteTo(element);
                root.Add(element);
            }
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new XmlTextWriter(fs, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                doc.WriteTo(writer);
                writer.Flush();
            }
        }

        private static string GetFullPath(ITaskItem item, string baseDirectory)
        {
            var dir = item.GetMetadata("DefiningProjectDirectory");
            if (string.IsNullOrWhiteSpace(dir))
                dir = baseDirectory;

            return GetFullPath(item.ItemSpec, dir);
        }

        private static string GetFullPath(string path, string baseDirectory)
        {
            var fullPath = path;
            if (!Path.IsPathRooted(fullPath) && baseDirectory != null)
                fullPath = Path.Combine(baseDirectory, fullPath);

            fullPath = Path.GetFullPath(fullPath);
            return fullPath;
        }
    }
}