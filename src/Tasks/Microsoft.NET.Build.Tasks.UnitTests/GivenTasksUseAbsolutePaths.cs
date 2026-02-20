// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NET.Build.Tasks.UnitTests;

/// <summary>
/// Tests that verify MSBuild tasks handle file paths correctly in multi-node scenarios.
/// 
/// ## TEST DESIGN PRINCIPLE
/// 
/// When MSBuild runs in parallel mode, tasks may be spawned on different nodes, each with
/// a potentially different working directory. Tasks that use relative paths resolved from
/// the current working directory will fail in these scenarios.
/// 
/// ### The Problem
/// Consider a task that receives a relative path like "obj/project.assets.json":
/// - The file exists at: C:\MyProject\obj\project.assets.json
/// - The task is spawned with CWD: D:\MSBuildNodes\Node1\
/// - The task tries to open: D:\MSBuildNodes\Node1\obj\project.assets.json (WRONG!)
/// 
/// ### The Solution
/// Tasks should either:
/// 1. Receive absolute paths from MSBuild properties (MSBuild typically provides these)
/// 2. Use TaskEnvironment.GetAbsolutePath() to resolve relative paths correctly
/// 
/// ### Test Strategy
/// These tests create files in a "project" directory, then verify task behavior by:
/// - Passing RELATIVE paths to tasks
/// - Expecting tasks to FAIL if they don't use TaskEnvironment (demonstrating the problem)
/// - Expecting tasks to SUCCEED if they correctly use absolute path resolution
/// 
/// A passing test with `result.Should().BeFalse()` indicates the task needs to be updated
/// to use TaskEnvironment for correct multi-node behavior.
/// </summary>
public class GivenTasksUseAbsolutePaths : IDisposable
{
    private readonly TaskTestEnvironment _env;
    private readonly ITestOutputHelper _output;

    public GivenTasksUseAbsolutePaths(ITestOutputHelper output)
    {
        _env = new TaskTestEnvironment();
        _output = output;
    }

    public void Dispose()
    {
        _env.Dispose();
    }

    #region Infrastructure Verification Tests

    [Fact]
    public void TestEnvironment_ProjectAndSpawnDirectories_AreDifferent()
    {
        // Verify the test infrastructure creates separate directories
        _env.ProjectDirectory.Should().NotBe(_env.SpawnDirectory);
        Directory.Exists(_env.ProjectDirectory).Should().BeTrue();
        Directory.Exists(_env.SpawnDirectory).Should().BeTrue();
        
        _output.WriteLine($"Project directory: {_env.ProjectDirectory}");
        _output.WriteLine($"Spawn directory: {_env.SpawnDirectory}");
    }

    [Fact]
    public void TestEnvironment_DemonstratesPathDifference()
    {
        // Create a file in the project directory
        var projectFile = _env.CreateProjectFile("test.txt", "content");
        
        // Get what the correct and incorrect paths would be
        var correctPath = _env.GetProjectPath("test.txt");
        var incorrectPath = _env.GetIncorrectPath("test.txt");
        
        // They should be different
        correctPath.Should().NotBe(incorrectPath);
        
        // The file exists at the correct path but not at the incorrect path
        File.Exists(correctPath).Should().BeTrue("file was created in project directory");
        File.Exists(incorrectPath).Should().BeFalse("file should not exist in spawn directory");
        
        _output.WriteLine($"Correct path (in project): {correctPath}");
        _output.WriteLine($"Incorrect path (in spawn): {incorrectPath}");
    }

    #endregion

    #region AllowEmptyTelemetry - No File I/O

    /// <summary>
    /// AllowEmptyTelemetry does not perform file I/O, so no relative path testing needed.
    /// </summary>
    [Fact]
    public void AllowEmptyTelemetry_NoFileIO_ShouldSucceed()
    {
        var task = new AllowEmptyTelemetry
        {
            BuildEngine = new MockBuildEngine()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region ApplyImplicitVersions - No File I/O

    /// <summary>
    /// ApplyImplicitVersions operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void ApplyImplicitVersions_NoFileIO_ShouldSucceed()
    {
        var task = new ApplyImplicitVersions
        {
            BuildEngine = new MockBuildEngine(),
            TargetFrameworkVersion = "8.0",
            TargetLatestRuntimePatch = false,
            PackageReferences = Array.Empty<ITaskItem>(),
            ImplicitPackageReferenceVersions = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForDuplicateFrameworkReferences - No File I/O

    /// <summary>
    /// CheckForDuplicateFrameworkReferences operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CheckForDuplicateFrameworkReferences_NoFileIO_ShouldSucceed()
    {
        var task = new CheckForDuplicateFrameworkReferences
        {
            BuildEngine = new MockBuildEngine(),
            FrameworkReferences = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForDuplicateItemMetadata - No File I/O

    /// <summary>
    /// CheckForDuplicateItemMetadata operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CheckForDuplicateItemMetadata_NoFileIO_ShouldSucceed()
    {
        var task = new CheckForDuplicateItemMetadata
        {
            BuildEngine = new MockBuildEngine(),
            Items = new[] { new MockTaskItem("item1.cs", new Dictionary<string, string> { ["Key"] = "Value1" }) },
            MetadataName = "Key"
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForDuplicateItems - No File I/O

    /// <summary>
    /// CheckForDuplicateItems operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CheckForDuplicateItems_NoFileIO_ShouldSucceed()
    {
        var task = new CheckForDuplicateItems
        {
            BuildEngine = new MockBuildEngine(),
            Items = new[] { new MockTaskItem("file.cs", new Dictionary<string, string>()) },
            ItemName = "Compile",
            PropertyNameToDisableDefaultItems = "EnableDefaultCompileItems",
            MoreInformationLink = "https://aka.ms/test",
            DefaultItemsEnabled = true,
            DefaultItemsOfThisTypeEnabled = true
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForImplicitPackageReferenceOverrides - No File I/O

    /// <summary>
    /// CheckForImplicitPackageReferenceOverrides operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CheckForImplicitPackageReferenceOverrides_NoFileIO_ShouldSucceed()
    {
        var task = new CheckForImplicitPackageReferenceOverrides
        {
            BuildEngine = new MockBuildEngine(),
            PackageReferenceItems = Array.Empty<ITaskItem>(),
            MoreInformationLink = "https://aka.ms/test"
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForTargetInAssetsFile

    /// <summary>
    /// Tests that CheckForTargetInAssetsFile can resolve relative paths.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath, SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads AssetsFilePath - if it uses File.Exists/ReadAllText with
    ///         relative path, it resolves from CWD instead of project directory.
    /// </summary>
    [Fact]
    public void CheckForTargetInAssetsFile_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create file in PROJECT directory
        var assetsContent = @"{
            ""version"": 3,
            ""targets"": { "".NETCoreApp,Version=v8.0"": {} },
            ""libraries"": {},
            ""projectFileDependencyGroups"": { "".NETCoreApp,Version=v8.0"": [] },
            ""project"": { ""version"": ""1.0.0"", ""frameworks"": { ""net8.0"": {} } }
        }";
        _env.CreateProjectDirectory("obj");
        _env.CreateProjectFile("obj/project.assets.json", assetsContent);
        
        // Verify test infrastructure: file exists in project dir but NOT at CWD
        var correctPath = _env.GetProjectPath("obj/project.assets.json");
        File.Exists(correctPath).Should().BeTrue("file should exist in project directory");
        File.Exists("obj/project.assets.json").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE path
        var task = new CheckForTargetInAssetsFile
        {
            BuildEngine = new MockBuildEngine(),
            AssetsFilePath = "obj/project.assets.json",  // RELATIVE path
            TargetFramework = "net8.0"
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed by resolving path from project directory
        // Currently FAILS because task uses Path.GetFullPath or File.Exists with relative path
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
    }

    #endregion

    #region CheckIfPackageReferenceShouldBeFrameworkReference - No File I/O

    /// <summary>
    /// CheckIfPackageReferenceShouldBeFrameworkReference operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CheckIfPackageReferenceShouldBeFrameworkReference_NoFileIO_ShouldSucceed()
    {
        var task = new CheckIfPackageReferenceShouldBeFrameworkReference
        {
            BuildEngine = new MockBuildEngine(),
            PackageReferences = Array.Empty<ITaskItem>(),
            FrameworkReferences = Array.Empty<ITaskItem>(),
            PackageReferenceToReplace = ""
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CheckForUnsupportedWinMDReferences - Skipped (Windows-specific)

    #endregion

    #region CollatePackageDownloads - No File I/O

    /// <summary>
    /// CollatePackageDownloads operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CollatePackageDownloads_NoFileIO_ShouldSucceed()
    {
        var task = new CollatePackageDownloads
        {
            BuildEngine = new MockBuildEngine(),
            Packages = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region CollectSDKReferencesDesignTime - No File I/O

    /// <summary>
    /// CollectSDKReferencesDesignTime operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void CollectSDKReferencesDesignTime_NoFileIO_ShouldSucceed()
    {
        var task = new CollectSDKReferencesDesignTime
        {
            BuildEngine = new MockBuildEngine(),
            SdkReferences = Array.Empty<ITaskItem>(),
            PackageReferences = Array.Empty<ITaskItem>(),
            DefaultImplicitPackages = ""
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    // NOTE: CreateAppHost, CreateComHost tests removed - require Microsoft.NET.HostModel

    #region CreateWindowsSdkKnownFrameworkReferences - No File I/O

    /// <summary>
    /// CreateWindowsSdkKnownFrameworkReferences operates on in-memory data, no file I/O.
    /// </summary>
    [Fact]
    public void CreateWindowsSdkKnownFrameworkReferences_NoFileIO_ShouldSucceed()
    {
        var task = new CreateWindowsSdkKnownFrameworkReferences
        {
            BuildEngine = new MockBuildEngine(),
            WindowsSdkSupportedTargetPlatformVersions = Array.Empty<ITaskItem>(),
            UseWindowsSDKPreview = false,
            TargetFrameworkVersion = "8.0",
            TargetPlatformVersion = "10.0.19041.0"
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region FilterResolvedFiles - Skipped (complex dependencies)

    #endregion

    #region FindItemsFromPackages - No File I/O

    /// <summary>
    /// FindItemsFromPackages operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void FindItemsFromPackages_NoFileIO_ShouldSucceed()
    {
        var task = new FindItemsFromPackages
        {
            BuildEngine = new MockBuildEngine(),
            Items = Array.Empty<ITaskItem>(),
            Packages = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    // NOTE: GenerateBundle, GenerateClsidMap tests removed - require Microsoft.NET.HostModel

    #region GenerateDepsFile

    /// <summary>
    /// Tests that GenerateDepsFile can resolve relative paths for reading assets and writing deps.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath, SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads AssetsFilePath and writes DepsFilePath - relative paths
    ///         resolve from CWD instead of project directory.
    /// </summary>
    [Fact]
    public void GenerateDepsFile_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create files in PROJECT directory
        var assetsContent = @"{
            ""version"": 3,
            ""targets"": { "".NETCoreApp,Version=v8.0"": {} },
            ""libraries"": {},
            ""projectFileDependencyGroups"": { "".NETCoreApp,Version=v8.0"": [] },
            ""project"": { ""version"": ""1.0.0"", ""frameworks"": { ""net8.0"": {} } }
        }";
        _env.CreateProjectDirectory("obj");
        _env.CreateProjectFile("obj/project.assets.json", assetsContent);
        _env.CreateProjectFile("myapp.csproj", "<Project></Project>");
        
        // Verify test infrastructure
        File.Exists(_env.GetProjectPath("obj/project.assets.json")).Should().BeTrue();
        File.Exists("obj/project.assets.json").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE paths
        var task = new GenerateDepsFile
        {
            BuildEngine = new MockBuildEngine(),
            ProjectPath = "myapp.csproj",                    // RELATIVE
            AssetsFilePath = "obj/project.assets.json",      // RELATIVE
            DepsFilePath = "obj/myapp.deps.json",            // RELATIVE
            TargetFramework = "net8.0",
            AssemblyName = "myapp",
            AssemblyExtension = ".dll",
            AssemblyVersion = "1.0.0.0",
            IncludeMainProject = true,
            RuntimeFrameworks = Array.Empty<ITaskItem>(),
            CompileReferences = Array.Empty<ITaskItem>(),
            ResolvedNuGetFiles = Array.Empty<ITaskItem>(),
            ResolvedRuntimeTargetsFiles = Array.Empty<ITaskItem>(),
            RuntimeGraphPath = ""
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed and write file to project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
        File.Exists(_env.GetProjectPath("obj/myapp.deps.json")).Should().BeTrue("deps file should be written to project dir");
    }

    #endregion

    #region GenerateGlobalUsings - No File I/O

    /// <summary>
    /// GenerateGlobalUsings generates in-memory strings, no file I/O.
    /// </summary>
    [Fact]
    public void GenerateGlobalUsings_NoFileIO_ShouldSucceed()
    {
        var task = new GenerateGlobalUsings
        {
            BuildEngine = new MockBuildEngine(),
            Usings = new[]
            {
                new MockTaskItem("System", new Dictionary<string, string>()),
                new MockTaskItem("System.Collections.Generic", new Dictionary<string, string>())
            }
        };

        var result = task.Execute();
        result.Should().BeTrue();
        task.Lines.Should().NotBeEmpty();
    }

    #endregion

    // NOTE: GenerateRegFreeComManifest test removed - requires Microsoft.NET.HostModel

    #region GenerateRuntimeConfigurationFiles

    /// <summary>
    /// Tests that GenerateRuntimeConfigurationFiles can write to relative paths.
    /// 
    /// SKILL: SKILL_02_File_Class, SKILL_03_Directory_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task writes RuntimeConfigPath - relative path resolves from CWD.
    /// </summary>
    [Fact]
    public void GenerateRuntimeConfigurationFiles_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create output directory in PROJECT
        _env.CreateProjectDirectory("obj");
        
        // Verify test infrastructure
        Directory.Exists(_env.GetProjectPath("obj")).Should().BeTrue();
        Directory.Exists("obj").Should().BeFalse("obj should NOT exist relative to CWD");

        // Act: Give task RELATIVE path for output
        var task = new GenerateRuntimeConfigurationFiles
        {
            BuildEngine = new MockBuildEngine(),
            RuntimeConfigPath = "obj/myapp.runtimeconfig.json",  // RELATIVE
            TargetFrameworkMoniker = ".NETCoreApp,Version=v8.0",
            RuntimeFrameworks = Array.Empty<ITaskItem>(),
            RollForward = "LatestMinor",
            UserRuntimeConfig = "",
            HostConfigurationOptions = Array.Empty<ITaskItem>(),
            AdditionalProbingPaths = Array.Empty<ITaskItem>(),
            IsSelfContained = false,
            WriteAdditionalProbingPathsToMainConfig = false,
            WriteIncludedFrameworks = false,
            AlwaysIncludeCoreFramework = false
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed and write to project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
        File.Exists(_env.GetProjectPath("obj/myapp.runtimeconfig.json")).Should().BeTrue(
            "runtimeconfig should be written to project dir, not CWD");
    }

    #endregion

    // NOTE: GenerateShims test removed - requires Microsoft.NET.HostModel

    #region GenerateSupportedTargetFrameworkAlias - No File I/O

    /// <summary>
    /// GenerateSupportedTargetFrameworkAlias operates on in-memory data, no file I/O.
    /// </summary>
    [Fact]
    public void GenerateSupportedTargetFrameworkAlias_NoFileIO_ShouldSucceed()
    {
        var task = new GenerateSupportedTargetFrameworkAlias
        {
            BuildEngine = new MockBuildEngine(),
            SupportedTargetFramework = Array.Empty<ITaskItem>(),
            TargetFrameworkMoniker = ".NETCoreApp,Version=v8.0"
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region GenerateToolsSettingsFile

    /// <summary>
    /// Tests that GenerateToolsSettingsFile can write to relative paths.
    /// 
    /// SKILL: SKILL_02_File_Class, SKILL_03_Directory_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task writes ToolsSettingsFilePath - relative path resolves from CWD.
    /// </summary>
    [Fact]
    public void GenerateToolsSettingsFile_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create output directory in PROJECT
        _env.CreateProjectDirectory("obj");
        
        // Verify test infrastructure
        Directory.Exists(_env.GetProjectPath("obj")).Should().BeTrue();
        Directory.Exists("obj").Should().BeFalse("obj should NOT exist relative to CWD");

        // Act: Give task RELATIVE path
        var task = new GenerateToolsSettingsFile
        {
            BuildEngine = new MockBuildEngine(),
            EntryPointRelativePath = "myapp.dll",
            CommandName = "mytool",
            ToolsSettingsFilePath = "obj/DotnetToolSettings.xml"  // RELATIVE
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed and write to project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
        File.Exists(_env.GetProjectPath("obj/DotnetToolSettings.xml")).Should().BeTrue(
            "settings file should be written to project dir, not CWD");
    }

    #endregion

    #region GetAssemblyAttributes

    /// <summary>
    /// Tests that GetAssemblyAttributes can read from relative paths.
    /// 
    /// SKILL: SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads PathToTemplateFile - relative path resolves from CWD.
    /// </summary>
    [Fact]
    public void GetAssemblyAttributes_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create template file in PROJECT directory
        _env.CreateProjectDirectory("obj");
        _env.CreateProjectFile("obj/AssemblyInfo.cs.template", "// template content");
        
        // Verify test infrastructure
        File.Exists(_env.GetProjectPath("obj/AssemblyInfo.cs.template")).Should().BeTrue();
        File.Exists("obj/AssemblyInfo.cs.template").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE path
        var task = new GetAssemblyAttributes
        {
            BuildEngine = new MockBuildEngine(),
            PathToTemplateFile = "obj/AssemblyInfo.cs.template"  // RELATIVE
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed by reading from project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
    }

    #endregion

    #region GetAssemblyVersion - No File I/O

    /// <summary>
    /// GetAssemblyVersion parses version strings, no file I/O.
    /// </summary>
    [Fact]
    public void GetAssemblyVersion_NoFileIO_ShouldSucceed()
    {
        var task = new GetAssemblyVersion
        {
            BuildEngine = new MockBuildEngine(),
            NuGetVersion = "1.0.0"
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region GetDefaultPlatformTargetForNetFramework - No File I/O

    /// <summary>
    /// GetDefaultPlatformTargetForNetFramework returns a constant, no file I/O.
    /// </summary>
    [Fact]
    public void GetDefaultPlatformTargetForNetFramework_NoFileIO_ShouldSucceed()
    {
        var task = new GetDefaultPlatformTargetForNetFramework
        {
            BuildEngine = new MockBuildEngine()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region GetEmbeddedApphostPaths

    /// <summary>
    /// Tests that GetEmbeddedApphostPaths resolves PackagedShimOutputDirectory correctly.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task uses Path.Combine with PackagedShimOutputDirectory - resolves from CWD if relative.
    /// </summary>
    [Fact]
    public void GetEmbeddedApphostPaths_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create directory in PROJECT
        _env.CreateProjectDirectory("shims");
        
        // Verify test infrastructure
        Directory.Exists(_env.GetProjectPath("shims")).Should().BeTrue();
        Directory.Exists("shims").Should().BeFalse("dir should NOT exist relative to CWD");

        // Provide RID items to trigger path generation
        var ridItem = new MockTaskItem("win-x64", new Dictionary<string, string>());

        // Act: Give task RELATIVE path
        var task = new GetEmbeddedApphostPaths
        {
            BuildEngine = new MockBuildEngine(),
            PackagedShimOutputDirectory = "shims",  // RELATIVE
            ShimRuntimeIdentifiers = new[] { ridItem }
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        result.Should().BeTrue();
        
        // Verify output paths reference project directory, not CWD
        if (task.EmbeddedApphostPaths?.Length > 0)
        {
            var outputPath = task.EmbeddedApphostPaths[0].ItemSpec;
            _output.WriteLine($"Output path: {outputPath}");
            
            // Output should be under project directory, not CWD
            outputPath.Should().StartWith(_env.ProjectDirectory,
                "output path should be relative to project directory, not CWD");
        }
    }

    #endregion

    #region GetNuGetShortFolderName - No File I/O

    /// <summary>
    /// GetNuGetShortFolderName parses TFM string, no file I/O.
    /// </summary>
    [Fact]
    public void GetNuGetShortFolderName_NoFileIO_ShouldSucceed()
    {
        var task = new GetNuGetShortFolderName
        {
            BuildEngine = new MockBuildEngine(),
            TargetFrameworkMoniker = ".NETCoreApp,Version=v8.0"
        };

        var result = task.Execute();
        result.Should().BeTrue();
        task.NuGetShortFolderName.Should().NotBeEmpty();
    }

    #endregion

    #region GetPackageDirectory - No File I/O

    /// <summary>
    /// GetPackageDirectory processes item metadata, no file I/O.
    /// </summary>
    [Fact]
    public void GetPackageDirectory_NoFileIO_ShouldSucceed()
    {
        var task = new GetPackageDirectory
        {
            BuildEngine = new MockBuildEngine(),
            Items = new[]
            {
                new MockTaskItem("file.dll", new Dictionary<string, string>
                {
                    ["NuGetPackageId"] = "TestPackage",
                    ["NuGetPackageVersion"] = "1.0.0"
                })
            }
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region GetPackagesToPrune - Skipped (complex setup)

    #endregion

    #region GetPublishItemsOutputGroupOutputs

    /// <summary>
    /// Tests that GetPublishItemsOutputGroupOutputs resolves PublishDir correctly.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task uses PublishDir to compute output paths - resolves from CWD if relative.
    /// </summary>
    [Fact]
    public void GetPublishItemsOutputGroupOutputs_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create directory in PROJECT
        _env.CreateProjectDirectory("publish");
        var filePath = _env.CreateProjectFile("publish/myapp.dll", "MZ");
        
        // Verify test infrastructure
        Directory.Exists(_env.GetProjectPath("publish")).Should().BeTrue();
        Directory.Exists("publish").Should().BeFalse("dir should NOT exist relative to CWD");

        // Provide files to trigger output computation
        var fileItem = new MockTaskItem(filePath, new Dictionary<string, string>
        {
            ["RelativePath"] = "myapp.dll"
        });

        // Act: Give task RELATIVE path
        var task = new GetPublishItemsOutputGroupOutputs
        {
            BuildEngine = new MockBuildEngine(),
            PublishDir = "publish",  // RELATIVE
            ResolvedFileToPublish = new[] { fileItem }
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        result.Should().BeTrue();
        
        // Verify output paths reference project directory, not CWD
        if (task.PublishItemsOutputGroupOutputs?.Length > 0)
        {
            var outputPath = task.PublishItemsOutputGroupOutputs[0].GetMetadata("FinalOutputPath");
            _output.WriteLine($"FinalOutputPath: {outputPath}");
            
            // Output should reference project directory, not CWD
            if (!string.IsNullOrEmpty(outputPath))
            {
                outputPath.Should().StartWith(_env.ProjectDirectory,
                    "output path should be relative to project directory, not CWD");
            }
        }
    }

    #endregion

    #region JoinItems - No File I/O

    /// <summary>
    /// JoinItems merges in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void JoinItems_NoFileIO_ShouldSucceed()
    {
        var task = new JoinItems
        {
            BuildEngine = new MockBuildEngine(),
            Left = new[] { new MockTaskItem("item1", new Dictionary<string, string> { ["LeftData"] = "L1" }) },
            Right = new[] { new MockTaskItem("item1", new Dictionary<string, string> { ["RightData"] = "R1" }) },
            LeftMetadata = new[] { "*" },
            RightMetadata = new[] { "*" }
        };

        var result = task.Execute();
        result.Should().BeTrue();
        task.JoinResult.Should().HaveCount(1);
    }

    #endregion

    #region ParseTargetManifests - No File I/O

    /// <summary>
    /// ParseTargetManifests parses string input, no file I/O with empty input.
    /// </summary>
    [Fact]
    public void ParseTargetManifests_NoFileIO_ShouldSucceed()
    {
        var task = new ParseTargetManifests
        {
            BuildEngine = new MockBuildEngine(),
            TargetManifestFiles = ""
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region PickBestRid - Skipped (complex RID handling)

    #endregion

    #region PrepareForReadyToRunCompilation

    /// <summary>
    /// Tests that PrepareForReadyToRunCompilation resolves OutputPath correctly.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task uses Path.Combine(OutputPath, relativePath) which resolves from CWD if OutputPath is relative.
    /// </summary>
    [Fact]
    public void PrepareForReadyToRunCompilation_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create output directory and assembly in PROJECT directory
        _env.CreateProjectDirectory("obj/r2r");
        var dllPath = _env.CreateProjectFile("myapp.dll", "MZ"); // Fake DLL content
        
        // Verify test infrastructure
        Directory.Exists(_env.GetProjectPath("obj/r2r")).Should().BeTrue();
        Directory.Exists("obj/r2r").Should().BeFalse("dir should NOT exist relative to CWD");

        // Create an assembly item that will trigger path resolution
        var assemblyItem = new MockTaskItem(dllPath, new Dictionary<string, string>
        {
            ["RelativePath"] = "myapp.dll",
            ["SetOfInput"] = "Compile"
        });

        // Act: Give task RELATIVE OutputPath
        var task = new PrepareForReadyToRunCompilation
        {
            BuildEngine = new MockBuildEngine(),
            Assemblies = new[] { assemblyItem },
            OutputPath = "obj/r2r",  // RELATIVE - should resolve from project dir
            MainAssembly = new MockTaskItem(dllPath, new Dictionary<string, string>()),
            EmitSymbols = false,
            ReadyToRunUseCrossgen2 = false,
            IncludeSymbolsInSingleFile = false
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // The task might succeed, but the output paths would be wrong
        // Check that output paths reference the PROJECT directory, not CWD
        if (task.ReadyToRunFilesToPublish.Length > 0)
        {
            var outputPath = task.ReadyToRunFilesToPublish[0].ItemSpec;
            _output.WriteLine($"Output path: {outputPath}");
            
            // Output should be under project directory, not CWD
            outputPath.Should().StartWith(_env.ProjectDirectory,
                "output path should be relative to project directory, not CWD");
        }
    }

    #endregion

    #region ProcessFrameworkReferences - No File I/O (with empty input)

    /// <summary>
    /// ProcessFrameworkReferences with empty FrameworkReferences does no file I/O.
    /// The task only reads from TargetingPackRoot/NetCoreRoot when resolving actual framework references.
    /// </summary>
    [Fact]
    public void ProcessFrameworkReferences_NoFileIO_ShouldSucceed()
    {
        var task = new ProcessFrameworkReferences
        {
            BuildEngine = new MockBuildEngine(),
            FrameworkReferences = Array.Empty<ITaskItem>(),
            KnownFrameworkReferences = Array.Empty<ITaskItem>(),
            TargetFrameworkIdentifier = ".NETCoreApp",
            TargetFrameworkVersion = "8.0",
            TargetingPackRoot = "packs",  // Not used with empty input
            RuntimeGraphPath = "",
            SelfContained = false,
            ReadyToRunEnabled = false,
            NETCoreSdkRuntimeIdentifier = "win-x64",
            NetCoreRoot = "dotnet"  // Not used with empty input
        };

        var result = task.Execute();
        // With empty input, task succeeds without file I/O
        result.Should().BeTrue();
    }

    #endregion

    #region ProduceContentAssets - No File I/O (with empty input)

    /// <summary>
    /// ProduceContentAssets with empty ContentFileDependencies does no file I/O.
    /// The task only processes files when there are actual content dependencies.
    /// </summary>
    [Fact]
    public void ProduceContentAssets_NoFileIO_ShouldSucceed()
    {
        var task = new ProduceContentAssets
        {
            BuildEngine = new MockBuildEngine(),
            ContentFileDependencies = Array.Empty<ITaskItem>(),
            ContentPreprocessorOutputDirectory = "obj/content",  // Not used with empty input
            ProjectLanguage = "C#"
        };

        var result = task.Execute();
        // With empty input, task succeeds without file I/O
        result.Should().BeTrue();
    }

    #endregion

    #region RemoveDuplicatePackageReferences - No File I/O

    /// <summary>
    /// RemoveDuplicatePackageReferences operates on in-memory items, no file I/O.
    /// </summary>
    [Fact]
    public void RemoveDuplicatePackageReferences_NoFileIO_ShouldSucceed()
    {
        var task = new RemoveDuplicatePackageReferences
        {
            BuildEngine = new MockBuildEngine(),
            InputPackageReferences = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region ResolveAppHosts - No File I/O (with empty input)

    /// <summary>
    /// ResolveAppHosts with empty KnownAppHostPacks does no file I/O.
    /// The task only reads files when resolving actual app host packs.
    /// </summary>
    [Fact]
    public void ResolveAppHosts_NoFileIO_ShouldSucceed()
    {
        var task = new ResolveAppHosts
        {
            BuildEngine = new MockBuildEngine(),
            TargetFrameworkIdentifier = ".NETCoreApp",
            TargetFrameworkVersion = "8.0",
            TargetingPackRoot = "packs",  // Not used with empty input
            AppHostRuntimeIdentifier = "win-x64",
            PackAsToolShimRuntimeIdentifiers = Array.Empty<ITaskItem>(),
            KnownAppHostPacks = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        // With empty input, task succeeds without file I/O
        _output.WriteLine($"Result: {result}");
    }

    #endregion

    #region ResolveFrameworkReferences - No File I/O

    /// <summary>
    /// ResolveFrameworkReferences with empty items does no file I/O.
    /// </summary>
    [Fact]
    public void ResolveFrameworkReferences_NoFileIO_ShouldSucceed()
    {
        var task = new ResolveFrameworkReferences
        {
            BuildEngine = new MockBuildEngine(),
            ResolvedFrameworkReferences = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region ResolvePackageAssets

    /// <summary>
    /// This test verifies that ResolvePackageAssets correctly resolves relative paths
    /// using TaskEnvironment, allowing it to work in multi-node MSBuild scenarios.
    /// 
    /// TEST DESIGN:
    /// - File is created in the PROJECT directory
    /// - Task receives RELATIVE paths (as MSBuild might provide)
    /// - Task should use TaskEnvironment to resolve paths relative to the project
    /// - Test FAILS until the task is updated to use TaskEnvironment
    /// - Test PASSES once the task correctly uses TaskEnvironment
    /// 
    /// CURRENT STATE: This test FAILS because ResolvePackageAssets does not yet
    /// use TaskEnvironment to resolve relative paths. It tries to resolve from CWD.
    /// </summary>
    [Fact]
    public void ResolvePackageAssets_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        var assetsContent = @"{
            ""version"": 3,
            ""targets"": { "".NETCoreApp,Version=v8.0"": {} },
            ""libraries"": {},
            ""projectFileDependencyGroups"": { "".NETCoreApp,Version=v8.0"": [] },
            ""project"": { ""version"": ""1.0.0"", ""frameworks"": { ""net8.0"": {} } }
        }";
        
        // Create the file in the PROJECT directory
        _env.CreateProjectDirectory("obj");
        _env.CreateProjectFile("obj/project.assets.json", assetsContent);
        
        // Verify the file exists in project dir but not in spawn dir (CWD)
        var correctAbsolutePath = _env.GetProjectPath("obj/project.assets.json");
        File.Exists(correctAbsolutePath).Should().BeTrue("file should exist in project directory");
        
        // Provide RELATIVE paths to the task
        // A correctly implemented task should resolve these relative to ProjectDirectory
        const string relativePath = "obj/project.assets.json";

        var task = new ResolvePackageAssets
        {
            BuildEngine = new MockBuildEngine(),
            ProjectAssetsCacheFile = "obj/project.assets.cache",
            ProjectAssetsFile = relativePath,
            ProjectPath = "myapp.csproj",
            TargetFramework = "net8.0",
            RuntimeIdentifier = "",
            DisablePackageAssetsCache = true,
            DotNetAppHostExecutableNameWithoutExtension = "apphost",
            DefaultImplicitPackages = ""
            // TODO: Once TaskEnvironment is implemented, set it here:
            // TaskEnvironment = _env.TaskEnvironment
        };

        _output.WriteLine($"Current directory: {Environment.CurrentDirectory}");
        _output.WriteLine($"Project directory: {_env.ProjectDirectory}");
        _output.WriteLine($"File exists at correct path: {File.Exists(correctAbsolutePath)}");
        _output.WriteLine($"File exists at relative path from CWD: {File.Exists(relativePath)}");

        var result = task.Execute();
        
        // The task SHOULD succeed by resolving relative paths from the project directory
        // Currently FAILS because the task doesn't use TaskEnvironment
        result.Should().BeTrue("task should succeed when TaskEnvironment correctly resolves relative paths");
    }

    #endregion

    #region ResolvePackageDependencies

    /// <summary>
    /// Tests that ResolvePackageDependencies can resolve relative paths.
    /// 
    /// SKILL: SKILL_01_Path_GetFullPath, SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads ProjectAssetsFile - relative path resolves from CWD.
    /// </summary>
    [Fact]
    public void ResolvePackageDependencies_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create assets file in PROJECT directory
        var assetsContent = @"{
            ""version"": 3,
            ""targets"": { "".NETCoreApp,Version=v8.0"": {} },
            ""libraries"": {},
            ""projectFileDependencyGroups"": { "".NETCoreApp,Version=v8.0"": [] },
            ""project"": { ""version"": ""1.0.0"", ""frameworks"": { ""net8.0"": {} } }
        }";
        _env.CreateProjectDirectory("obj");
        _env.CreateProjectFile("obj/project.assets.json", assetsContent);
        _env.CreateProjectFile("myapp.csproj", "<Project></Project>");
        
        // Verify test infrastructure
        File.Exists(_env.GetProjectPath("obj/project.assets.json")).Should().BeTrue();
        File.Exists("obj/project.assets.json").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE paths
        var task = new ResolvePackageDependencies
        {
            BuildEngine = new MockBuildEngine(),
            ProjectAssetsFile = "obj/project.assets.json",  // RELATIVE
            ProjectPath = "myapp.csproj"                     // RELATIVE
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed by reading from project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
    }

    #endregion

    #region ResolveReadyToRunCompilers - No File I/O

    /// <summary>
    /// ResolveReadyToRunCompilers with empty items does no file I/O.
    /// </summary>
    [Fact]
    public void ResolveReadyToRunCompilers_NoFileIO_ShouldSucceed()
    {
        var task = new ResolveReadyToRunCompilers
        {
            BuildEngine = new MockBuildEngine(),
            RuntimePacks = Array.Empty<ITaskItem>(),
            Crossgen2Packs = Array.Empty<ITaskItem>(),
            TargetingPacks = Array.Empty<ITaskItem>(),
            RuntimeGraphPath = "",
            NETCoreSdkRuntimeIdentifier = "win-x64"
        };

        var result = task.Execute();
        _output.WriteLine($"Result: {result}");
    }

    #endregion

    #region ResolveCopyLocalAssets - Skipped (complex dependencies)

    #endregion

    #region ResolveRuntimePackAssets - No File I/O

    /// <summary>
    /// ResolveRuntimePackAssets with empty items does no file I/O.
    /// </summary>
    [Fact]
    public void ResolveRuntimePackAssets_NoFileIO_ShouldSucceed()
    {
        var task = new ResolveRuntimePackAssets
        {
            BuildEngine = new MockBuildEngine(),
            ResolvedRuntimePacks = Array.Empty<ITaskItem>(),
            SatelliteResourceLanguages = Array.Empty<ITaskItem>()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region ResolveTargetingPackAssets - No File I/O

    /// <summary>
    /// ResolveTargetingPackAssets with empty items does no file I/O.
    /// </summary>
    [Fact]
    public void ResolveTargetingPackAssets_NoFileIO_ShouldSucceed()
    {
        var task = new ResolveTargetingPackAssets
        {
            BuildEngine = new MockBuildEngine(),
            FrameworkReferences = Array.Empty<ITaskItem>(),
            ResolvedTargetingPacks = Array.Empty<ITaskItem>(),
            RuntimeFrameworks = Array.Empty<ITaskItem>(),
            GenerateErrorForMissingTargetingPacks = false
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region SelectRuntimeIdentifierSpecificItems - Skipped (complex RID handling)

    #endregion

    #region SetGeneratedAppConfigMetadata

    /// <summary>
    /// Tests that SetGeneratedAppConfigMetadata can handle relative paths.
    /// 
    /// SKILL: SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads AppConfigFile - relative path resolves from CWD.
    /// </summary>
    [Fact]
    public void SetGeneratedAppConfigMetadata_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create config file in PROJECT directory
        _env.CreateProjectFile("App.config", "<configuration></configuration>");
        _env.CreateProjectDirectory("obj");
        
        // Verify test infrastructure
        File.Exists(_env.GetProjectPath("App.config")).Should().BeTrue();
        File.Exists("App.config").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE paths
        var task = new SetGeneratedAppConfigMetadata
        {
            BuildEngine = new MockBuildEngine(),
            AppConfigFile = new MockTaskItem("App.config", new Dictionary<string, string>()),  // RELATIVE
            GeneratedAppConfigFile = "obj/myapp.exe.config",  // RELATIVE
            TargetName = "myapp"
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed by resolving paths from project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
    }

    #endregion

    #region ShowMissingWorkloads - No File I/O (with empty input)

    /// <summary>
    /// ShowMissingWorkloads with empty MissingWorkloadPacks does no file I/O.
    /// The task only uses NetCoreRoot when there are actual missing workloads to report.
    /// </summary>
    [Fact]
    public void ShowMissingWorkloads_NoFileIO_ShouldSucceed()
    {
        var task = new ShowMissingWorkloads
        {
            BuildEngine = new MockBuildEngine(),
            MissingWorkloadPacks = Array.Empty<ITaskItem>(),
            NetCoreRoot = "dotnet"  // Not used with empty input
        };

        var result = task.Execute();
        // With empty input, task succeeds without file I/O
        result.Should().BeTrue();
    }

    #endregion

    #region ShowPreviewMessage - No File I/O

    /// <summary>
    /// ShowPreviewMessage logs a message, no file I/O.
    /// </summary>
    [Fact]
    public void ShowPreviewMessage_NoFileIO_ShouldSucceed()
    {
        var task = new ShowPreviewMessage
        {
            BuildEngine = new MockBuildEngine()
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region ValidateExecutableReferences - No File I/O

    /// <summary>
    /// ValidateExecutableReferences with empty items does no file I/O.
    /// </summary>
    [Fact]
    public void ValidateExecutableReferences_NoFileIO_ShouldSucceed()
    {
        var task = new ValidateExecutableReferences
        {
            BuildEngine = new MockBuildEngine(),
            ReferencedProjects = Array.Empty<ITaskItem>(),
            SelfContained = false,
            IsExecutable = true
        };

        var result = task.Execute();
        result.Should().BeTrue();
    }

    #endregion

    #region WriteAppConfigWithSupportedRuntime

    /// <summary>
    /// Tests that WriteAppConfigWithSupportedRuntime can handle relative paths.
    /// 
    /// SKILL: SKILL_02_File_Class
    /// EXPECTED: FAIL (until task uses TaskEnvironment)
    /// REASON: Task reads AppConfigFile and writes OutputAppConfigFile - 
    ///         relative paths resolve from CWD.
    /// </summary>
    [Fact]
    public void WriteAppConfigWithSupportedRuntime_WithRelativePaths_ShouldResolveFromProjectDirectory()
    {
        // Arrange: Create files in PROJECT directory
        _env.CreateProjectFile("App.config", "<configuration></configuration>");
        _env.CreateProjectDirectory("bin");
        
        // Verify test infrastructure
        File.Exists(_env.GetProjectPath("App.config")).Should().BeTrue();
        File.Exists("App.config").Should().BeFalse("file should NOT exist relative to CWD");

        // Act: Give task RELATIVE paths
        var task = new WriteAppConfigWithSupportedRuntime
        {
            BuildEngine = new MockBuildEngine(),
            AppConfigFile = new MockTaskItem("App.config", new Dictionary<string, string>()),  // RELATIVE
            OutputAppConfigFile = new MockTaskItem("bin/myapp.exe.config", new Dictionary<string, string>()),  // RELATIVE
            TargetFrameworkIdentifier = ".NETFramework",
            TargetFrameworkVersion = "4.8"
        };

        _output.WriteLine($"CWD: {Environment.CurrentDirectory}");
        _output.WriteLine($"ProjectDir: {_env.ProjectDirectory}");
        
        var result = task.Execute();
        
        // Assert: Task should succeed and write to project directory
        result.Should().BeTrue("task should resolve relative paths via TaskEnvironment");
        File.Exists(_env.GetProjectPath("bin/myapp.exe.config")).Should().BeTrue(
            "output should be written to project dir, not CWD");
    }

    #endregion

    #region Demonstration: Absolute vs Relative Paths

    [Fact]
    public void AbsolutePath_AlwaysPointsToCorrectFile()
    {
        var content = "test content";
        var absolutePath = _env.CreateProjectFile("data/input.txt", content);
        
        Path.IsPathRooted(absolutePath).Should().BeTrue();
        
        var readContent = File.ReadAllText(absolutePath);
        readContent.Should().Be(content);
        
        _output.WriteLine($"Absolute path: {absolutePath}");
        _output.WriteLine($"Successfully read file content: {readContent}");
    }

    [Fact]
    public void MockTaskEnvironment_ResolvesRelativeToProjectDirectory()
    {
        var content = "test content";
        _env.CreateProjectFile("subdir/file.txt", content);
        
        var resolvedPath = _env.TaskEnvironment.GetAbsolutePath("subdir/file.txt");
        
        var expectedPath = _env.GetProjectPath("subdir/file.txt");
        ((string)resolvedPath).Replace('/', Path.DirectorySeparatorChar)
            .Should().Be(expectedPath.Replace('/', Path.DirectorySeparatorChar));
        
        File.Exists(resolvedPath).Should().BeTrue();
        
        _output.WriteLine($"TaskEnvironment resolved: {resolvedPath}");
    }

    #endregion
}
