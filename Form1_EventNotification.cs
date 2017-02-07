
using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;



namespace HID_PnP_Demo
{
    /*
    //Delegate declarations. These are used as callback functions to notify the application of USB events
    public delegate void deviceChange_delegate();

    partial class Form1
    {
        //Delegate used as callback functions to notify the application of USB events
        deviceChange_delegate deviceChange_handler;

        //Constant definitions from setupapi.h, which we aren't allowed to include directly since this is C#
        //internal const uint DIGCF_PRESENT = 0x02;
        //internal const uint DIGCF_DEVICEINTERFACE = 0x10;
        //Constant definitions for certain WM_DEVICECHANGE messages
        internal const uint WM_DEVICECHANGE = 0x0219;
        internal const uint DBT_DEVICEARRIVAL = 0x8000;
        internal const uint DBT_DEVICEREMOVEPENDING = 0x8003;
        internal const uint DBT_DEVICEREMOVECOMPLETE = 0x8004;
        internal const uint DBT_CONFIGCHANGED = 0x0018;
        //Other constant definitions
        internal const uint DBT_DEVTYP_DEVICEINTERFACE = 0x05;
        internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00;
        internal const uint ERROR_SUCCESS = 0x00;
        internal const uint ERROR_NO_MORE_ITEMS = 0x00000103;
        internal const uint SPDRP_HARDWAREID = 0x00000001;

        internal struct DEV_BROADCAST_DEVICEINTERFACE
        {
            internal uint dbcc_size;            //DWORD
            internal uint dbcc_devicetype;      //DWORD
            internal uint dbcc_reserved;        //DWORD
            internal Guid dbcc_classguid;       //GUID
            internal char[] dbcc_name;          //TCHAR array
        }

        //Need this function for receiving all of the WM_DEVICECHANGE messages.  See MSDN documentation for
        //description of what this function does/how to use it. Note: name is remapped "RegisterDeviceNotificationUM" to
        //avoid possible build error conflicts.
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            uint Flags);

        //Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
        Guid InterfaceClassGuid = new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30);


        private void InitializeDeviceChangeNotifications(deviceChange_delegate deviceChange_h)
        {
            deviceChange_handler = deviceChange_h;

            //Register for WM_DEVICECHANGE notifications.  This code uses these messages to detect plug and play connection/disconnection events for USB devices
            DEV_BROADCAST_DEVICEINTERFACE DeviceBroadcastHeader = new DEV_BROADCAST_DEVICEINTERFACE();
            DeviceBroadcastHeader.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            DeviceBroadcastHeader.dbcc_size = (uint)Marshal.SizeOf(DeviceBroadcastHeader);
            DeviceBroadcastHeader.dbcc_reserved = 0;    //Reserved says not to use...
            DeviceBroadcastHeader.dbcc_classguid = InterfaceClassGuid;

            //Need to get the address of the DeviceBroadcastHeader to call RegisterDeviceNotification(), but
            //can't use "&DeviceBroadcastHeader".  Instead, using a roundabout means to get the address by 
            //making a duplicate copy using Marshal.StructureToPtr().
            IntPtr pDeviceBroadcastHeader = IntPtr.Zero;  //Make a pointer.
            pDeviceBroadcastHeader = Marshal.AllocHGlobal(Marshal.SizeOf(DeviceBroadcastHeader)); //allocate memory for a new DEV_BROADCAST_DEVICEINTERFACE structure, and return the address 
            Marshal.StructureToPtr(DeviceBroadcastHeader, pDeviceBroadcastHeader, false);  //Copies the DeviceBroadcastHeader structure into the memory already allocated at DeviceBroadcastHeaderWithPointer
            RegisterDeviceNotification(this.Handle, pDeviceBroadcastHeader, DEVICE_NOTIFY_WINDOW_HANDLE);
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                if (((int)m.WParam == DBT_DEVICEARRIVAL) || ((int)m.WParam == DBT_DEVICEREMOVEPENDING) || ((int)m.WParam == DBT_DEVICEREMOVECOMPLETE) || ((int)m.WParam == DBT_CONFIGCHANGED))
                {
                    //WM_DEVICECHANGE messages by themselves are quite generic, and can be caused by a number of different
                    //sources, not just your USB hardware device.  Therefore, must check to find out if any changes relavant
                    //to your device (with known VID/PID) took place before doing any kind of opening or closing of handles/endpoints.
                    //(the message could have been totally unrelated to your application/USB device)

                    deviceChange_handler();

                }
            }
            //Call original handler function
            base.WndProc(ref m);
        }

    } // partial class Form1
    */

} // namespace HID_PnP_Demo