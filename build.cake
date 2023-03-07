#tool nuget:?package=vswhere&version=2.8.4
#tool nuget:?package=NUnit.ConsoleRunner&version=3.13.2
#addin nuget:?package=Cake.FileHelpers&version=5.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var version_main = "3.8.1";
var version = Argument("build-version", EnvironmentVariable("BUILD_NUMBER") ?? version_main + ".1");
var repositoryUrl = Argument("repository-url", "https://github.com/stardeath/MonoGame.Extended");
var configuration = Argument("build-configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

MSBuildSettings msPackSettings, mdPackSettings, msBuildSettings;
DotNetMSBuildSettings dnMsBuildSettings;
DotNetBuildSettings dnBuildSettings;
DotNetPackSettings dnPackSettings;
DotNetPublishSettings dnPublishSettings;
DotNetNuGetPushSettings dnNuGetPushSettings;

private void PackMSBuild(string filePath)
{
    MSBuild(filePath, msPackSettings);
}

private void PackDotnet(string filePath)
{
    DotNetPack(filePath, dnPackSettings);
}

private void PublishDotnet(string filePath)
{
    DotNetPublish(filePath, dnPublishSettings);
}

private void NuGetPushDotNet(string filePath)
{
    DotNetNuGetPush(filePath, dnNuGetPushSettings);
}

private bool GetMSBuildWith(string requires)
{
    if (IsRunningOnWindows())
    {
        DirectoryPath vsLatest = VSWhereLatest(new VSWhereLatestSettings { Requires = requires });

        if (vsLatest != null)
        {
            var files = GetFiles(vsLatest.FullPath + "/**/MSBuild.exe");
            if (files.Any())
            {
                msPackSettings.ToolPath = files.First();
                return true;
            }
        }
    }

    return false;
}

private void ParseVersion()
{
    if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
    {
        version = version_main + "." + EnvironmentVariable("GITHUB_RUN_NUMBER");

        if (EnvironmentVariable("GITHUB_REPOSITORY") != "stardeath/MonoGame.Extended")
            version += "-" + EnvironmentVariable("GITHUB_REPOSITORY_OWNER");
        else if (EnvironmentVariable("GITHUB_REF_TYPE") == "branch" && EnvironmentVariable("GITHUB_REF") != "refs/heads/master")
            version += "-develop";

        repositoryUrl = "https://github.com/" + EnvironmentVariable("GITHUB_REPOSITORY");
    }
    else
    {
        var branch = EnvironmentVariable("BRANCH_NAME") ?? string.Empty;    
        if (!branch.Contains("master"))
            version += "-develop";
    }

    Console.WriteLine("Version: " + version);
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    // Set MGFXC_WINE_PATH for building shaders on macOS and Linux
    System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", EnvironmentVariable("HOME") + "/.winemonogame");

    ParseVersion();

    msPackSettings = new MSBuildSettings();
    msPackSettings.Verbosity = Verbosity.Minimal;
    msPackSettings.Configuration = configuration;
    msPackSettings.Restore = true;
    msPackSettings.WithProperty("Version", version);
    msPackSettings.WithProperty("OutputDirectory", "Artifacts/NuGet");
    msPackSettings.WithProperty("RepositoryUrl", repositoryUrl);
    msPackSettings.WithTarget("Pack");

    mdPackSettings = new MSBuildSettings();
    mdPackSettings.Verbosity = Verbosity.Minimal;
    mdPackSettings.Configuration = configuration;
    mdPackSettings.WithProperty("Version", version);
    mdPackSettings.WithProperty("RepositoryUrl", repositoryUrl);
    mdPackSettings.WithTarget("PackageAddin");

    msBuildSettings = new MSBuildSettings();
    msBuildSettings.Verbosity = Verbosity.Minimal;
    msBuildSettings.Configuration = configuration;
    msBuildSettings.WithProperty("Version", version);
    msBuildSettings.WithProperty("RepositoryUrl", repositoryUrl);

    dnMsBuildSettings = new DotNetMSBuildSettings();
    dnMsBuildSettings.WithProperty("Version", version);
    dnMsBuildSettings.WithProperty("RepositoryUrl", repositoryUrl);

    dnBuildSettings = new DotNetBuildSettings();
    dnBuildSettings.MSBuildSettings = dnMsBuildSettings;
    dnBuildSettings.Verbosity = DotNetVerbosity.Minimal;
    dnBuildSettings.Configuration = configuration;

    dnPackSettings = new DotNetPackSettings();
    dnPackSettings.MSBuildSettings = dnMsBuildSettings;
    dnPackSettings.Verbosity = DotNetVerbosity.Minimal;
    dnPackSettings.OutputDirectory = "Artifacts/NuGet";
    dnPackSettings.Configuration = configuration;

    dnPublishSettings = new DotNetPublishSettings();
    dnPublishSettings.MSBuildSettings = dnMsBuildSettings;
    dnPublishSettings.Verbosity = DotNetVerbosity.Minimal;
    dnPublishSettings.Configuration = configuration;
    dnPublishSettings.SelfContained = false;
    dnPublishSettings.Sources = new List<string>() {"E:/nuget_local/"};

    dnNuGetPushSettings = new DotNetNuGetPushSettings();
    dnNuGetPushSettings.Source = "E:/nuget_local/";
});

Task("Build_MonoGame_Extended")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    DotNetRestore("src/cs/MonoGame.Extended/MonoGame.Extended.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Collisions/MonoGame.Extended.Collisions.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Content.Pipeline/MonoGame.Extended.Content.Pipeline.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Entities/MonoGame.Extended.Entities.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Graphics/MonoGame.Extended.Graphics.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Gui/MonoGame.Extended.Gui.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Input/MonoGame.Extended.Input.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Particles/MonoGame.Extended.Particles.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Tiled/MonoGame.Extended.Tiled.csproj");
    DotNetRestore("src/cs/MonoGame.Extended.Tweening/MonoGame.Extended.Tweening.csproj");

    DotNetBuild("src/cs/MonoGame.Extended/MonoGame.Extended.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Collisions/MonoGame.Extended.Collisions.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Entities/MonoGame.Extended.Entities.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Graphics/MonoGame.Extended.Graphics.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Input/MonoGame.Extended.Input.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Particles/MonoGame.Extended.Particles.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Tweening/MonoGame.Extended.Tweening.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Gui/MonoGame.Extended.Gui.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Tiled/MonoGame.Extended.Tiled.csproj");
    DotNetBuild("src/cs/MonoGame.Extended.Content.Pipeline/MonoGame.Extended.Content.Pipeline.csproj");
});

Task("Pack_MonoGame_Extended")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    PackDotnet("src/cs/MonoGame.Extended/MonoGame.Extended.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Collisions/MonoGame.Extended.Collisions.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Content.Pipeline/MonoGame.Extended.Content.Pipeline.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Entities/MonoGame.Extended.Entities.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Graphics/MonoGame.Extended.Graphics.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Gui/MonoGame.Extended.Gui.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Input/MonoGame.Extended.Input.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Particles/MonoGame.Extended.Particles.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Tiled/MonoGame.Extended.Tiled.csproj");
    PackDotnet("src/cs/MonoGame.Extended.Tweening/MonoGame.Extended.Tweening.csproj");
});

Task("Push_MonoGame_Extended")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    NuGetPushDotNet("./Artifacts/NuGet/MonoGame.Extended.3.8.1.1-develop.nupkg");
    /*
    PublishDotnet("src/cs/MonoGame.Extended/MonoGame.Extended.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Collisions/MonoGame.Extended.Collisions.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Content.Pipeline/MonoGame.Extended.Content.Pipeline.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Entities/MonoGame.Extended.Entities.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Graphics/MonoGame.Extended.Graphics.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Gui/MonoGame.Extended.Gui.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Input/MonoGame.Extended.Input.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Particles/MonoGame.Extended.Particles.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Tiled/MonoGame.Extended.Tiled.csproj");
    PublishDotnet("src/cs/MonoGame.Extended.Tweening/MonoGame.Extended.Tweening.csproj");
    */
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("SanityCheck")
    .IsDependentOn("Prep");

Task("BuildAll")
    .IsDependentOn("Build_MonoGame_Extended");

Task("Pack")
    .IsDependentOn("BuildAll")
    .IsDependentOn("Pack_MonoGame_Extended");

Task("Push")
    //.IsDependentOn("Pack")
    .IsDependentOn("Push_MonoGame_Extended");

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
