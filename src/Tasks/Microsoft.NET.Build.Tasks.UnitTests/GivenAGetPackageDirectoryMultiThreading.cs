// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAGetPackageDirectoryMultiThreading
    {
        [Fact]
        public void ImplementsIMultiThreadableTask()
        {
            var task = new GetPackageDirectory();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void HasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(GetPackageDirectory).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void AssetsFile_IsResolvedRelativeToProjectDirectory()
        {
            var projectDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"pkgdir-mt-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(projectDir);
            try
            {
                var objDir = Path.Combine(projectDir, "obj");
                Directory.CreateDirectory(objDir);
                File.WriteAllText(Path.Combine(objDir, "project.assets.json"),
                    """
                    {
                      "version": 3,
                      "targets": { ".NETCoreApp,Version=v8.0": {} },
                      "libraries": {},
                      "packageFolders": {},
                      "projectFileDependencyGroups": { ".NETCoreApp,Version=v8.0": [] },
                      "project": {
                        "version": "1.0.0",
                        "frameworks": { "net8.0": {} }
                      }
                    }
                    """);

                var task = new GetPackageDirectory
                {
                    BuildEngine = new MockBuildEngine(),
                    AssetsFileWithAdditionalPackageFolders = "obj\\project.assets.json",
                    Items = Array.Empty<ITaskItem>(),
                    PackageFolders = Array.Empty<string>(),
                };

                var teProp = task.GetType().GetProperty("TaskEnvironment");
                teProp.Should().NotBeNull("task must have a TaskEnvironment property after migration");
                teProp!.SetValue(task, TaskEnvironmentHelper.CreateForTest(projectDir));

                var result = task.Execute();
                result.Should().BeTrue("task should succeed when assets file is found via TaskEnvironment");
            }
            finally
            {
                Directory.Delete(projectDir, true);
            }
        }
    }
}
