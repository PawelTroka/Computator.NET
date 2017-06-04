#addin Cake.Coveralls
#addin nuget:?package=Cake.Codecov

#tool coveralls.net
#tool nuget:?package=OpenCover
#tool nuget:?package=NUnit.ConsoleRunner
#tool nuget:?package=Codecov

//////////////////////////////////////////////////////////////////////
// ENVIRONMENTAL VARIABLES
//////////////////////////////////////////////////////////////////////
var coverallsRepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN");//"KEH5rJaqCoWoCV2MhkrMlClj3SVIlB2Eu0YK4mqmhRM+ANEfGiFyROo2RWHkJXQz"
var configuration = (EnvironmentVariable("configuration") ?? EnvironmentVariable("build_config")) ?? "Release";
var netmoniker = EnvironmentVariable("netmoniker") ?? "net461";
var travisOsName = EnvironmentVariable("TRAVIS_OS_NAME");
var dotNetCore = EnvironmentVariable("DOTNETCORE");
string monoVersion = null;

Type type = Type.GetType("Mono.Runtime");
if (type != null)
{
	var displayName = type.GetMethod("GetDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
	if (displayName != null)
		monoVersion = displayName.Invoke(null, null).ToString();
}




//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var solution = "Computator.NET" + (netmoniker != "net461" ? "."+netmoniker : "") + ".sln";
var mainProject = "Computator.NET/Computator.NET" + (netmoniker != "net461" ? "."+netmoniker : "") + ".csproj";
var unitTestsProject = "Computator.NET.Tests/Computator.NET.Tests.csproj";
var integrationTestsProject = "Computator.NET.IntegrationTests/Computator.NET.IntegrationTests.csproj";

var allTestsBinaries = "**/bin/" + configuration+ "/" + netmoniker + "/*Test*.dll";
var integrationTestsBinaries = "Computator.NET.IntegrationTests/"+"bin/" + configuration+ "/" + netmoniker + "/*Test*.dll";
var unitTestsBinaries = "Computator.NET.Tests/"+"bin/" + configuration+ "/" + netmoniker + "/*Test*.dll";



//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Install-Linux")
	.Does(() =>
{
	StartProcess("sudo", "apt-get install git-all");
	StartProcess("git", "pull");
	StartProcess("sudo", @"apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF");
	StartProcess("echo", @"""deb http://download.mono-project.com/repo/debian wheezy main"" | sudo tee  /etc/apt/sources.list.d/mono-xamarin.list");
	StartProcess("echo", @"""deb http://download.mono-project.com/repo/debian wheezy-apache24-compat main"" | sudo tee -a /etc/apt/sources.list.d/mono-xamarin.list");
	StartProcess("sudo", "apt-get install libgcc1-*");
	StartProcess("sudo", "apt-get install libc6-*");
	StartProcess("sudo", "apt-get install mono-complete");
	StartProcess("sudo", "apt-get install monodevelop --fix-missing");
	StartProcess("sudo", "apt-get install libmono-webbrowser4.0-cil");
	StartProcess("sudo", "apt-get install libgluezilla");
	StartProcess("sudo", "apt-get install curl");
	StartProcess("sudo", "apt-get install libgtk2.0-dev");
});

Task("Clean")
	.Does(() =>
{
	DeleteDirectories(GetDirectories("Computator.NET*/**/bin"), recursive:true);
	DeleteDirectories(GetDirectories("Computator.NET*/**/obj"), recursive:true);
});

Task("Restore")
	.IsDependentOn("Clean")
	.Does(() =>
{

if(dotNetCore=="1")
	StartProcess("dotnet", "restore "+solution);
else
	NuGetRestore("./"+solution);
	
if (travisOsName == "linux")
{
	StartProcess("sudo", "apt-get install libgsl2");
	System.Environment.SetEnvironmentVariable("DISPLAY", "99.0", System.EnvironmentVariableTarget.Process);//StartProcess("export", "DISPLAY=:99.0");
	StartProcess("sh", "-e /etc/init.d/xvfb start");
	System.Environment.SetEnvironmentVariable("DISPLAY", "99.0", System.EnvironmentVariableTarget.Process);//StartProcess("export", "DISPLAY=:99.0");
	StartProcess("sleep", "3");//give xvfb some time to start
}
else if(travisOsName == "osx")
{
	StartProcess("brew", "install gsl");
}

if(travisOsName=="linux" || travisOsName=="osx")
	System.Environment.SetEnvironmentVariable("MONO_WINFORMS_XIM_STYLE", "disabled", System.EnvironmentVariableTarget.Process);//StartProcess("export", "MONO_WINFORMS_XIM_STYLE=disabled");
});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
{
	if(IsRunningOnWindows() || (monoVersion!=null && (monoVersion.Contains("5.0.0")||monoVersion.Contains("5.0.1")||monoVersion.Contains("5.2.0"))))
	{
	  // Use MSBuild
	  MSBuild(mainProject, settings =>
		settings.SetConfiguration(configuration));

	  MSBuild(integrationTestsProject, settings =>
		settings.SetConfiguration(configuration));

	  MSBuild(unitTestsProject, settings =>
		settings.SetConfiguration(configuration));
	}
	else
	{
	  // Use XBuild
	  XBuild(mainProject, settings =>
		settings.SetConfiguration(configuration));

	  XBuild(integrationTestsProject, settings =>
		settings.SetConfiguration(configuration));

	  XBuild(unitTestsProject, settings =>
		settings.SetConfiguration(configuration));
	}
});

Task("UnitTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(unitTestsBinaries, new NUnit3Settings() {
		Labels = NUnit3Labels.All,
		//NoResults = true
		});
});

Task("IntegrationTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(integrationTestsBinaries, new NUnit3Settings() {
		Labels = NUnit3Labels.All,
		//NoResults = true
		});
});

Task("AllTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(allTestsBinaries, new NUnit3Settings() {
		Labels = NUnit3Labels.All,
		//NoResults = true
		});
});

Task("Calculate-Coverage")
	.IsDependentOn("Build")
	.Does(() =>
{
	OpenCover(tool => {
  tool.NUnit3(allTestsBinaries,
	new NUnit3Settings {
	  NoResults = true,
	  //InProcess = true,
	  //Domain = Domain.Single,
	  Where = "cat!=LongRunningTests",
	  ShadowCopy = false,
	});
  },
  new FilePath("coverage.xml"),
  (new OpenCoverSettings()
	{
		Register="user",
		SkipAutoProps = true,
		
	})
	.WithFilter("+[Computator.NET*]*")
	.WithFilter("-[Computator.NET.Core]Computator.NET.Core.Properties.*")
	.WithFilter("-[Computator.NET.Tests]*")
	.WithFilter("-[Computator.NET.IntegrationTests]*")
	.ExcludeByAttribute("*.ExcludeFromCodeCoverage*"));
});

Task("Upload-Coverage")
	.IsDependentOn("Calculate-Coverage")
	.Does(() =>
{
	CoverallsNet("coverage.xml", CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
	{
		RepoToken = coverallsRepoToken
	});

	Codecov("coverage.xml");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("UnitTests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
