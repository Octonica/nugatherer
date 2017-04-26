using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace Octonica.NuGatherer.Tests
{
    internal class TestBuildEngine : IBuildEngine
    {
        public bool ContinueOnError
        {
            get { throw new NotImplementedException(); }
        }

        public int LineNumberOfTaskNode => -1;

        public int ColumnNumberOfTaskNode => -1;

        public string ProjectFileOfTaskNode { get; }

        public List<LazyFormattedBuildEventArgs> BuildEvents { get; } = new List<LazyFormattedBuildEventArgs>();

        public TestBuildEngine()
        {
            var assembly = typeof(TestBuildEngine).GetTypeInfo().Assembly;
            var path = assembly.Location;
            var dir = Path.GetDirectoryName(path);
            Assert.That(dir, Is.Not.Null);
            ProjectFileOfTaskNode = Path.GetFullPath(Path.Combine(dir, "fake_root.msbuildproj"));
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            BuildEvents.Add(e);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            BuildEvents.Add(e);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            BuildEvents.Add(e);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            BuildEvents.Add(e);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }
    }
}
