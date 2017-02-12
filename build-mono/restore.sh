#if [ "$TRAVIS_OS_NAME" == "osx" ]; then brew update; brew install openssl; ln -s /usr/local/opt/openssl/lib/libcrypto.1.0.0.dylib /usr/local/lib/; ln -s /usr/local/opt/openssl/lib/libssl.1.0.0.dylib /usr/local/lib/; fiopenssl; fi

if [ "$TRAVIS_OS_NAME" == "linux" ] || [ "$TRAVIS_OS_NAME" == "osx" ]; then "export DISPLAY=:99.0"; fi

if [ "$TRAVIS_OS_NAME" == "linux" ]; then sh -e /etc/init.d/xvfb start; fi
if [ "$TRAVIS_OS_NAME" == "linux" ]; then sleep 3; fi # give xvfb some time to start

if [ "$DOTNETCORE" != "1" ]; then nuget restore Computator.NET"$netmoniker".sln; else dotnet restore; fi
nuget install NUnit.Console -Version 3.6.0 -OutputDirectory testrunner

export MONO_WINFORMS_XIM_STYLE=disabled