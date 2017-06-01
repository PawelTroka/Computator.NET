if [ -z "$build_config" ]; then export build_config="Release"; fi #default is Release

sudo apt-get install libgtk2.0-dev
curl -sS https://api.nuget.org/packages/mono.cecil.0.9.5.4.nupkg > /tmp/mono.cecil.0.9.5.4.nupkg.zip
unzip /tmp/mono.cecil.0.9.5.4.nupkg.zip -d /tmp/cecil
cp /tmp/cecil/lib/net40/Mono.Cecil.dll .
cp /tmp/cecil/lib/net40/Mono.Cecil.dll /tmp/cecil/
git clone --depth=50 git://github.com/csMACnz/monocov.git ../csMACnz/monocov
cd ../csMACnz/monocov
cp /tmp/cecil/Mono.Cecil.dll .
./configure
make
sudo make install
cd ../../Computator.NET

nuget install NUnit.Console -Version 3.6.1 -OutputDirectory testrunner
nuget install coveralls.net -Version 0.7.0 -OutputDirectory codecoveragetools

export LD_LIBRARY_PATH=/usr/local/lib
mono --debug --profile=monocov:outfile=monocovCoverage.cov,+[Computator.NET*]*,-[Computator.NET.Core]Computator.NET.Core.Properties.*,-[Computator.NET.Tests]*,-[Computator.NET.IntegrationTests]* ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe --noresult --inprocess --domain=Single --where:cat!=LongRunningTests ""Computator.NET.Tests/bin/"$build_config"/"$netmoniker"/Computator.NET.Tests.dll"" ""Computator.NET.IntegrationTests/bin/"$build_config"/"$netmoniker"/Computator.NET.IntegrationTests.dll""
monocov --export-xml=monocovCoverage monocovCoverage.cov

REPO_COMMIT_AUTHOR=$(git show -s --pretty=format:"%cn")
REPO_COMMIT_AUTHOR_EMAIL=$(git show -s --pretty=format:"%ce")
REPO_COMMIT_MESSAGE=$(git show -s --pretty=format:"%s")
echo $TRAVIS_COMMIT
echo $TRAVIS_BRANCH
echo $REPO_COMMIT_AUTHOR
echo $REPO_COMMIT_AUTHOR_EMAIL
echo $REPO_COMMIT_MESSAGE
echo $TRAVIS_JOB_ID

mono ./codecoveragetools/coveralls.net.0.7.0/tools/csmacnz.Coveralls.exe --monocov -i ./monocovCoverage.cov --repoTokenVariable COVERALLS_REPO_TOKEN --commitId "$TRAVIS_COMMIT" --commitBranch "$TRAVIS_BRANCH" --commitAuthor "$REPO_COMMIT_AUTHOR" --commitEmail "$REPO_COMMIT_AUTHOR_EMAIL" --commitMessage "$REPO_COMMIT_MESSAGE" --jobId "$TRAVIS_JOB_ID"  --serviceName "travis-ci"  --useRelativePaths