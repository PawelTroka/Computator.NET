sudo apt-get install git-all
git pull
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
echo "deb http://download.mono-project.com/repo/debian wheezy-apache24-compat main" | sudo tee -a /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get install libgcc1-*
sudo apt-get install libc6-*
sudo apt-get install mono-complete
sudo apt-get install monodevelop --fix-missing
sudo apt-get install libmono-webbrowser4.0-cil
sudo apt-get install libgluezilla
sudo apt-get install curl
sudo apt-get install libgtk2.0-dev