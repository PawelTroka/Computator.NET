#addin Cake.Coveralls
#addin nuget:?package=Cake.Codecov
#addin nuget:?package=Cake.AppPackager
#addin nuget:?package=Cake.VersionReader

#tool coveralls.net
#tool nuget:?package=OpenCover
#tool nuget:?package=NUnit.ConsoleRunner
#tool nuget:?package=Codecov

//////////////////////////////////////////////////////////////////////
// ENVIRONMENTAL VARIABLES
//////////////////////////////////////////////////////////////////////
var coverallsRepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN");//"KEH5rJaqCoWoCV2MhkrMlClj3SVIlB2Eu0YK4mqmhRM+ANEfGiFyROo2RWHkJXQz"
var configuration = (EnvironmentVariable("configuration") ?? EnvironmentVariable("build_config")) ?? "Release";
var travisOsName = EnvironmentVariable("TRAVIS_OS_NAME");
var dotNetCore = EnvironmentVariable("DOTNETCORE");
string monoVersion = null;
string monoVersionShort = null;

Type type = Type.GetType("Mono.Runtime");
if (type != null)
{
	var displayName = type.GetMethod("GetDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
	if (displayName != null)
	{
		monoVersion = displayName.Invoke(null, null).ToString();
		Information("Mono version is "+monoVersion);
		monoVersionShort = string.Join(".",System.Text.RegularExpressions.Regex.Match(monoVersion,@"(\d+\.\d+(?:\.\d+(?:\.\d+)?)?)").Value.Split(".".ToCharArray(),StringSplitOptions.RemoveEmptyEntries).Take(3));
		Information("Mono version short is "+monoVersionShort);
	}
}

var isMonoButSupportsMsBuild = monoVersion!=null && System.Text.RegularExpressions.Regex.IsMatch(monoVersion,@"([5-9]|\d{2,})\.\d+\.\d+(\.\d+)?");


var normalNUnit3Settings = new NUnit3Settings()
{
	DisposeRunners = true,
	Configuration = configuration,
	Labels = NUnit3Labels.All,
	//NoResults = true
};


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var solution = "Computator.NET.sln";
var mainProject = "Computator.NET/Computator.NET.csproj";
var mainProjectNet40 = "Computator.NET/Computator.NET.net40.csproj";
var mainProjectBinPath = @"Computator.NET/bin/" + configuration + @"/v4.6.1";
var mainProjectBinPathNet40 = @"Computator.NET/bin/" + configuration + @"/v4.0";

var installerProject = "Computator.NET.Installer/Computator.NET.Installer.csproj";
var unitTestsProject = "Computator.NET.Core.Tests/Computator.NET.Core.Tests.csproj";
var integrationTestsProject = "Computator.NET.Core.IntegrationTests/Computator.NET.Core.IntegrationTests.csproj";
var webApiIntegrationTestsProject = "Computator.NET.WebApi.IntegrationTests/Computator.NET.WebApi.IntegrationTests.csproj";

var allTestsBinaries = "**/bin/" + configuration+ "/**/Computator.NET*Test*.dll";
var integrationTestsBinaries = "Computator.NET*.IntegrationTests/"+"bin/" + configuration+ "/**/Computator.NET*Test*.dll";
var unitTestsBinaries = "Computator.NET*.Tests/"+"bin/" + configuration+ "/**/Computator.NET*Test*.dll";

var allWebTestsBinaries = "Computator.NET.Web*.*Tests*/"+"bin/" + configuration+ "/**/Computator.NET*Test*.dll";

var msBuildSettings = new MSBuildSettings {
	Verbosity = Verbosity.Minimal,
	ToolVersion = MSBuildToolVersion.Default,//The highest available MSBuild tool version//VS2017
	Configuration = configuration,
	PlatformTarget = PlatformTarget.MSIL,
	MSBuildPlatform = MSBuildPlatform.Automatic,
	DetailedSummary = true,
	};

	if(!IsRunningOnWindows() && isMonoButSupportsMsBuild)
	{
		if(travisOsName == "osx" || System.Environment.OSVersion.Platform == System.PlatformID.MacOSX)
		{
			var msBuildVersions = new [] {"15.3","15.2","15.1","15.0","14.1","14.0"};
			var monoVersions = new [] {"Current", monoVersionShort};
			var msBuildExecutableNames = new [] {"MSBuild.exe", "MSBuild.dll", "msBuild.exe", "msbuild.exe", "msBuild.dll", "msbuild.dll"};

			var msBuildPossiblePaths = new List<string>();

			foreach(var msBuildVersion in msBuildVersions)
			{
				foreach(var msBuildExecutableName in msBuildExecutableNames)
				{
					foreach(var chosenMonoVersion in monoVersions)
					{
						msBuildPossiblePaths.Add(@"/Library/Frameworks/Mono.framework/Versions/" + chosenMonoVersion + @"/lib/mono/msbuild/" + msBuildVersion +@"/bin/"+msBuildExecutableName);
						msBuildPossiblePaths.Add(@"/Library/Frameworks/Mono.framework/Versions/" + chosenMonoVersion + @"/bin/mono/msbuild/" + msBuildVersion +@"/bin/"+msBuildExecutableName);
					}
					msBuildPossiblePaths.Add(@"/usr/bin/mono/msbuild/"+msBuildVersion+@"/bin/"+msBuildExecutableName);
					msBuildPossiblePaths.Add(@"/usr/local/bin/mono/msbuild/"+msBuildVersion+@"/bin/"+msBuildExecutableName);

					msBuildPossiblePaths.Add(@"/usr/lib/mono/msbuild/"+msBuildVersion+@"/bin/"+msBuildExecutableName);
					msBuildPossiblePaths.Add(@"/usr/local/lib/mono/msbuild/"+msBuildVersion+@"/bin/"+msBuildExecutableName);
				}
			}

			var msBuildPath = msBuildPossiblePaths.FirstOrDefault(p => FileExists(p));
			if(msBuildPath!=null)
			{
				Information("MSBuild path is "+msBuildPath);
				msBuildSettings.ToolPath = new FilePath(msBuildPath);
			}
			else
				Error("MSBuild path not found!");
		}
		else
			msBuildSettings.ToolPath = new FilePath(@"/usr/lib/mono/msbuild/15.0/bin/MSBuild.dll");//hack for Linux bug - missing MSBuild path
	}

var xBuildSettings = new XBuildSettings {
	Verbosity = Verbosity.Minimal,
	ToolVersion = XBuildToolVersion.Default,//The highest available XBuild tool version//NET40
	Configuration = configuration,
	//PlatformTarget = PlatformTarget.MSIL,
	};

var monoEnvVars = new Dictionary<string,string>() { {"DISPLAY", "99.0"},{"MONO_WINFORMS_XIM_STYLE", "disabled"} };

if(!IsRunningOnWindows())
{
	if(msBuildSettings.EnvironmentVariables==null)
		msBuildSettings.EnvironmentVariables = new Dictionary<string,string>();
	if(xBuildSettings.EnvironmentVariables==null)
		xBuildSettings.EnvironmentVariables = new Dictionary<string,string>();

	foreach(var monoEnvVar in monoEnvVars)
	{
		msBuildSettings.EnvironmentVariables.Add(monoEnvVar.Key,monoEnvVar.Value);
		xBuildSettings.EnvironmentVariables.Add(monoEnvVar.Key,monoEnvVar.Value);
	}
}

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
	DeleteDirectories(GetDirectories("AppPackages"), recursive:true);
	//DeleteDirectories(GetDirectories("publish"), recursive:true);
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
	
	var xvfvProcessSettings = new ProcessSettings() { Arguments="--start --quiet --pidfile /tmp/custom_xvfb_99.pid --make-pidfile --background --exec /usr/bin/Xvfb -- :99 -ac -screen 0 1280x1024x16" };
	if(xvfvProcessSettings.EnvironmentVariables==null)
		xvfvProcessSettings.EnvironmentVariables=new Dictionary<string,string>();
	foreach(var envVar in monoEnvVars)
		xvfvProcessSettings.EnvironmentVariables.Add(envVar.Key, envVar.Value);
	StartProcess(@"/sbin/start-stop-daemon", xvfvProcessSettings);
	
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
	if(IsRunningOnWindows() || isMonoButSupportsMsBuild)
	{
	  // Use MSBuild
	  MSBuild(mainProject, msBuildSettings);
	  MSBuild(mainProjectNet40, msBuildSettings);
	  MSBuild(unitTestsProject, msBuildSettings);
	  MSBuild(integrationTestsProject, msBuildSettings);
	  MSBuild(webApiIntegrationTestsProject, msBuildSettings);
	  
	}
	else
	{
	  // Use XBuild
	  XBuild(mainProject, xBuildSettings);
	  XBuild(mainProjectNet40, xBuildSettings);
	  XBuild(unitTestsProject, xBuildSettings);
	  XBuild(integrationTestsProject, xBuildSettings);
	  XBuild(webApiIntegrationTestsProject, xBuildSettings);
	}
});

Task("UnitTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(unitTestsBinaries, normalNUnit3Settings);
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		AppVeyor.UploadTestResults("./TestResult.xml", AppVeyorTestResultsType.NUnit3);
	}
});

Task("IntegrationTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(integrationTestsBinaries, normalNUnit3Settings);
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		AppVeyor.UploadTestResults("./TestResult.xml", AppVeyorTestResultsType.NUnit3);
	}
});

Task("AllTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(allTestsBinaries, normalNUnit3Settings);
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		AppVeyor.UploadTestResults("./TestResult.xml", AppVeyorTestResultsType.NUnit3);
	}
});

Task("WebTests")
	.IsDependentOn("Build")
	.Does(() =>
{
	NUnit3(allWebTestsBinaries, normalNUnit3Settings);
	if(AppVeyor.IsRunningOnAppVeyor)
	{
		AppVeyor.UploadTestResults("./TestResult.xml", AppVeyorTestResultsType.NUnit3);
	}
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
	.WithFilter("-[Computator.NET.Localization]*")
	.WithFilter("-[Computator.NET.Core.Tests]*")
	.WithFilter("-[Computator.NET.Core.IntegrationTests]*")
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


Task("Build-Installer")
	.IsDependentOn("Build")
	.Does(() =>
{
	if(IsRunningOnWindows())
	{
		  // Use MSBuild
		  //msBuildSettings.ArgumentCustomization=null;
		  MSBuild(installerProject, new MSBuildSettings
		  {
			Verbosity = Verbosity.Minimal,
			ToolVersion = MSBuildToolVersion.Default,//The highest available MSBuild tool version//VS2017
			Configuration = configuration,
			PlatformTarget = PlatformTarget.MSIL,
			MSBuildPlatform = MSBuildPlatform.Automatic,
			DetailedSummary = true,
		  });
	}
	else
	{
		Warning("Building installer is currently not supported on Unix");
	}
});

Task("Build-Uwp")
	.IsDependentOn("Build")
	.Does(() =>
{
	if(IsRunningOnWindows())
	{
		var packageFiles = @"AppPackages/PackageFiles";
		
		CopyDirectory(mainProjectBinPath,packageFiles);
		CopyFileToDirectory(@"build-uwp/AppxManifest.xml",packageFiles);
		CopyFileToDirectory(@"build-uwp/Registry.dat",packageFiles);

		CopyDirectory(@"Computator.NET.Core/Special/windows-x64",packageFiles);
		CopyDirectory(@"Graphics/Assets",packageFiles+@"/Assets");
		
		CopyDirectory(@"Computator.NET.Core/TSL Examples",packageFiles+@"/VFS/Users/ContainerAdministrator/Documents/Computator.NET/TSL Examples");
		CopyDirectory(@"Computator.NET.Core/Static/fonts",packageFiles+@"/VFS/Windows/Fonts");

		var programFilesPath = System.Environment.Is64BitOperatingSystem  ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		var makePriPaths = GetFiles(programFilesPath + @"\Windows Kits\10\bin\x86\makepri.exe");
		var makePriPath = makePriPaths.First();
		StartProcess(makePriPath, @"createconfig /cf AppPackages\PackageFiles\priconfig.xml /dq en-US");//makepri createconfig /cf AppPackages\PackageFiles\priconfig.xml /dq en-US
		StartProcess(makePriPath, @"new /pr AppPackages\PackageFiles /cf AppPackages\PackageFiles\priconfig.xml");//makepri new /pr AppPackages\PackageFiles /cf AppPackages\PackageFiles\priconfig.xml	
		
		MoveFiles(@"*.pri", packageFiles);// move /y .\*.pri AppPackages\PackageFiles
		
		AppPack("AppPackages/Computator.NET.appx", new DirectoryPath("AppPackages/PackageFiles"));		
		Sign(GetFiles("AppPackages/*.appx"), new SignToolSignSettings {
			TimeStampUri = new Uri("http://timestamp.digicert.com"),
			DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
			CertPath = @"build-uwp/Computator.NET_TemporaryKey.pfx",
		});
	}
	else
	{
		Warning("Building Universal Windows App is currently not supported on Unix");
	}
});


Task("Publish")
	.IsDependentOn("Build")
	.Does(() =>
{
	DeleteDirectories(GetDirectories("publish"), recursive:true);

	var versionNumber = GetFullVersionNumber(mainProjectBinPath+@"/Computator.NET.exe");
	var versionNumberNet40 = GetFullVersionNumber(mainProjectBinPathNet40+@"/Computator.NET.exe");
	if(versionNumber!=versionNumberNet40)
		Error("Version numbers for main executable differ for normal and NET40 build!");

	EnsureDirectoryExists("publish");

	var namesSuffix = ".v" + versionNumber + ".";
	var namesSuffixNet40 = ".v" + versionNumber + (IsRunningOnWindows() ? "-WindowsXP" : "-net40") + ".";

	//Publish Portable
	var zipPublishPath = @"publish/Computator.NET" + namesSuffix + "zip";
	var zipPublishPathNet40 = @"publish/Computator.NET" + namesSuffixNet40 + "zip";

	Zip(mainProjectBinPath, zipPublishPath);
	Zip(mainProjectBinPathNet40, zipPublishPathNet40);

	if(AppVeyor.IsRunningOnAppVeyor)
	{
		AppVeyor.UploadArtifact(zipPublishPath);
		AppVeyor.UploadArtifact(zipPublishPathNet40);
	}
	else if(TravisCI.IsRunningOnTravisCI)
		Warning("Publishing artifacts in TravisCI is not yet supported.");

	//Publish UWP
	if(IsRunningOnWindows())
	{
		var appxPublishPath = "publish/Computator.NET" + namesSuffix + "appx";
		RunTarget("Build-Uwp");
		MoveFile(@"AppPackages/Computator.NET.appx", appxPublishPath);
		if(AppVeyor.IsRunningOnAppVeyor)
		{
			AppVeyor.UploadArtifact(appxPublishPath);
		}
	}
	else
		Warning("Publishing UWP app is not supported on Unix.");
	
	//Publish Installer
	if(IsRunningOnWindows())
	{
		RunTarget("Build-Installer");
		var installerPublishPath = @"publish/Computator.NET.Installer" + namesSuffix + "exe";
		MoveFile(GetFiles(@"Computator.NET.Installer/bin/" + configuration + @"/Computator.NET.Installer.exe").Single(),installerPublishPath);
		if(AppVeyor.IsRunningOnAppVeyor)
			AppVeyor.UploadArtifact(installerPublishPath);
	}
	else
		Warning("Publishing Installer is not supported on Unix.");
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
