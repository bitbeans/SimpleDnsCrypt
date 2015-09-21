# Simple DNSCrypt

Simple DNSCrypt is a simple management tool to configure dnscrypt-proxy on windows based systems.

If you are looking for an only command line tool, you can use the [dnscrypt-proxy](https://dnscrypt.org/) software. There are pre-compiled versions for any os.
The dnscrypt-proxy software is written and maintained by Frank Denis (@jedisct1).

### Status

**There will be bugs. May change. May break.**

Missing features:

- IPv6 support
- Secondary resolver support

### Installation

To install Simple DNSCrypt use the [latest MSI package](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.2.3/SimpleDNSCrypt.msi).
You don`t need to download the dnscrypt-resolvers.csv or the dnscrypt-proxy package at all.
Both are included in the msi package, the dnscrypt-resolvers.csv can be updated from inside the software (and will be verified with minisign).

#### File Signing
The MSI package and the SimpleDnsCrypt.exe are signed via a *COMODO RSA Code Signing CA*. 
The files are signed under the name: *EAM Experience Area Münsingen GmbH*

You also can verify the MSI package with [minisign](https://jedisct1.github.io/minisign/).
The minisign [signature](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.2.3/SimpleDNSCrypt.msi.minisig) can be verified with the following command:

	minisign -Vm SimpleDNSCrypt.msi -P RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV

#### Uninstall
To uninstall Simple DNSCrypt and dnscrypt-proxy, just go to the windows Control Panel (Programs and Features) and search for Simple DNSCrypt.


### Overview

#### Standard Settings
![beta 0.2.1](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/standard.png)

#### Advanced Settings
![beta 0.2.1](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/advanced.png)

### Compatibility

This software was tested on:

|             | 32 bit      | 64 bit     |
| :----------- | :-----------: | :-----------: | 
| Windows 7    | untested        | untested        | 
| Windows 8.1     | untested        | tested       | 
| Windows 10     | untested        | tested        | 

Feel free to report your success or failure: [here](https://github.com/bitbeans/SimpleDnsCrypt/issues/5)

### Requirements

This software targets .NET 4.5.

### Translations

SimpleDNSCrypt currently speaks the following languages:

- english
- german

If you are able to translate the the resx files into more languages, please feel free to send a pull request. 


### Alternative Software

If you don`t like this software, there are two similar projects:

- [DNSCrypt WinClient](https://github.com/Noxwizard/dnscrypt-winclient): the original DNSCrypt user interface for Windows.
- [DNSCrypt Windows Service Manager](http://simonclausen.dk/projects/dnscrypt-winservicemgr/): a full-featured DNSCrypt user interface for Windows.


### Used Software

- libsodium-net [doc](https://www.gitbook.com/book/bitbeans/libsodium-net/details) [:octocat:](https://github.com/adamcaudill/libsodium-net)
- WPF Localize Extension [doc](https://wpflocalizeextension.codeplex.com/) [:octocat:](https://github.com/SeriousM/WPFLocalizationExtension) 
- Caliburn Micro [doc](http://caliburnmicro.com/) [:octocat:](https://github.com/Caliburn-Micro/Caliburn.Micro/) 
- MahApps Metro [doc](http://mahapps.com/) [:octocat:](https://github.com/MahApps/MahApps.Metro) 
- minisign-net [:octocat:](https://github.com/bitbeans/minisign-net) 

### Special Thanks

Frank Denis (@jedisct1) for developing [libsodium](https://github.com/jedisct1/libsodium) and [DNSCrypt](https://dnscrypt.org)
  
The EAM GmbH and [bytejail](https://bytejail.com) for funding this project

### License
[MIT](https://en.wikipedia.org/wiki/MIT_License)