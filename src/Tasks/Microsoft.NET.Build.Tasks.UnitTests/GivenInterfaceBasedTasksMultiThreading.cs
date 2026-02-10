// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenInterfaceBasedTasksMultiThreading
    {
        [Theory]
        [InlineData(typeof(GenerateShims))]
        [InlineData(typeof(GetPackagesToPrune))]
        [InlineData(typeof(PickBestRid))]
        [InlineData(typeof(PrepareForReadyToRunCompilation))]
        [InlineData(typeof(ResolveReadyToRunCompilers))]
        [InlineData(typeof(RunCsWinRTGenerator))]
        [InlineData(typeof(RunReadyToRunCompiler))]
        [InlineData(typeof(ShowMissingWorkloads))]
        [InlineData(typeof(ProcessFrameworkReferences))]
        [InlineData(typeof(ResolveRuntimePackAssets))]
        [InlineData(typeof(ResolveTargetingPackAssets))]
        public void ItHasMSBuildMultiThreadableTaskAttribute(Type taskType)
        {
            taskType.Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Theory]
        [InlineData(typeof(GenerateShims))]
        [InlineData(typeof(GetPackagesToPrune))]
        [InlineData(typeof(PickBestRid))]
        [InlineData(typeof(PrepareForReadyToRunCompilation))]
        [InlineData(typeof(ResolveReadyToRunCompilers))]
        [InlineData(typeof(RunCsWinRTGenerator))]
        [InlineData(typeof(RunReadyToRunCompiler))]
        [InlineData(typeof(ShowMissingWorkloads))]
        [InlineData(typeof(ProcessFrameworkReferences))]
        [InlineData(typeof(ResolveRuntimePackAssets))]
        [InlineData(typeof(ResolveTargetingPackAssets))]
        public void ItImplementsIMultiThreadableTask(Type taskType)
        {
            var task = Activator.CreateInstance(taskType);
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Theory]
        [InlineData(typeof(GenerateShims))]
        [InlineData(typeof(GetPackagesToPrune))]
        [InlineData(typeof(PickBestRid))]
        [InlineData(typeof(PrepareForReadyToRunCompilation))]
        [InlineData(typeof(ResolveReadyToRunCompilers))]
        [InlineData(typeof(RunCsWinRTGenerator))]
        [InlineData(typeof(RunReadyToRunCompiler))]
        [InlineData(typeof(ShowMissingWorkloads))]
        [InlineData(typeof(ProcessFrameworkReferences))]
        [InlineData(typeof(ResolveRuntimePackAssets))]
        [InlineData(typeof(ResolveTargetingPackAssets))]
        public void ItHasTaskEnvironmentProperty(Type taskType)
        {
            var prop = taskType.GetProperty("TaskEnvironment",
                BindingFlags.Public | BindingFlags.Instance);
            prop.Should().NotBeNull($"{taskType.Name} must have a public TaskEnvironment property");
            prop!.PropertyType.Should().Be(typeof(TaskEnvironment));
            prop.CanRead.Should().BeTrue();
            prop.CanWrite.Should().BeTrue();
        }
    }
}
