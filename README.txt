This open source (GNU GPLv3) project is aimed at simplifying the development of C# applications that communicate with USB HID (Human Interface Device) devices.

There used to be a Microchip HID PnP Demo application that demonstrated how to connect to a HID device from C#.
The demo is designed to work with their USB demo boards such as the "PIC18F46J50 FS USB Demo Board" (see http://www.microchip.com/Developmenttools/ProductDetails.aspx?PartNO=MA180024)
However, that particular application was based on VisualStudio 2010 and .NET framework 2.0 and no longer gets maintained it seems.

This VisualStudio solution was created on VisualStudio Community 2015 (which is free) and .NET framework 4.6. Please be aware that all projects must be compiled with "x86" setting even on a 64bit platform. 
There are 3 projects in this solution:

- A WindowsForm application named HidDemoWindowsForms
- A WPF application named HidDemoWpf
- A Console application named HidDemoConsole

All 3 projects offer very similar functionality. They connect to suitably programmed HID device, read an ADC value and pushbutton state and let you toggle an LED. They also list all available HID devices and notify you when a device is attached or detached.

All the heavy lifting is done in a common file named hid.cs. It can be included in any C# project, no matter if it is based on WPF, WindowsForms or just a console application. It's full of DLL imports, Marshalling and COM API calls. In other words, it's not pretty and it doesn't look much like C# even though it is. You should not need to edit this file or know anything about it. 

hid.cs offers a nice, truely C# interface that lets you communicate to HID devices. Basically all you need to do is to create an instance of HidUtility and subscribe to the events you're interested in. The 3 applications should give you a good starting point.

More information is available on https://soldernerd.com/2017/02/14/c-usb-hid-utility/

Lukas Fässler
soldernerd.com
2017-02-13





