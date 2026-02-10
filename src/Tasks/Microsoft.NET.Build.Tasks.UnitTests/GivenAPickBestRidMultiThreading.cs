// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAPickBestRidMultiThreading
    {
        [Fact]
        public void ImplementsIMultiThreadableTask()
        {
            var task = new PickBestRid();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void HasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(PickBestRid).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void RuntimeGraphPath_IsResolvedRelativeToProjectDirectory()
        {
            var projectDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"pickrid-mt-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(projectDir);
            try
            {
                File.WriteAllText(Path.Combine(projectDir, "runtime.json"),
                    """{"runtimes":{"win-x64":{"#import":["win","any"]},"win":{"#import":["any"]},"any":{}}}""");

                var task = new PickBestRid
                {
                    BuildEngine = new MockBuildEngine(),
                    RuntimeGraphPath = "runtime.json",
                    TargetRid = "win-x64",
                    SupportedRids = new[] { "win-x64", "linux-x64" },
                };

                var teProp = task.GetType().GetProperty("TaskEnvironment");
                teProp.Should().NotBeNull("task must have a TaskEnvironment property after migration");
                teProp!.SetValue(task, TaskEnvironmentHelper.CreateForTest(projectDir));

                var result = task.Execute();
                result.Should().BeTrue();
                task.MatchingRid.Should().Be("win-x64");
            }
            finally
            {
                Directory.Delete(projectDir, true);
            }
        }
    }
}
