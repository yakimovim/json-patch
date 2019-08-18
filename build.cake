#tool "nuget:?package=Brutal.Dev.StrongNameSigner&version=2.3.0"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./build") + Directory(configuration);
var solutionFile = "./JsonPatch.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild(
        solutionFile,
        new DotNetCoreBuildSettings {
            Configuration = configuration,
            OutputDirectory = buildDir
        }
    );
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testProjects = GetFiles("./test/**/*.Tests.csproj");
    foreach(var testProject in testProjects)
    {
        DotNetCoreTest(
            testProject.FullPath,
            new DotNetCoreTestSettings {
                Configuration = configuration
            }
        );
    }
});

Task("Sign-Strong-Name")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    CleanDirectory(buildDir + Directory("Signed"));

    var strongNameSignerExe = "./tools/Brutal.Dev.StrongNameSigner.2.3.0/build/StrongNameSigner.Console.exe";

    using(var process = StartAndReturnProcess(strongNameSignerExe,
        new ProcessSettings {
            Arguments = $"-a \"{buildDir}/EdlinSoftware.JsonPatch.dll\" -k EdlinSoftware.snk -out \"{buildDir}/Signed\""
        }))
    {
        process.WaitForExit();

        // This should output 0 as valid arguments supplied
        var exitCode = process.GetExitCode();
        Information("Exit code: {0}", exitCode);

        if(exitCode != 0)
        {
            throw new Exception("Unable to create strong name signed version of the assembly");
        }
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Sign-Strong-Name");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
