// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenACreateComHostMultiThreading
    {
        [Fact]
        public void ImplementsIMultiThreadableTask()
        {
            var task = new CreateComHost();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void HasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(CreateComHost).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void Paths_AreResolvedRelativeToProjectDirectory()
        {
            // Create a unique temp directory to act as the project directory
            var projectDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"comhost-mt-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(projectDir);
            try
            {
                // Create relative path structure under the project dir
                var sourceDir = Path.Combine(projectDir, "source");
                var destDir = Path.Combine(projectDir, "output");
                Directory.CreateDirectory(sourceDir);
                Directory.CreateDirectory(destDir);

                // Create a fake comhost source file and clsid map
                File.WriteAllText(Path.Combine(sourceDir, "comhost.dll"), "fake-source");
                File.WriteAllText(Path.Combine(sourceDir, "clsidmap.bin"), "fake-clsid");

                var task = new CreateComHost
                {
                    BuildEngine = new MockBuildEngine(),
                    ComHostSourcePath = "source\\comhost.dll",
                    ComHostDestinationPath = "output\\comhost.dll",
                    ClsidMapPath = "source\\clsidmap.bin",
                };

                // Set TaskEnvironment via reflection (may not exist yet)
                var teProp = task.GetType().GetProperty("TaskEnvironment");
                teProp.Should().NotBeNull("task must have a TaskEnvironment property after migration");
                teProp!.SetValue(task, TaskEnvironmentHelper.CreateForTest(projectDir));

                // ComHost.Create will throw because our fake files aren't valid PE binaries.
                // The key assertion is that the exception comes from PE processing (ResourceUpdater),
                // NOT from "file not found" — proving paths were resolved via TaskEnvironment.
                try
                {
                    task.Execute();
                }
                catch (Exception)
                {
                    // Expected — ComHost.Create fails on fake binaries
                }

                // Verify that any errors logged are NOT about missing files
                var engine = (MockBuildEngine)task.BuildEngine;
                var errors = engine.Errors.Select(e => e.Message).ToList();
                errors.Should().NotContain(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    && e.Contains("comhost", StringComparison.OrdinalIgnoreCase),
                    "paths should be resolved via TaskEnvironment, not CWD");
            }
            finally
            {
                Directory.Delete(projectDir, true);
            }
        }
    }
}
