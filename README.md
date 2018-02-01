# ![Alt text](img/icons/64x64.png "Simple DNSCrypt") Simple DNSCrypt

[![license](https://img.shields.io/github/license/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/blob/master/LICENSE.md) [![Github All Releases](https://img.shields.io/github/release/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![dnscrypt--proxy](https://img.shields.io/badge/dnscrypt--proxy-2.0.0rc-orange.svg?style=flat-square)](https://github.com/jedisct1/dnscrypt-proxy) [![Github All Releases](https://img.shields.io/github/downloads/bitbeans/SimpleDnsCrypt/total.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![donate PayPal](https://img.shields.io/badge/donate-PayPal-green.svg?style=flat-square)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=c.hermann@bitbeans.de&item_name=Donation+to+the+Simple+DNSCrypt+project) [![donate pledgie](https://img.shields.io/badge/donate-pledgie-green.svg?style=flat-square)](https://pledgie.com/campaigns/32588)

Simple DNSCrypt is a simple management tool to configure dnscrypt-proxy on windows based systems.

If you are looking for an only command line tool, you can use the [dnscrypt-proxy](https://github.com/jedisct1/dnscrypt-proxy) software. There are pre-compiled versions for any os.
The dnscrypt-proxy software is written and maintained by Frank Denis (@jedisct1).

# ![Alt text](img/icons/32x32.png "Status") Status

Preview with dnscrypt-proxy-2.0.0rc

[![Build status](https://ci.appveyor.com/api/projects/status/1st7yuscwwx2duib/branch/dnscrypt-proxy2?svg=true)](https://ci.appveyor.com/project/bitbeans/simplednscrypt/branch/dnscrypt-proxy2)

[Download x86 (preview, unsigned portable version)](https://simplednscrypt.blob.core.windows.net/deploy/SimpleDnsCrypt/bin/x86/SimpleDNSCrypt_x86.zip)

[Download x64 (preview, unsigned portable version)](https://simplednscrypt.blob.core.windows.net/deploy/SimpleDnsCrypt/bin/x64/SimpleDNSCrypt_x64.zip)

[Translations](https://poeditor.com/join/project/3frSzJtSqc) feel free to contribute :smiley:


# ![Alt text](img/icons/32x32.png "Installation") Installation

To install Simple DNSCrypt use the latest MSI packages: [x86](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.3/SimpleDNSCrypt.msi) or [x64](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.3/SimpleDNSCrypt64.msi).
You don`t need to download the dnscrypt-resolvers.csv or the dnscrypt-proxy package at all.
Both are included in the msi package, the dnscrypt-resolvers.csv can be updated from inside the software (and will be verified with minisign).

### ![Alt text](img/icons/16x16.png "File Signing") File Signing
The MSI package and the SimpleDnsCrypt.exe are signed via a *COMODO RSA Code Signing CA*. 
The files are signed under the name: *EAM Experience Area Münsingen GmbH*

You also can verify the MSI packages with [minisign](https://jedisct1.github.io/minisign/).
The minisign signatures [x86](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.3/SimpleDNSCrypt.msi.minisig) and [x64](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.3/SimpleDNSCrypt64.msi.minisig) can be verified with the following command:

	minisign -Vm SimpleDNSCrypt.msi -P RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV

### ![Alt text](img/icons/16x16.png "Uninstall") Uninstall
To uninstall Simple DNSCrypt and dnscrypt-proxy, just go to the Windows Control Panel (Programs and Features) and search for Simple DNSCrypt.

### ![Alt text](img/icons/16x16.png "Updates") Updates
Simple DNSCrypt will automatically check for new versions on startup.

# ![Alt text](img/icons/32x32.png "Translations") Translations

Start the translation for the new version: https://poeditor.com/join/project/3frSzJtSqc

Thanks to [POEditor](https://poeditor.com) for the open source license!

The **old** SimpleDNSCrypt speaks the following languages (17):

- Bulgarian (@rddim)
- Danish (@simonclausen)
- Dutch (Tim Tyteca)
- English
- French (@didihu)
- German
- Italian (@ShellAddicted)
- Indonesian (@christantoan)
- Japanese (@bungoume)
- Norwegian (@niikoo)
- Persian/Farsi (@robin98)
- Russian (Vlad)
- Simplified Chinese (@jerryhou85)
- Spanish (@bcien) (@pablomh)
- Swedish (@eson57)
- Traditional Chinese (@porsche613)
- Turkish (@emirgian)

# ![Alt text](img/icons/32x32.png "Want to say thanks?") Want to say thanks?

* Hit the :star: Star :star: button
* <a href='https://pledgie.com/campaigns/32588'><img alt='Click here to lend your support to: Simple DNSCrypt and make a donation at pledgie.com !' src='https://pledgie.com/campaigns/32588.png?skin_name=chrome' border='0' ></a>


# ![Alt text](img/icons/32x32.png "Used Software") Used Software

-TODO

# ![Alt text](img/icons/32x32.png "Special Thanks") Special Thanks

Frank Denis (@jedisct1) for developing [libsodium](https://github.com/jedisct1/libsodium) and [DNSCrypt](https://dnscrypt.org)
  
The EAM GmbH and [bytejail](https://bytejail.com) for funding this project

# ![Alt text](img/icons/32x32.png "License") License
[MIT](https://en.wikipedia.org/wiki/MIT_License)
