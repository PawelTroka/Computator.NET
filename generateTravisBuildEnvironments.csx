var monos = new[]{
                //"nightly",
                //"weekly",
                //"alpha",
                //"beta",
                //"latest",
                "4.6.2",
                //"4.6.1",
                //"4.6.0" ,
                "4.4.2",
                //"4.4.1","4.4.0","4.2.3","4.2.2","4.2.1","4.2.0","4.0.5","4.0.4.4","4.0.4","4.0.3","4.0.2","4.0.1",
                "4.0.0",
                "3.12.0",
                "3.10.0",
                "3.8.0",
                "3.2.8",
                "2.10.8"
                };

var oses = new[] { "linux",
                    "osx"
                 };

var sudos = new[] {
                    "required",
                    //"false"
                };

var dists = new[] { "trusty", "precise" };

var dotnets = new[] {"1.0.0-preview2-003121", "1.0.0-preview2-003131" };

var osx_images = new[] {
    "xcode8.2",//OS X 10.12
    //"xcode8.1",//OS X 10.12
    //"xcode8",//OS X 10.11
    "xcode7.3",//OS X 10.11
    //"xcode7.2",//OS X 10.11
    "xcode6.4"//OS X 10.10
    };

var osConfigs = new Dictionary<string, string[]>()
            {
                { "linux",dists},
                {"osx",osx_images }
            };

StringBuilder sb = new StringBuilder("matrix:" + Environment.NewLine);
sb.AppendLine("  include:");
Console.Write(sb.ToString());

foreach (var os in oses)
    foreach (var osConfig in osConfigs[os])
        foreach (var sudo in sudos)
        {
            foreach (var mono in monos)
            {
                if (os == "linux" && (mono.StartsWith("2") || (mono.StartsWith("3") && mono[2] == '2')))
                    continue;//versions older than 3.8.0 do not support nuget on linux and we need it bad

                if(mono== "2.10.8" && os == "osx")
                    continue;//Mono 2.10.8 is unavailable on Mac OS X

                sb = new StringBuilder($"    - os: {os}{Environment.NewLine}");
                if (os == "linux")
                    sb.AppendLine($"      dist: {osConfig}");
                if (os == "osx")
                    sb.AppendLine($"      osx_image: {osConfig}");
                sb.AppendLine($"      sudo: {sudo}");

                sb.AppendLine($"      mono: {mono}");

                if (mono.StartsWith("2.") || mono.StartsWith("3.") || (mono.StartsWith("4.") && mono[2] < '6'))
                    sb.AppendLine($"      env: netmoniker=.NET40");
                //else
                //sb.AppendLine($"      env: netmoniker=");

                Console.Write(sb.ToString());
            }

            if (osConfig != "precise")//.NET Core does not work currently on precise dist
                foreach (var dotnet in dotnets)
                {

                    sb = new StringBuilder($"    - os: {os}{Environment.NewLine}");
                    if (os == "linux")
                        sb.AppendLine($"      dist: {osConfig}");
                    if (os == "osx")
                        sb.AppendLine($"      osx_image: {osConfig}");
                    sb.AppendLine($"      sudo: {sudo}");

                    sb.AppendLine($"      dotnet: {dotnet}");
                    sb.AppendLine($"      mono: none");
                    sb.AppendLine($"      env: DOTNETCORE=1");
                    Console.Write(sb.ToString());
                }
        }