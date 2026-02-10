// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAParseTargetManifestsMultiThreading
    {
        [Fact]
        public void ImplementsIMultiThreadableTask()
        {
            var task = new ParseTargetManifests();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void HasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(ParseTargetManifests).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ManifestFile_IsResolvedRelativeToProjectDirectory()
        {
            var projectDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"manifest-mt-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(projectDir);
            try
            {
                // Create a valid store artifact XML at a relative path
                File.WriteAllText(Path.Combine(projectDir, "artifact.xml"),
                    """
                    <StoreArtifacts>
                      <Package Id="TestPackage" Version="1.0.0" />
                    </StoreArtifacts>
                    """);

                var task = new ParseTargetManifests
                {
                    BuildEngine = new MockBuildEngine(),
                    TargetManifestFiles = "artifact.xml",
                };

                var teProp = task.GetType().GetProperty("TaskEnvironment");
                teProp.Should().NotBeNull("task must have a TaskEnvironment property after migration");
                teProp!.SetValue(task, TaskEnvironmentHelper.CreateForTest(projectDir));

                var result = task.Execute();
                result.Should().BeTrue("task should succeed when manifest is found via TaskEnvironment");
                task.RuntimeStorePackages.Should().HaveCount(1);
                task.RuntimeStorePackages[0].GetMetadata("NuGetPackageId").Should().Be("TestPackage");
            }
            finally
            {
                Directory.Delete(projectDir, true);
            }
        }
    }
}
