# PixyUSBdotNET
Pixy USB .NET library

I put together the Pixy USB .NET library to make it easier to communicate with the PixyCam over USB. 

More on pixy cam: http://charmedlabs.com/default/pixy-cmucam5/

I included a sample application to show usage. I don't have much experience in C/C++, so I've hacked my way through it. Please excuse poor C/C++ code/style.
This library is also still in progress. I'm currently working on additional commands as well as writing code to retrieve images from the camera.

I'll take any help I can get! Thanks for looking!

A few notes:

Regarding use with PixyCam:
- Make sure you set a signature if you want the sample application to work. Just fire up PixyMon and tag a color to use as a signature.
- Make sure you are using a cable that works (most recommended is under 3 feet) or sends enough power to the servos. I deal with this a lot and it was super frustrating, until I got a cable that could transmit more power over further distances.

Regarding source code:
- To compile everything you'll need the BOOST libraries. 
- LibUSB project is included because I had to make some minor changes for it to compile run on windows, and pixyusblib because I'm making changes to the source there as well.
- Note that the projects for the libUSB and the pixyusblib have a post-build event copying the DLLs to the SampleApplication and PixyUSB.NET output folder locations. This is because the pixyusblib needs the libUSB DLLs in the same directory and thus the PixyUSB.NET needs both of them in the same directory as well. So to use in your own application you would have your .EXE and then the PixyUSB.NET, pixyusblib, and libUSB DLLs in there as well.

Finally, keep in mind the library is a work in progress and does not contain all the features yet. Enjoy! I'd love to hear what others can do with this.

Blog Post: http://pabloaizpiri.com/2015/10/25/net-pixycam-library/

Pablo D. Aizpiri
