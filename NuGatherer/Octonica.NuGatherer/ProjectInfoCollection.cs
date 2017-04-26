using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;

namespace Octonica.NuGatherer
{
    internal class ProjectInfoCollection: IDisposable
    {
        private readonly ProjectCollection _projectCollection;

        private readonly Dictionary<string, ProjectInfo> _projects = new Dictionary<string, ProjectInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Project> _loadedProject = new Dictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        public ProjectInfoCollection(Dictionary<string, string> properties)
        {
            _projectCollection = new ProjectCollection(properties);
        }

        public ProjectInfo LoadProjectInfo(string path)
        {
            var project = new ProjectInfo(path);
            ProjectInfo existingProject;
            if (_projects.TryGetValue(project.Path, out existingProject))
                return existingProject;

            _projects.Add(project.Path, project);
            return project;
        }

        public Project Load(ProjectInfo projectInfo)
        {
            Project project;
            if (!_loadedProject.TryGetValue(projectInfo.Path, out project))
                _loadedProject.Add(projectInfo.Path, project = _projectCollection.LoadProject(projectInfo.Path));

            return project;
        }

        public void Dispose()
        {
            _projectCollection.Dispose();
        }

        public bool ValidateNuGetPackages(IDictionary<string, List<NuGetPackageInfo>> nugetPackages, TaskLoggingHelper log)
        {
            var rootMap = new Dictionary<string, HashSet<string>>(_projects.Comparer);
            foreach (var root in _projects.Values.Where(v => v.ReferencedFrom.Count == 0).Select(p => p.Path))
                rootMap.Add(root, new HashSet<string>(rootMap.Comparer));

            foreach (var project in _projects.Values)
            {
                var projectRoots = new HashSet<string>(rootMap.Comparer);
                GatherRoots(project, projectRoots);
                foreach (var root in projectRoots)
                    rootMap[root].Add(project.Path);
            }

            var packageComparer = NuGetPackageInfoComparer.Instance;
            bool isValid = true;
            foreach (var pair in rootMap)
            {
                var uniquePackages = new Dictionary<string, NuGetPackageInfo>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var project in pair.Value)
                {
                    List<NuGetPackageInfo> packages;
                    if (!nugetPackages.TryGetValue(project, out packages) || packages == null)
                        continue;

                    foreach (var package in packages)
                    {
                        NuGetPackageInfo existingPackage;
                        if (uniquePackages.TryGetValue(package.Id, out existingPackage))
                        {
                            if (!packageComparer.Equals(package, existingPackage))
                            {
                                log.LogError(
                                    "Package version conflict detected. Package id: {0}; Expected version (from {1}): {2}; Actual version (from {3}): {4}; Root project: {5}",
                                    existingPackage.Id,
                                    existingPackage.VersionOrigin,
                                    existingPackage.Version,
                                    package.VersionOrigin,
                                    package.Version,
                                    pair.Key);
                                isValid = false;
                            }
                        }
                        else
                            uniquePackages.Add(package.Id, package);
                    }
                }
            }

            return isValid;
        }

        private void GatherRoots(ProjectInfo project, ISet<string> roots)
        {
            if (project.ReferencedFrom.Count == 0)
            {
                if (!roots.Contains(project.Path))
                    roots.Add(project.Path);
                return;
            }

            foreach (var refProj in project.ReferencedFrom)
                GatherRoots(_projects[refProj], roots);
        }
    }
}
