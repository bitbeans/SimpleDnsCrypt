[![license](https://img.shields.io/github/license/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/blob/master/LICENSE.md) [![Build status](https://img.shields.io/appveyor/ci/bitbeans/simplednscrypt/master.svg?style=flat-square)](https://ci.appveyor.com/project/bitbeans/simplednscrypt/branch/master) [![Github All Releases](https://img.shields.io/github/release/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![dnscrypt--proxy](https://img.shields.io/badge/dnscrypt--proxy-2.0.6-orange.svg?style=flat-square)](https://github.com/jedisct1/dnscrypt-proxy) [![Github All Releases](https://img.shields.io/github/downloads/bitbeans/SimpleDnsCrypt/total.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![donate PayPal](https://img.shields.io/badge/donate-PayPal-green.svg?style=flat-square)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=c.hermann@bitbeans.de&item_name=Donation+to+the+Simple+DNSCrypt+project) [![donate pledgie](https://img.shields.io/badge/donate-pledgie-green.svg?style=flat-square)](https://pledgie.com/campaigns/32588)

# ![Simple DNSCrypt](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/icons/256x256.png)

# Simple DNSCrypt
Simple DNSCrypt is a simple management tool to configure [dnscrypt-proxy](https://github.com/jedisct1/dnscrypt-proxy) on windows based systems. 

## Status

New version based on dnscrypt-proxy 2.0.6

### Preview Download (CI)

[Download x86 (preview, unsigned portable version)](https://simplednscrypt.blob.core.windows.net/deploy/SimpleDnsCrypt/bin/x86/SimpleDNSCrypt_x86.zip) - *AppVeyor Build*

[Download x64 (preview, unsigned portable version)](https://simplednscrypt.blob.core.windows.net/deploy/SimpleDnsCrypt/bin/x64/SimpleDNSCrypt_x64.zip) - *AppVeyor Build*


## Getting Started


### Prerequisites

At least one system with Windows 7 SP1 and the installation of. NET Framework 4.6.1 is currently required.

You also will need: Microsoft Visual C++ Redistributable for Visual Studio 2017 [x64](https://aka.ms/vs/15/release/VC_redist.x64.exe) or [x86](https://aka.ms/vs/15/release/VC_redist.x86.exe)


### Installing

To install Simple DNSCrypt use the latest (stable) MSI packages: [x86](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.5.3/SimpleDNSCrypt.msi) or [x64](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.5.3/SimpleDNSCrypt64.msi).

### File Signing

The MSI package and the SimpleDnsCrypt.exe are signed via a *COMODO RSA Code Signing CA*. 
The files are signed under the name: *EAM Experience Area Münsingen GmbH*

You also can verify the MSI packages with [minisign](https://jedisct1.github.io/minisign/).
The minisign signatures [x86](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.5.3/SimpleDNSCrypt.msi.minisig) and [x64](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.5.3/SimpleDNSCrypt64.msi.minisig) can be verified with the following command:

	minisign -Vm SimpleDNSCrypt.msi -P RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV
	minisign -Vm SimpleDNSCrypt64.msi -P RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV

### Deinstallation

To uninstall Simple DNSCrypt and dnscrypt-proxy, just go to the Windows Control Panel (Programs and Features) and search for Simple DNSCrypt.

### Updates

Simple DNSCrypt will automatically search for the latest version at startup.

## Translations

Translations are created with [POEditor](https://poeditor.com).
If you can add or correct a language, feel free to do so: 

[https://poeditor.com/join/project/3frSzJtSqc](https://poeditor.com/join/project/3frSzJtSqc "poeditor.com")

## Screenshots

![maintab](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/mainmenu.png)

![resolvers](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/resolvers.png)

![advanced](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/advancedsettings.png)

![blacklist](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/blacklist.png)

![blocklog](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/blocklog.png)

![settings](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/preview/settings.png)

## Built With

* [Visual Studio 2017 (15.5.x)](https://www.visualstudio.com/downloads/)
* [.NET Framework 4.6.x](https://www.microsoft.com/net/download/windows)
* [ReSharper](https://www.jetbrains.com/resharper/) 
* [Advanced Installer](https://www.advancedinstaller.com/)

## Authors

* **Christian Hermann** - [bitbeans](https://github.com/bitbeans)

See also the list of [Contributors.md](Contributors.md) who participated in this project. 
If you are a translator, feel free to update this file.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Used Software and Libraries

- [Baseclass.Contrib.Nuget.Output](https://github.com/baseclass/Contrib.Nuget)
- [Caliburn.Micro](https://github.com/Caliburn-Micro/Caliburn.Micro)
- [ControlzEx](https://github.com/ControlzEx/ControlzEx)
- [Costura.Fody](https://github.com/Fody/Costura)
- [Fody](https://github.com/Fody/Fody)
- [helper-net](https://github.com/bitbeans/helper-net)
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [minisign-net](https://github.com/bitbeans/minisign-net)
- [Nett](https://github.com/paiden/Nett)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [NLog](https://github.com/nlog/NLog)
- [libsodium-net](https://github.com/adamcaudill/libsodium-net)
- [WPFLocalizationExtension](https://github.com/SeriousM/WPFLocalizationExtension)
- [XAMLMarkupExtensions](https://github.com/MrCircuit/XAMLMarkupExtensions)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)


## Thanks to

* Frank Denis for the development of [dnscrypt-proxy](https://github.com/jedisct1/dnscrypt-proxy)
* all users, translators and contributors
* [ReSharper](https://www.jetbrains.com/resharper/) for providing a free open source license
* [POEditor](https://poeditor.com) for providing a free open source license
