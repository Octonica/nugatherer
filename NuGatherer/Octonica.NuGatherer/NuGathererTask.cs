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
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif

            var baseDirectory = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            if (baseDirectory == null)
                Log.LogWarning("Can't resolve the root directory for the project.");

            bool isValid = true;
            var properties = PrepareProperties(baseDirectory);
            var nugetPackages = new Dictionary<string, List<NuGetPackageInfo>>(StringComparer.Ordinal);
            using (var collection = new ProjectInfoCollection(properties))
            {
                var projectsByFramework = GroupByTargetFramework(collection, baseDirectory);
                foreach (var pair in projectsByFramework)
                {
                    ProjectInfoCollection localCollection=null;
                    try
                    {
                        if (pair.Key == string.Empty)
                        {
                            localCollection = collection;
                        }
                        else
                        {
                            var localProperties = properties.ToDictionary(p => p.Key, p => p.Value, properties.Comparer);
                            localProperties["TargetFramework"] = pair.Key;
                            localCollection = new ProjectInfoCollection(localProperties);
                        }

                        isValid &= Validate(localCollection, pair.Value, nugetPackages);
                    }
                    finally
                    {
                        if (!ReferenceEquals(collection, localCollection))
                            localCollection?.Dispose();
                    }
                }

                if (!isValid)
                    return false;

                var consolidatedPackages = Consolidate(nugetPackages.SelectMany(p => p.Value ?? Enumerable.Empty<NuGetPackageInfo>()));
                WriteConfigFile(baseDirectory, consolidatedPackages);
            }

            return true;
        }

        private Dictionary<string, Queue<string>> GroupByTargetFramework(ProjectInfoCollection collection, string baseDirectory)
        {
            var result = new Dictionary<string, Queue<string>> {{string.Empty, new Queue<string>()}};
            foreach (var rootProjectItem in RootProjects)
            {
                var filePath = GetFullPath(rootProjectItem, baseDirectory);
                var proj = collection.GetOrLoadProject(filePath);

                var targetFrameworksStr = proj.GetPropertyValue("TargetFrameworks");
                string[] targetFrameworks;
                if (!string.IsNullOrWhiteSpace(targetFrameworksStr))
                {
                    targetFrameworks = targetFrameworksStr.Split(';');
                    targetFrameworks = targetFrameworks.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToArray();
                }
                else
                    targetFrameworks = new string[0];

                foreach (var targetFramework in targetFrameworks)
                {
                    Queue<string> queue;
                    if (!result.TryGetValue(targetFramework, out queue))
                        result.Add(targetFramework, queue = new Queue<string>());
                    queue.Enqueue(filePath);
                }

                result[string.Empty].Enqueue(filePath);
            }

            return result;
        }

        private bool Validate(ProjectInfoCollection collection, Queue<string> projectPaths, Dictionary<string, List<NuGetPackageInfo>> nugetPackages)
        {
            while (projectPaths.Count > 0)
            {
                var projectInfo = collection.GetOrLoad(projectPaths.Dequeue());
                if (nugetPackages.ContainsKey(projectInfo.FilePath))
                    continue;

                var directory = Path.GetDirectoryName(projectInfo.FilePath);
                Debug.Assert(directory != null);

                var packages = projectInfo.GetPackages(Log);
                nugetPackages.Add(projectInfo.FilePath, packages);

                var project = projectInfo.GetOrLoadProject();
                var references = project.GetItems("ProjectReference");
                foreach (var reference in references)
                {
                    var dir = reference.Project?.DirectoryPath;
                    if (string.IsNullOrWhiteSpace(dir))
                        dir = directory;

                    var refProjPath = GetFullPath(reference.EvaluatedInclude, dir);

                    var refProjInfo = collection.GetOrLoad(refProjPath);
                    refProjInfo.ReferencedFrom.Add(projectInfo.FilePath);

                    projectPaths.Enqueue(refProjPath);
                }
            }

            return collection.ValidateNuGetPackages(nugetPackages, Log);
        }

        private Dictionary<string, string> PrepareProperties(string baseDirectory)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

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
            using (var writer = XmlWriter.Create(fs, new XmlWriterSettings {Encoding = Encoding.UTF8, Indent = true}))
            {
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