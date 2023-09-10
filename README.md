# BambuLabsListener

This is a listener & notifier for Bambu Labs printers using MQTT to listen on the messages the printer sends and provide print notifications via Discord

_This has only been tested on a P1P as that's the only printer I have. If you have a X1C and are willing to test this out, please visit https://github.com/derekantrican/BambuLabsListener/issues/2_

## Demo

//Insert demo video

## Cross-plat compatibility

- Windows: tested & working
- OSX: tested & working
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

## Future plans

_Theoretically,_ MQTT could also be used to send (publish) messages TO the printer (eg starting a print job). I haven't tested that and don't know if it works. Regardless, I don't plan on supporting that in the future - this repository is only meant to serve as a small tool & example code for building something similar. But anyone is welcome to fork & create a PR to contribute!