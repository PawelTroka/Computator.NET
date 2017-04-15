@RD /S /Q Computator.NET\bin\Release
@RD AppPackages\PackageFiles
nuget restore Computator.NET.sln
msbuild Computator.NET.DataTypes\Computator.NET.DataTypes.csproj
msbuild Computator.NET.Core\Computator.NET.Core.csproj
msbuild Computator.NET.Charting\Computator.NET.Charting.csproj
msbuild Computator.NET\Computator.NET.csproj
xcopy /s Computator.NET\bin\Release AppPackages\PackageFiles
xcopy build-uwp\AppxManifest.xml AppPackages\PackageFiles\AppxManifest.xml
xcopy /s Computator.NET.Core\Special\windows-x64 AppPackages\PackageFiles
xcopy /s Graphics\Assets AppPackages\PackageFiles\Assets
xcopy /s "Computator.NET.Core\TSL Examples" "AppPackages\PackageFiles\VFS\Users\ContainerAdministrator\Documents\Computator.NET\TSL Examples"
xcopy /s Computator.NET.Core\Static\fonts AppPackages\PackageFiles\VFS\Windows\Fonts
makeappx pack -d AppPackages\PackageFiles -p AppPackages\Computator.NET.appx
signtool.exe sign -f build-uwp\Computator.NET_TemporaryKey.pfx -fd SHA256 -v .\AppPackages\Computator.NET.appx