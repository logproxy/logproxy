#addin nuget:?package=Cake.Docker&version=1.0.0

var target = Argument("target", "DockerImageBuild");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory($"./artifacts");
    CleanDirectory($"./LogProxy/bin/{configuration}");
    CleanDirectory($"./LogProxy.UnitTests/bin/{configuration}");
    CleanDirectory($"./LogProxy.IntegrationTests/bin/{configuration}");
    DockerRemove(new DockerImageRemoveSettings(), "logproxy/logproxy:latest");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./LogProxy.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./LogProxy.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetCorePublish("./LogProxy/LogProxy.csproj", new DotNetCorePublishSettings
    {
        Configuration = configuration,
    	OutputDirectory = "./artifacts"
    });
});

Task("DockerImageBuild")
    .IsDependentOn("Publish")
    .Does(() =>
{
    DockerBuild(new DockerImageBuildSettings
    {
    	Tag = new string[] {"logproxy/logproxy:latest"}
    }, "./artifacts/");
});


RunTarget(target);
