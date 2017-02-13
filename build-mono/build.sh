if [ -z "$build_config" ]; then export build_config="Release"; fi #default is Release

xbuild /p:Configuration="$build_config" Computator.NET.DataTypes/Computator.NET.DataTypes"$netmoniker".csproj
xbuild /p:Configuration="$build_config" Computator.NET.Charting/Computator.NET.Charting"$netmoniker".csproj
xbuild /p:Configuration="$build_config" Computator.NET.Core/Computator.NET.Core"$netmoniker".csproj
xbuild /p:Configuration="$build_config" Computator.NET/Computator.NET"$netmoniker".csproj
xbuild /p:Configuration="$build_config" Computator.NET.Tests/Computator.NET.Tests.csproj
xbuild /p:Configuration="$build_config" Computator.NET.IntegrationTests/Computator.NET.IntegrationTests.csproj