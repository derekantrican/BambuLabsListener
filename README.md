# BambuLabsListener

This is a listener & notifier for Bambu Labs printers using MQTT to listen on the messages the printer sends and provide print notifications via Discord

_This has only been tested on a P1P as that's the only printer I have. If you have a X1C and are willing to test this out, please visit https://github.com/derekantrican/BambuLabsListener/issues/2_

## Demo

https://github.com/derekantrican/BambuLabsListener/assets/1558019/c08882ff-e89c-4caa-8848-8a92f62c486e

## Cross-plat compatibility

- Windows: tested & working
- OSX: tested on an older Macbook - same issues as Linux https://github.com/derekantrican/BambuLabsListener/issues/1. Let me know if you test this
- Linux: I tried testing on a raspberry pi & had some issues (https://github.com/derekantrican/BambuLabsListener/issues/1). If you try this on Linux, please let me know your findings

## Getting started

_For this to work, you need to be on the same network as your printer_

0. *Install .NET 6 if you do not have it installed already (https://dotnet.microsoft.com/en-us/download/dotnet/6.0 - ".NET Runtime" for your platform)*
1. Clone the repo
2. Open your terminal & `cd` into the repository folder
3. Run `dotnet run`
4. A settings.json file will be created for you. Open it up and populate the values listed
5. Run `dotnet run` again and the application will start listening to messages from your printer
6. Start a print! You should be able to watch the console for message updates such as when a print starts, layer progress, and print finishing. Discord notifications will be sent to the webhook specified when a print starts & finishes

## Troubleshooting

As mentioned in https://github.com/derekantrican/BambuLabsListener/issues/3#issuecomment-1714352307 I had some issues getting this to work unless I installed [MQTT Explorer](https://mqtt-explorer.com/) first & got it working listening to my printer's message stream. After that, this tool seemed to work (no idea why that might be).

## Questions/Issues

- For **Questions** (such as "how do I..."), please [open a discussion](https://github.com/derekantrican/BambuLabsListener/discussions)
- For **Issues** (such as a true bug or feature request), please [open an issue](https://github.com/derekantrican/BambuLabsListener/issues)

## Support/Donate to this project

Thank you so much for considering donating to this project! The more donations I get, the more I can justify putting work into this project.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5RZJY)

## Future plans

Todos:

- Get this working "out of the box" on any platform (https://github.com/derekantrican/BambuLabsListener/issues/1, https://github.com/derekantrican/BambuLabsListener/issues/3)
- Confirm this works with the X1C (https://github.com/derekantrican/BambuLabsListener/issues/2)

#### UI
_It has been suggested that I turn this into an OctoPrint-like interface. That would be a lot of work (and a lot of learning new things for me), but I can see the value in it. If this gets enough demand and there's enough sponsorship of this project, I'll be able to justify time towards this feature._

#### _Sending_ messages
_Theoretically,_ MQTT could also be used to send (publish) messages TO the printer (eg starting a print job). I haven't tested that and don't know if it works. I have no plans to do this currently - this repository is only meant to serve as a small tool & example code for building something similar. But anyone is welcome to fork & create a PR to contribute!
