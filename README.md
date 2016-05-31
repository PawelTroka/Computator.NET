# Computator.NET
Computator.NET is a unique open numerical software that is fast and easy to use and stands up to other feature-wise software.
![Quick presentation of Computator.NET features](../master/Presentation/quick_presentation.gif "Quick presentation of Computator.NET features")

###Its features include:
* **Real**, **complex** and two-variable (**3D**) function **charts**
* **Real**, **complex** and two-variable (**3D**) function **calculator**
* **Numerical calculations** on real functions including many different methods
* Over 107 **Elementary functions**
* Over 141 **Special functions**
* Over 21 **Matrix functions and operations**
* Original **scripting language** designed for scientists, powerful for easy computations including matrices
* User-created **custom functions** written with scripting language
* **Mathematical notation**, including raising to power, just like it should be



# Release 2.0 beta
*This release is focused on removing all the limitations and inconsistencies that have been going throught Computator.NET history. Be careful, though it may be unstable, because of all the new features and huge changes to application architecture itself.*
* View [changelog here](https://github.com/PawelTroka/Computator.NET/releases/tag/v2.0.0-beta)
* Download [installer here](https://github.com/PawelTroka/Computator.NET/releases/download/v2.0.0-beta/Computator.NET.Installer.v2.0.0.beta.exe)
* Download [portable version (*.zip) here](https://github.com/PawelTroka/Computator.NET/releases/download/v2.0.0-beta/Computator.NET.v2.0.0-beta.zip)
* Alternatively [download installer here](http://fizyka.dk/Computator.NET.Installer%20v2.0.0%20beta.exe)
* Alternatively [download portable version (*.zip) here](http://fizyka.dk/Computator.NET v2.0.0-beta.zip)


# Installation & Setup
1. This project requires **.NET Framework** at least in version **4.0 Full**, you can download it here (https://www.microsoft.com/en-US/download/details.aspx?id=17718).
2. Go to [latest release page](https://github.com/PawelTroka/Computator.NET/releases/latest)
3. Now you can download either portable version (the one with .zip extension) or installer (the one with .exe extension). In general, installer is recommended, because it sets up tsl and tslf files extensions for Computator.NET in registry.
4. If you downloaded portable version, just unzip it into location you want to store and you are ready to go.
5. If you downloaded version with installer, just click on the installer file, it will guide you through the whole process and download all required files and libraries.
6. After installation or unzip, if you want to run it, simply click on Computator.NET.exe (or Computator.NET shortcut on desktop if you had chosen installer) in whatever location you had chosen for it.



# Contributing

#####Programmers
* This repo supports **Fork & Pull Model**, you can do any useful changes and when you are done make a pull request. If your work is of good quality, it will be included in main branch.
* List of **things to do** is **updated constantly**, you can view it here: [TODO file](../master/TODO). Choose wisely because some of those things are way harder than it looks.
* List of known bugs (often with little or no reasonable solutions) is here: [BUGS file](../master/BUGS).

#####Testers
* Our **unit tests coverage is small**, partly because of the fact that there is good chunk of UI code here. Still I am afraid that there are lots and lots of not found bugs. If you want to help us fix those bugs, first try to catch them by **writing good unit tests**.

#####Users
* If **you have found ANY bugs** or you think something might be bug please report it in [**issues**](https://github.com/PawelTroka/Computator.NET/issues) as fast as you can. Any help with testing / finding bugs is greatly appreciated.


#Screenshots
![Computator.NET v1.8 is coming - functions with description](../master/Presentation/Computator.NET%20v1.8%20is%20coming%20-%20functions%20with%20description.jpg "Computator.NET v1.8 is coming - functions with description")
![Computator.NET v1.8 is coming 2 - writing in exponent, f(x,y) chart](../master/Presentation/Computator.NET%20v1.8%20is%20coming%202%20-%20writing%20in%20exponent,%20f(x,y)%20chart.jpg "Computator.NET v1.8 is coming 2 - writing in exponent, f(x,y) chart")
![functions and constants details](../master/Presentation/functions%20and%20constants%20details.jpg "functions and constants details")
![Interesting implicit function](../master/Presentation/interesting%20implicit%20function.jpg "Interesting implicit function")
![Simple implicit function](../master/Presentation/simple%20implicit%20function.jpg "Simple implicit function")
![Euler Gamma by Computator.NET](../master/Presentation/gamma%20by%20computator.net.jpg "Euler Gamma by Computator.NET")

# FAQ

###What shortcut do you use to write in exponent, just like in gif presentation?

LShift+6 (^), just like it is listed in **Edit** menu from where you can also **activate / deactivate** writing in exponent without using this shortcut.
![Writing in exponent](../master/Presentation/writing-in-exponent.gif "Writing in exponent")


###How can I solve equations / do my homework using Computator.NET?

Computator.NET is numerical software that aims to provide functionality similar to Matlab and Mathematica. But it certainly isn't full Computer Algebra System, at least not yet. You can solve some equations by using '**Numerical calculations**' tab and '**Function root**' operation. Depending on the equation, different methods work better or worse.
![Solving equations using numerical calculations](../master/Presentation/solving%20equations%20using%20numerical%20calculations.gif "Solving equations using numerical calculations")
Alternatively, you can always solve equations graphically, simply by looking at the chart.
![solving equations graphically](../master/Presentation/solving%20equations%20graphically.gif "solving equations graphically")


# Minimum system requirements

* **.NET Framework** **4.0 Full** or later (https://www.microsoft.com/en-US/download/details.aspx?id=17718) (included by installer)
* .NET 4.0 **KB2468871** update (included by installer)
* **Microsoft Visual C++ 2015 Update 2 Redistributable Package** (vc_redist 14.0.23918) (included by installer)
* Operating system **Windows XP SP3** or later (**Windows Vista** or later is recommended, **Windows 10** is the best option because of continous testing on it)
* Processor **1 GHz** or faster
* Memory **512 MB**	or more
* **DirectX 9.0** support
* Graphics memory **128 MB** or more
* Free HDD space **200 MB** or more


# License

Computator.NET along with all content files, TSL/TSLF scripts and any work shared on this github project page is [GNU GPL v3 licensed](../master/LICENSE). Any libraries used by Computator.NET are GNU GPL v3 - compatible.
