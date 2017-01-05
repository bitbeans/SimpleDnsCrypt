# ![Alt text](img/icons/64x64.png "Simple DNSCrypt") Simple DNSCrypt

[![license](https://img.shields.io/github/license/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/blob/master/LICENSE.md) [![Github All Releases](https://img.shields.io/github/release/bitbeans/SimpleDnsCrypt.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![Github All Releases](https://img.shields.io/github/downloads/bitbeans/SimpleDnsCrypt/total.svg?style=flat-square)](https://github.com/bitbeans/SimpleDnsCrypt/releases/latest) [![donate PayPal](https://img.shields.io/badge/donate-PayPal-green.svg?style=flat-square)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=c.hermann@bitbeans.de&item_name=Donation+to+the+Simple+DNSCrypt+project) [![donate pledgie](https://img.shields.io/badge/donate-pledgie-green.svg?style=flat-square)](https://pledgie.com/campaigns/32588)

Simple DNSCrypt is a simple management tool to configure dnscrypt-proxy on windows based systems.

If you are looking for an only command line tool, you can use the [dnscrypt-proxy](https://dnscrypt.org/) software. There are pre-compiled versions for any os.
The dnscrypt-proxy software is written and maintained by Frank Denis (@jedisct1).

# ![Alt text](img/icons/32x32.png "Status") Status

Missing features:

- IPv6 support - see [#1](https://github.com/bitbeans/SimpleDnsCrypt/issues/1)

dnscrypt-proxy version: **1.9.1**

# ![Alt text](img/icons/32x32.png "Installation") Installation

To install Simple DNSCrypt use the [latest MSI package](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.0/SimpleDNSCrypt.msi).
You don`t need to download the dnscrypt-resolvers.csv or the dnscrypt-proxy package at all.
Both are included in the msi package, the dnscrypt-resolvers.csv can be updated from inside the software (and will be verified with minisign).

### ![Alt text](img/icons/16x16.png "File Signing") File Signing
The MSI package and the SimpleDnsCrypt.exe are signed via a *COMODO RSA Code Signing CA*. 
The files are signed under the name: *EAM Experience Area Münsingen GmbH*

You also can verify the MSI package with [minisign](https://jedisct1.github.io/minisign/).
The minisign [signature](https://github.com/bitbeans/SimpleDnsCrypt/releases/download/0.4.0/SimpleDNSCrypt.msi.minisig) can be verified with the following command:

	minisign -Vm SimpleDNSCrypt.msi -P RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV

### ![Alt text](img/icons/16x16.png "Uninstall") Uninstall
To uninstall Simple DNSCrypt and dnscrypt-proxy, just go to the Windows Control Panel (Programs and Features) and search for Simple DNSCrypt.

### ![Alt text](img/icons/16x16.png "Updates") Updates
Simple DNSCrypt will automatically check for new versions on startup.

# ![Alt text](img/icons/32x32.png "Overview") Overview

#### Standard Settings
![standard view](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/standard.png)

#### Advanced Settings
![advanced view](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/advanced.png)

##### Plugins

![plugin view](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/plugins.png)

##### Block and Blacklist

![plugin view](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/blacklist.png)

##### Live Log

![plugin view](https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/img/livelog.png)

Simple DNSCrypt currently supports four plugins:

**libdcplugin_ldns_aaaa_blocking**

If your network doesn't support IPv6, chances are that your applications are still constantly trying to resolve IPv6 addresses, causing unnecessary slowdowns.

This plugin causes the proxy to reply immediately to IPv6 requests, without having to send a useless request to upstream resolvers, and having to wait for a response.

**libdcplugin_logging**

This plugin logs the DNS queries received by the proxy. The logs are stored in a local file.
You can choose the folder, where dnscrypt-proxy will store the logfile (dns.log).

**libdcplugin_cache**

This plugin implements a simple, zero-configuration DNS response cache.
The mimimum time to keep a record in cache can be specified in the cache plugin: 60 - 86400 seconds.

**libdcplugin_ldns_blocking**

This plugin returns a REFUSED response if the query name is in a list of blacklisted names, or if at least one of the returned IP addresses happens to be in a list of blacklisted IPs.

A file should list one entry per line.

IPv4 and IPv6 addresses are supported. For names, leading and trailing wildcards (*) are also supported (e.g. *xxx*, *.example.com, ads.*)

**Note:** If the file(s) is/are empty, the service may **not** start!


# ![Alt text](img/icons/32x32.png "Compatibility") Compatibility

This software was tested on:

|             | 32 bit      | 64 bit     |
| :----------- | :-----------: | :-----------: | 
| Windows 7    | tested        | tested        | 
| Windows 8.1     | tested        | tested       | 
| Windows 10     | tested        | tested        | 

Feel free to report your success or failure: [here](https://github.com/bitbeans/SimpleDnsCrypt/issues/5)

# ![Alt text](img/icons/32x32.png "Requirements") Requirements

- This software targets .NET 4.6.
- It also requires Visual C++ Redistributable for Visual Studio 2015 x86.

# ![Alt text](img/icons/32x32.png "Translations") Translations

SimpleDNSCrypt currently speaks the following languages (13):

- Danish (@simonclausen)
- Dutch (Tim Tyteca)
- English
- French (@didihu)
- German
- Italian (@ShellAddicted)
- Indonesian (@christantoan)
- Persian/Farsi (@robin98)
- Russian (Vlad)
- Simplified Chinese (@jerryhou85)
- Spanish (@bcien) (@pablomh)
- Swedish (@eson57)
- Turkish (@emirgian)

If you are able to translate the resx files into more languages, please feel free to send a pull request. 

# ![Alt text](img/icons/32x32.png "Want to say thanks?") Want to say thanks?

* Hit the :star: Star :star: button
* <a href='https://pledgie.com/campaigns/32588'><img alt='Click here to lend your support to: Simple DNSCrypt and make a donation at pledgie.com !' src='https://pledgie.com/campaigns/32588.png?skin_name=chrome' border='0' ></a>

# ![Alt text](img/icons/32x32.png "Alternative Software") Alternative Software

If you don`t like this software, there are two similar projects:

- [DNSCrypt WinClient](https://github.com/Noxwizard/dnscrypt-winclient): the original DNSCrypt user interface for Windows.
- [DNSCrypt Windows Service Manager](http://simonclausen.dk/projects/dnscrypt-winservicemgr/): a full-featured DNSCrypt user interface for Windows.

# ![Alt text](img/icons/32x32.png "Used Software") Used Software

- libsodium-net [doc](https://www.gitbook.com/book/bitbeans/libsodium-net/details) [:octocat:](https://github.com/adamcaudill/libsodium-net)
- WPF Localize Extension [doc](https://wpflocalizeextension.codeplex.com/) [:octocat:](https://github.com/SeriousM/WPFLocalizationExtension) 
- Caliburn Micro [doc](http://caliburnmicro.com/) [:octocat:](https://github.com/Caliburn-Micro/Caliburn.Micro/) 
- MahApps Metro [doc](http://mahapps.com/) [:octocat:](https://github.com/MahApps/MahApps.Metro) 
- minisign-net [:octocat:](https://github.com/bitbeans/minisign-net) 
- YamlDotNet [:octocat:](https://github.com/aaubry/YamlDotNet) 
- helper-net [:octocat:](https://github.com/bitbeans/helper-net) 
- DNS [:octocat:](https://github.com/kapetan/dns) 

# ![Alt text](img/icons/32x32.png "Special Thanks") Special Thanks

Frank Denis (@jedisct1) for developing [libsodium](https://github.com/jedisct1/libsodium) and [DNSCrypt](https://dnscrypt.org)
  
The EAM GmbH and [bytejail](https://bytejail.com) for funding this project

# ![Alt text](img/icons/32x32.png "License") License
[MIT](https://en.wikipedia.org/wiki/MIT_License)
