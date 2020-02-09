Larkspur Ember Web Provider
=========================================================

* Web Site: https://www.irisbroadcast.org
* Github: https://github.com/IrisBroadcast/LarkspurEmberWebProvider

Creates a few EmBER+ nodes for using with controllers that know
the EmBER+ protocol. The information/nodes is also available to
access with SignalR and a REST application interface (API).

It's a standalone .NET Core Worker Service.

Could be used to bridge EmBER+ information and control to and from
the web.


License
=======
Larkspur Ember Web Provider is using a library 'EmberLib.net' from Lawo GmbH
```
EmberLib.net -- .NET implementation of the Ember+ Protocol
Copyright (c) 2012-2019 Lawo GmbH (http://www.lawo.com).
Distributed under the Boost Software License, Version 1.0.
```

Larkspur Ember Web Provider is Copyright (c) 2019 Roger Sandholm & Fredrik Bergholtz, Stockholm, Sweden
The code is licensed under the BSD 3-clause license.

The license for Larkspur Ember Web Provider is in the LICENSE.txt file

## How to get started

### Run and develop as a Microsoft Service
The application contains a package:
> Microsoft.Extensions.Hosting.WindowsServices
that enables the application to be runned as a windows service.

First build the application:
> dotnet publish -r win-x64 -c Release

Then to get the service up and running use the windows service 'sc' commands.
Open a terminal / powershell:
> sc create LarkspurService BinPath=C:\code\bin\LarkspurEmberWebProvider.exe

Then you should start the service:
> sc start LarkspurService

## Responsible Maintainers
- [Fredrik Bergholtz](https://github.com/freber???)
- [Roger Sandholm](https://github.com/Roog)