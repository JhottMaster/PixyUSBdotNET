# PixyUSBdotNET
## Pixy USB .NET library

I put together the Pixy USB .NET library to make it easier to communicate with the PixyCam over USB. 

More on pixy cam: http://charmedlabs.com/default/pixy-cmucam5/

I included a sample application to show usage. I don't have much experience in C/C++, so I've hacked my way through it. Please excuse poor C/C++ code/style.
This library is also still in progress. I'm currently working on additional commands as well as writing code to retrieve images from the camera.

I'll take any help I can get! I'd love to hear what others can do with this. Enjoy

Pablo D. Aizpiri
Reference blog post: http://pabloaizpiri.com/2015/10/25/net-pixycam-library/

## A few notes:

### Regarding use with PixyCam:
- Make sure you set a signature if you want the sample application to work. Just fire up PixyMon and tag a color to use as a signature.
- Make sure you are using a cable that works (most recommended is under 3 feet) or sends enough power to the servos. I deal with this a lot and it was super frustrating, until I got a cable that could transmit more power over further distances.

### Getting this project to run:
I've gotten multiple questions from people with a .NET background but not much C/C++ experience having difficulty getting this to run. I feel your pain. So I've put together a simple set-by-step guide here. To compile everything you'll need the BOOST libraries (http://www.boost.org). Note that the `pixyusblib` project in the solution uses the chrono library in BOOST which must be built separately to your target platform & toolset (http://www.boost.org/doc/libs/1_66_0/more/getting_started/windows.html#header-only-libraries). You'll need to set the configuration of the project to point to the correct boost install locations as well. *So, if you are on Windows and unfamiliar with boost/C/C++*, I recommend you do the following to keep it simple:
- Download boost *installer* _with_ the target platform libraries/binaries you need. You can do this from the 3rd party website boost links on their download page: https://dl.bintray.com/boostorg/release/1.66.0/binaries/ . 
- Choose the correct target platform and architecture. For the code in this repository I am targeting v141/32-bit so download "boost_1_66_0-msvc-14.1-32.exe" to match project.
- Update your `pixyusblib` project configuration to reflect the changes. For this project, I put mine in `C:\Program Files\boost\boost_1_66_0`:
  - Right-click on `pixyusblib` project in the solution and choose "Properties"
    - Under `Configuration Properties/General` check that the platform toolset you are targetting matches the boost binaries. This should work out of the box if you followed instructions above for downloading BOOST binaries and stuck with the target platform/architecture I used.
    - Under `Configuration Properties/VC++ Directories`, make sure your `Include Directories` includes the path to your boost install. For example, `$(VC_IncludePath);$(WindowsSDK_IncludePath);C:\Program Files\boost\boost_1_66_0`
    - Under `Configuration Properties/Libraries`, make sure your `Library Directories` includes the path to your boost binaries. For example, `$(VC_LibraryPath_x86);$(WindowsSDK_LibraryPath_x86);$(SolutionDir)libusb-1.0\$(Configuration);C:\Program Files\boost\boost_1_66_0\lib32-msvc-14.1`
    - Under `Configuration Properties/C/C++/General`, your "Additional Include Directories" should be pointing to your root boost install dir (e.g. `C:\Program Files\boost\boost_1_66_0`) 

Make sure `pixyusblib` builds. Same with `libusb-1.0` the `PixyUSB.NET`. If those are good, SampleApplication will run for you. Make sure it's set as the start-up project (right click on it to do this), hit debug, and if you have your pixy cam connected it should all run and show you a cool little demo! Obviously, I expect you already have PixyCam connected to your computer and using the PixyCam software can interact with it with not problems- if you can't do that reference PixyCam's support and make sure that is working first since my library is really just an extension of theirs.

#### Additional notes on project:
- Note that the projects for `libusb-1.0` and `pixyusblib` have a post-build event copying the DLLs to the `SampleApplication` and `PixyUSB.NET` output folder locations. This is because the `pixyusblib` needs the `libusb-1.0` DLLs in the same directory and thus the `PixyUSB.NET` project needs both of them in the same directory as well. So to use in your own application you would have your .EXE and then the PixyUSB.NET, pixyusblib, and libUSB DLLs in there as well.
- `libusb-1.0` project is included because I had to make some minor changes to LibUSB for it to compile run on Windows, and `pixyusblib` because I'm making changes to the source there as well.
Finally, keep in mind the library is a work in progress and does not contain all the features yet. 


