// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAttributeOnlyTasksMultiThreading
    {
        [Fact]
        public void FilterResolvedFilesHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(FilterResolvedFiles).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void GenerateRegFreeComManifestHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(GenerateRegFreeComManifest).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void GetAssemblyVersionHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(GetAssemblyVersion).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void GetPackageDirectoryHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(GetPackageDirectory).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ParseTargetManifestsHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(ParseTargetManifests).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ProduceContentAssetsHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(ProduceContentAssets).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ResolveCopyLocalAssetsHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(ResolveCopyLocalAssets).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void SelectRuntimeIdentifierSpecificItemsHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(SelectRuntimeIdentifierSpecificItems).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void SetGeneratedAppConfigMetadataHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(SetGeneratedAppConfigMetadata).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }

        [Fact]
        public void ValidateExecutableReferencesHasMSBuildMultiThreadableTaskAttribute()
        {
            typeof(ValidateExecutableReferences).Should().BeDecoratedWith<MSBuildMultiThreadableTaskAttribute>();
        }
    }
}
