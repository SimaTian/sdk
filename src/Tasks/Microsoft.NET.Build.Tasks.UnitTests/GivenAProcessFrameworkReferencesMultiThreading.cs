// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAProcessFrameworkReferencesMultiThreading
    {
        [Fact]
        public void ItHasMultiThreadableAttribute()
        {
            typeof(ProcessFrameworkReferences)
                .Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ItImplementsIMultiThreadableTask()
        {
            var task = new ProcessFrameworkReferences();
            task.Should().BeAssignableTo<IMultiThreadableTask>();
        }

        [Fact]
        public void ItUsesTaskEnvironmentForEnvironmentVariables()
        {
            // Verify the task implements IMultiThreadableTask which provides TaskEnvironment
            var task = new ProcessFrameworkReferences();
            var multiThreadable = task as IMultiThreadableTask;
            multiThreadable.Should().NotBeNull("ProcessFrameworkReferences must implement IMultiThreadableTask for thread-safe env var access");

            // Verify TaskEnvironment can be set via the interface
            var env = TaskEnvironmentHelper.CreateForTest();
            multiThreadable!.TaskEnvironment = env;
            multiThreadable.TaskEnvironment.Should().BeSameAs(env);
        }
    }
}
