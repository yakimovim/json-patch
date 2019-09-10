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
var nuGetDir = Directory("./nuget");
var solutionFile = "./JsonPatch.sln";

NuGetPackSettings CreateNuGetTemplate() {
    return new NuGetPackSettings {
        Version                 = "1.0.3",
        Title                   = "EdlinSoftware JsonPatch library",
        Authors                 = new[] { "Ivan Iakimov" },
        // Owners                  = new[] {"Contoso"},
        Description             = "Implementation of Json patch specification using Newtosoft Json library",
        Summary                 = "This library allows to apply JSON patch operations to JToken objects or to POCO objects",
        ProjectUrl              = new Uri("https://github.com/yakimovim/json-patch"),
        IconUrl                 = new Uri("https://raw.githubusercontent.com/yakimovim/json-patch/master/json.png"),
        LicenseUrl              = new Uri("https://raw.githubusercontent.com/yakimovim/json-patch/master/LICENSE"),
        Copyright               = "EdlinSoftware 2019",
        //ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
        Tags                    = new [] { "JSON" },
        RequireLicenseAcceptance= false,
        Symbols                 = false,
        NoPackageAnalysis       = true,
        BasePath                = buildDir,
        OutputDirectory         = nuGetDir,
        Dependencies            = {
            new NuSpecDependency {
                Id = "NETStandard.Library",
                Version = "1.6.1",
                Exclude = new [] { "Build", "Analyzers" },
                TargetFramework	= ".NETStandard1.2"
            },
            new NuSpecDependency {
                Id = "Newtonsoft.Json",
                Version = "12.0.2",
                Exclude = new [] { "Build", "Analyzers" },
                TargetFramework	= ".NETStandard1.2"
            }
        }
    };
}

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

Task("NuGet")
    .IsDependentOn("Sign-Strong-Name")
    .Does(() =>
{
    CleanDirectory(nuGetDir);

    // Create package with unsigned library
    var nuGetPackSettings = CreateNuGetTemplate();
    nuGetPackSettings.Id = "EdlinSoftware.JsonPatch";
    nuGetPackSettings.Files = new [] {
        new NuSpecContent {
            Source = "EdlinSoftware.JsonPatch.dll",
            Target = "lib/netstandard1.2"
        },
    };
    NuGetPack(nuGetPackSettings);

    // Create package with signed library
    nuGetPackSettings = CreateNuGetTemplate();
    nuGetPackSettings.Id = "EdlinSoftware.JsonPatch.Signed";
    nuGetPackSettings.Files = new [] {
        new NuSpecContent {
            Source = "Signed/EdlinSoftware.JsonPatch.dll",
            Target = "lib/netstandard1.2"
        },
    };
    NuGetPack(nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
