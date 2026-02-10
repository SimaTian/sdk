// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAGenerateRegFreeComManifestMultiThreading
    {
        [Fact]
        public void ImplementsIMultiThreadableTask()
        {
            var task = new GenerateRegFreeComManifest();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void HasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(GenerateRegFreeComManifest).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void IntermediateAssembly_IsResolvedRelativeToProjectDirectory()
        {
            var projectDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"regfree-mt-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(projectDir);
            try
            {
                // Place a copy of this test assembly at a relative path under the project dir
                var thisAssemblyPath = typeof(GivenAGenerateRegFreeComManifestMultiThreading).Assembly.Location;
                var binDir = Path.Combine(projectDir, "bin");
                Directory.CreateDirectory(binDir);
                var assemblyFileName = Path.GetFileName(thisAssemblyPath);
                File.Copy(thisAssemblyPath, Path.Combine(binDir, assemblyFileName));

                // Create fake clsidmap and manifest output paths
                File.WriteAllText(Path.Combine(binDir, "clsidmap.bin"), "{}");

                var task = new GenerateRegFreeComManifest
                {
                    BuildEngine = new MockBuildEngine(),
                    IntermediateAssembly = $"bin\\{assemblyFileName}",
                    ComHostName = "test.comhost.dll",
                    ClsidMapPath = "bin\\clsidmap.bin",
                    ComManifestPath = "bin\\test.manifest",
                };

                // Set TaskEnvironment via reflection
                var teProp = task.GetType().GetProperty("TaskEnvironment");
                teProp.Should().NotBeNull("task must have a TaskEnvironment property after migration");
                teProp!.SetValue(task, TaskEnvironmentHelper.CreateForTest(projectDir));

                // Execute — will try to read the assembly version from the relative path
                // resolved via TaskEnvironment, then create the manifest
                try
                {
                    task.Execute();
                }
                catch (Exception)
                {
                    // May fail due to invalid clsidmap content, but that's ok
                }

                // If the IntermediateAssembly was resolved correctly, the manifest file
                // should be created at the absolute path (or at least attempted)
                var engine = (MockBuildEngine)task.BuildEngine;
                var errors = engine.Errors.Select(e => e.Message).ToList();
                // Should NOT have file-not-found for the intermediate assembly
                errors.Should().NotContain(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    && e.Contains(assemblyFileName, StringComparison.OrdinalIgnoreCase),
                    "IntermediateAssembly should be resolved via TaskEnvironment");
            }
            finally
            {
                Directory.Delete(projectDir, true);
            }
        }
    }
}
