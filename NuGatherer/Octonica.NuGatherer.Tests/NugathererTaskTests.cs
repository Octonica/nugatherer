using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace Octonica.NuGatherer.Tests
{
    [TestFixture]
    [Category("NuGathererTask")]
    public class NugathererTaskTests
    {
        // See TestData/graph.txt for details

        [Test]
        public void Linear()
        {
            var task = CreateTask();
            task.PackagesOutFile = "packages.config";
            task.RootProjects = new ITaskItem[] {GetTaskItem("p1")};
            var executed = task.Execute();
            Assert.That(executed, Is.True);

            var packages = task.GetPackages(Path.GetDirectoryName(task.BuildEngine.ProjectFileOfTaskNode));
            var expectedPackages = new HashSet<INuGetPackageInfo>(NuGetPackageInfoComparer.Instance)
            {
                new TestNuGetPackageInfo("p1", "1"),
                new TestNuGetPackageInfo("p2", "1"),
                new TestNuGetPackageInfo("p3", "1"),
                new TestNuGetPackageInfo("p4", "1"),
                new TestNuGetPackageInfo("p5", "1")
            };

            expectedPackages.SymmetricExceptWith(packages);
            Assert.That(expectedPackages, Has.Count.EqualTo(0), "Some packages was not resolved.");
        }

        [Test]
        public void Graph()
        {
            var task = CreateTask();
            task.PackagesOutFile = "packages.config";
            task.RootProjects = new ITaskItem[] { GetTaskItem("p3") };

            var executed = task.Execute();
            Assert.That(executed, Is.True);

            var packages = task.GetPackages(Path.GetDirectoryName(task.BuildEngine.ProjectFileOfTaskNode));
            var expectedPackages = new HashSet<INuGetPackageInfo>(NuGetPackageInfoComparer.Instance)
            {
                new TestNuGetPackageInfo("p3", "1"),
                new TestNuGetPackageInfo("p4", "1"),
                new TestNuGetPackageInfo("p5", "1"),

                new TestNuGetPackageInfo("p10", "1"),
                new TestNuGetPackageInfo("p11", "1"),
                new TestNuGetPackageInfo("p12", "1"),
                new TestNuGetPackageInfo("p13", "1"),
                new TestNuGetPackageInfo("p14", "1"),
                new TestNuGetPackageInfo("p15", "1"),
                new TestNuGetPackageInfo("p16", "1")
            };

            expectedPackages.SymmetricExceptWith(packages);
            Assert.That(expectedPackages, Has.Count.EqualTo(0), "Some packages was not resolved.");
        }

        [Test]
        public void Conflicts()
        {
            var task = CreateTask();
            task.PackagesOutFile = "packages.config";
            task.RootProjects = new ITaskItem[] { GetTaskItem("p7") };

            var executed = task.Execute();
            Assert.That(executed, Is.False);

            var messages = ((TestBuildEngine) task.BuildEngine).BuildEvents;
            Assert.That(messages.OfType<BuildErrorEventArgs>().Count(), Is.EqualTo(2), "Too many error messages.");
        }

        private static NuGathererTask CreateTask()
        {
            var task = new NuGathererTask
            {
                BuildEngine = new TestBuildEngine(),
                HostObject = new TestTaskHost()
            };
            return task;
        }

        private static TaskItem GetTaskItem(string projectFolder)
        {
            var path = Path.Combine("../../../TestData", projectFolder, "proj.csproj");
            var taskItem = new TaskItem(path);
            return taskItem;
        }
    }
}
