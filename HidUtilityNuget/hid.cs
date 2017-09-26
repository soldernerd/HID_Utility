
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HidUtilityNuget
{
    internal static class UsbNotification
    {
        public const int DbtDevicearrival = 0x8000; // system detected a new device        
        public const int DbtDeviceremovecomplete = 0x8004; // device is gone      
        public const int WmDevicechange = 0x0219; // device change event      
        private const int DbtDevtypDeviceinterface = 5;
        private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
        private static IntPtr notificationHandle;

        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
            {
                DeviceType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidDevinterfaceUSBDevice,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceinterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }
    }

    //A class representing a USB device
    public class Device : EventArgs
    {
        public ushort Vid { get; private set; }
        public ushort Pid { get; private set; }
        public string DeviceID { get; private set; }
        public string ClassGuid { get; private set; }
        public string Caption { get; private set; }
        public string Manufacturer { get; private set; }

        public Device()
        {
            this.Vid = 0x0000;
            this.Pid = 0x0000;
            this.DeviceID = "";
            this.ClassGuid = "";
            this.Caption = "";
            this.Manufacturer = "";
        }

        public Device(ushort Vid, ushort Pid)
        {
            this.Vid = Vid;
            this.Pid = Pid;
            this.DeviceID = "";
            this.ClassGuid = "";
            this.Caption = "";
            this.Manufacturer = "";
        }

        public Device(ManagementObject wmi_obj)
        {
            this.DeviceID = wmi_obj["DeviceID"].ToString().ToUpper();
            this.ClassGuid = wmi_obj["ClassGuid"].ToString();
            this.Caption = wmi_obj["Caption"].ToString();
            this.Manufacturer = wmi_obj["Manufacturer"].ToString();
            Match match = Regex.Match(this.DeviceID, "PID_(.{4})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string pid_string = match.Groups[1].Value;
                Pid = ushort.Parse(pid_string, System.Globalization.NumberStyles.HexNumber);
            }
            match = Regex.Match(this.DeviceID, "VID_(.{4})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string vid_string = match.Groups[1].Value;
                Vid = ushort.Parse(vid_string, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} (VID=0x{1:X4} PID=0x{2:X4})", Caption, Vid, Pid);
        }
    }


    //A class representing a USB buffer
    public class UsbBuffer : EventArgs
    {
        public byte[] buffer
        {
            get;
            set;
        }
        public bool RequestTransfer
        {
            get;
            set;
        }
        public bool TransferSuccessful
        {
            get;
            set;
        }

        public UsbBuffer(byte[] buffer)
        {
            //buffer = new byte[65];
            this.buffer = buffer;
            RequestTransfer = false;
            TransferSuccessful = false;
            clear();
        }

        public void clear()
        {
            for (int i = 0; i < 65; ++i)
            {
                buffer[i] = 0xFF;
            }
        }
    }


    public class HidUtility
    {
        public delegate void DeviceAddedEventHandler(object sender, Device dev);
        public delegate void DeviceRemovedEventHandler(object sender, Device dev);
        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusEventArgs e);
        public delegate void SendPacketEventHandler(object sender, UsbBuffer OutBuffer);
        public delegate void PacketSentEventHandler(object sender, UsbBuffer OutBuffer);
        public delegate void ReceivePacketEventHandler(object sender, UsbBuffer InBuffer);
        public delegate void PacketReceivedEventHandler(object sender, UsbBuffer InBuffer);

        public event DeviceAddedEventHandler RaiseDeviceAddedEvent;
        public event DeviceRemovedEventHandler RaiseDeviceRemovedEvent;
        public event ConnectionStatusChangedEventHandler RaiseConnectionStatusChangedEvent;
        public event SendPacketEventHandler RaiseSendPacketEvent;
        public event PacketSentEventHandler RaisePacketSentEvent;
        public event ReceivePacketEventHandler RaiseReceivePacketEvent;
        public event PacketReceivedEventHandler RaisePacketReceivedEvent;

        private List<string> DeviceIdList;
        public List<Device> DeviceList { get; private set; }
        private Device DeviceToConnectTo;
        SafeFileHandle WriteHandleToUSBDevice = null;
        SafeFileHandle ReadHandleToUSBDevice = null;

        private System.ComponentModel.BackgroundWorker UsbThread;

        public enum UsbConnectionStatus
        {
            Disconnected,
            Connected,
            NotWorking
        }

        public UsbConnectionStatus ConnectionStatus { get; private set; }

        //A class representing a USB device
        public class ConnectionStatusEventArgs : EventArgs
        {
            public UsbConnectionStatus ConnectionStatus { get; private set; }

            public ConnectionStatusEventArgs(UsbConnectionStatus status)
            {
                this.ConnectionStatus = status;
            }

            public override string ToString()
            {
                return ConnectionStatus.ToString();
            }
        }


        //Constant definitions from setupapi.h, which we aren't allowed to include directly since this is C#
        internal const uint DIGCF_PRESENT = 0x02;
        internal const uint DIGCF_DEVICEINTERFACE = 0x10;
        //Constants for CreateFile() and other file I/O functions
        internal const short FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const short INVALID_HANDLE_VALUE = -1;
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint CREATE_NEW = 1;
        internal const uint CREATE_ALWAYS = 2;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;
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

        //Various structure definitions for structures that this code will be using
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal uint cbSize;               //DWORD
            internal Guid InterfaceClassGuid;   //GUID
            internal uint Flags;                //DWORD
            internal uint Reserved;             //ULONG_PTR MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
        }

        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal uint cbSize;               //DWORD
            internal char[] DevicePath;         //TCHAR array of any size
        }

        internal struct SP_DEVINFO_DATA
        {
            internal uint cbSize;       //DWORD
            internal Guid ClassGuid;    //GUID
            internal uint DevInst;      //DWORD
            internal uint Reserved;     //ULONG_PTR  MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
        }

        internal struct DEV_BROADCAST_DEVICEINTERFACE
        {
            internal uint dbcc_size;            //DWORD
            internal uint dbcc_devicetype;      //DWORD
            internal uint dbcc_reserved;        //DWORD
            internal Guid dbcc_classguid;       //GUID
            internal char[] dbcc_name;          //TCHAR array
        }

        //DLL Imports.  Need these to access various C style unmanaged functions contained in their respective DLL files.
        //--------------------------------------------------------------------------------------------------------------
        //Returns a HDEVINFO type for a device information set.  We will need the 
        //HDEVINFO as in input parameter for calling many of the other SetupDixxx() functions.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,     //LPGUID    Input: Need to supply the class GUID. 
            IntPtr Enumerator,      //PCTSTR    Input: Use NULL here, not important for our purposes
            IntPtr hwndParent,      //HWND      Input: Use NULL here, not important for our purposes
            uint Flags);            //DWORD     Input: Flags describing what kind of filtering to use.

        //Gives us "PSP_DEVICE_INTERFACE_DATA" which contains the Interface specific GUID (different
        //from class GUID).  We need the interface GUID to get the device path.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,           //Input: Give it the HDEVINFO we got from SetupDiGetClassDevs()
            IntPtr DeviceInfoData,          //Input (optional)
            ref Guid InterfaceClassGuid,    //Input 
            uint MemberIndex,               //Input: "Index" of the device you are interested in getting the path for.
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);    //Output: This function fills in an "SP_DEVICE_INTERFACE_DATA" structure.

        //SetupDiDestroyDeviceInfoList() frees up memory by destroying a DeviceInfoList
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);          //Input: Give it a handle to a device info list to deallocate from RAM.

        //SetupDiEnumDeviceInfo() fills in an "SP_DEVINFO_DATA" structure, which we need for SetupDiGetDeviceRegistryProperty()
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInterfaceData);

        //SetupDiGetDeviceRegistryProperty() gives us the hardware ID, which we use to check to see if it has matching VID/PID
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            ref uint PropertyRegDataType,
            IntPtr PropertyBuffer,
            uint PropertyBufferSize,
            ref uint RequiredSize);

        //SetupDiGetDeviceInterfaceDetail() gives us a device path, which is needed before CreateFile() can be used.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,                    //Input: Pointer to an structure which defines the device interface.  
            IntPtr DeviceInterfaceDetailData,      //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will receive the device path.
            uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
            ref uint RequiredSize,                  //Output (optional): The number of bytes needed to hold the entire struct 
            IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

        //Overload for SetupDiGetDeviceInterfaceDetail().  Need this one since we can't pass NULL pointers directly in C#.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,               //Input: Pointer to an structure which defines the device interface.  
            IntPtr DeviceInterfaceDetailData,       //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will contain the device path.
            uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
            IntPtr RequiredSize,                    //Output (optional): Pointer to a DWORD to tell you the number of bytes needed to hold the entire struct 
            IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

        //Need this function for receiving all of the WM_DEVICECHANGE messages.  See MSDN documentation for
        //description of what this function does/how to use it. Note: name is remapped "RegisterDeviceNotificationUM" to
        //avoid possible build error conflicts.
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            uint Flags);

        //Takes in a device path and opens a handle to the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        //Uses a handle (created with CreateFile()), and lets us write USB data to the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool WriteFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            ref uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        //Uses a handle (created with CreateFile()), and lets us read USB data from the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            ref uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        //--------------- Global Varibles Section ------------------
        //USB related variables that need to have wide scope.
        //bool AttachedState = false;                     //Need to keep track of the USB device attachment status for proper plug and play operation.
        //bool AttachedButBroken = false;
        
        //String DevicePath = null;   //Need the find the proper device path before you can open file handles.

        //Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
        Guid InterfaceClassGuid = new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30);

        private readonly IntPtr sourceHandle;
        private const int WM_COPYDATA = 0x004A;

        async void OnDeviceRemoved()
        {
            //Return immediately and do all the work asynchronously
            await Task.Yield();
            //Get a list with the device IDs of all removed devices
            List<string> NewDeviceIdList = getDeviceIdList();
            List<string> RemovedDeviceIdList = new List<string>();
            foreach(string devId in DeviceIdList)
            {
                if (!NewDeviceIdList.Contains(devId))
                {
                    RemovedDeviceIdList.Add(devId);
                }
            }
            //Get removed devices
            List<Device> RemovedDeviceList = new List<Device>();
            foreach(Device dev in DeviceList)
            {
                if(RemovedDeviceIdList.Contains(dev.DeviceID))
                {
                    RemovedDeviceList.Add(dev);
                }
            }
            //Loop through removed devices
            foreach(Device removedDevice in RemovedDeviceList)
            {
                //Remove removedDevice from DeviceList
                DeviceList.Remove(removedDevice);
                //Remove removedDevice's device ID from DeviceIdList
                DeviceIdList.Remove(removedDevice.DeviceID);
                //Raise event if there are any subscribers
                if (RaiseDeviceRemovedEvent != null)
                {
                    RaiseDeviceRemovedEvent(this, removedDevice);
                }
            }
            // Check if our device has been disconnected
            if (ConnectionStatus != UsbConnectionStatus.Disconnected)
            {
                String DevicePath = GetDevicePath(this.DeviceToConnectTo);
                // Try to connect if a device path has been obtained
                if (DevicePath == null)
                {
                    CloseDevice();
                }
            }
        }

        async void OnDeviceAdded()
        {
            // Return immediately and do all the work asynchronously
            await Task.Yield();
            // Loop through devices
            List <Device> NewDeviceList = getDeviceList();
            foreach(Device dev in NewDeviceList)
            {
                if(!(DeviceIdList.Contains(dev.DeviceID)))
                {
                    DeviceIdList.Add(dev.DeviceID);
                    DeviceList.Add(dev);
                    // Raise event if there are any subscribers
                    if (RaiseDeviceAddedEvent!=null)
                    {
                        RaiseDeviceAddedEvent(this, dev);
                    }
                }
            }
            // Try to connect to the device if we are not already connected
            if(ConnectionStatus!=UsbConnectionStatus.Connected)
            {
                SelectDevice(DeviceToConnectTo);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == UsbNotification.WmDevicechange)
            {
                switch ((int)wparam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                        OnDeviceRemoved();
                        break;
                    case UsbNotification.DbtDevicearrival:
                        OnDeviceAdded();
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private IntPtr CreateMessageOnlyWindow()
        {
            IntPtr HWND_MESSAGE = new IntPtr(-3);
            HwndSourceParameters sourceParam = new HwndSourceParameters() { ParentWindow = HWND_MESSAGE };
            HwndSource source = new HwndSource(sourceParam);
            source.AddHook(WndProc);
            return source.Handle;
        }

        //--------------- End of Global Varibles ------------------

        //public HidUtility(sendPacket_delegate sendPacket_h, packetSent_delegate packetSent_h, receivePacket_delegate receivePacket_h, packetReceived_delegate packetReceived_h)
        public HidUtility()
        {
            DeviceToConnectTo = new Device();
            DeviceIdList = getDeviceIdList();
            DeviceList = getDeviceList();

            sourceHandle = this.CreateMessageOnlyWindow();
            UsbNotification.RegisterUsbDeviceNotification(sourceHandle);

            UsbThread = new System.ComponentModel.BackgroundWorker();
            UsbThread.DoWork += new System.ComponentModel.DoWorkEventHandler(UsbThread_DoWork);
            UsbThread.RunWorkerAsync();
        }

        public void SelectDevice(Device dev)
        {
            // Save the device for future use
            this.DeviceToConnectTo = dev;
            // Close any device already connected
            CloseDevice();
            // Try to obtain a device path
            String DevicePath = GetDevicePath(this.DeviceToConnectTo);
            // Try to connect if a device path has been obtained
            if(DevicePath!=null)
            {
                OpenDevice(DevicePath);
            }
        }

        // Returns a list with the device IDs of all HID devices
        // Filters may be removed if a complete list of USB devices is desired
        private List<string> getDeviceIdList()
        {
            List<string> deviceIDs = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice");
            ManagementObjectCollection objs = searcher.Get();
            foreach (ManagementObject wmi_HD in objs)
            {
                string dep = wmi_HD["Dependent"].ToString();
                Match match = Regex.Match(dep, "\"(.+VID.+PID.+)\"$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string devId = match.Groups[1].Value;
                    devId = devId.Replace(@"\\", @"\");
                    devId = devId.ToUpper();
                    if(devId.Substring(0,3)=="HID")
                    {
                        deviceIDs.Add(devId);
                    }
                }
            }
            return deviceIDs;
        }

        // Returns a list of Device object representing all devices returned by getDeviceIdList()
        private List<Device> getDeviceList()
        {
            List<Device> devices = new List<Device>();
            List<string> deviceIDs = getDeviceIdList();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
            ManagementObjectCollection objs = searcher.Get();

            foreach (ManagementObject wmi_HD in objs)
            {
                string deviceId = wmi_HD["DeviceID"].ToString();
                if (deviceIDs.Contains(deviceId))
                {
                    string caption = wmi_HD["Caption"].ToString();
                    Device dev = new Device(wmi_HD);
                    devices.Add(dev);
                }
            }
            return devices;
        }

        private String GetDevicePath(Device dev)
        {
            /*
           Before we can "connect" our application to our USB embedded device, we must first find the device.
           A USB bus can have many devices simultaneously connected, so somehow we have to find our device only.
           This is done with the Vendor ID (VID) and Product ID (PID).  Each USB product line should have
           a unique combination of VID and PID.  

           Microsoft has created a number of functions which are useful for finding plug and play devices.  Documentation
           for each function used can be found in the MSDN library.  We will be using the following functions (unmanaged C functions):

           SetupDiGetClassDevs()					//provided by setupapi.dll, which comes with Windows
           SetupDiEnumDeviceInterfaces()			//provided by setupapi.dll, which comes with Windows
           GetLastError()							//provided by kernel32.dll, which comes with Windows
           SetupDiDestroyDeviceInfoList()			//provided by setupapi.dll, which comes with Windows
           SetupDiGetDeviceInterfaceDetail()		//provided by setupapi.dll, which comes with Windows
           SetupDiGetDeviceRegistryProperty()		//provided by setupapi.dll, which comes with Windows
           CreateFile()							//provided by kernel32.dll, which comes with Windows

           In order to call these unmanaged functions, the Marshal class is very useful.

           We will also be using the following unusual data types and structures.  Documentation can also be found in
           the MSDN library:

           PSP_DEVICE_INTERFACE_DATA
           PSP_DEVICE_INTERFACE_DETAIL_DATA
           SP_DEVINFO_DATA
           HDEVINFO
           HANDLE
           GUID

           The ultimate objective of the following code is to get the device path, which will be used elsewhere for getting
           read and write handles to the USB device.  Once the read/write handles are opened, only then can this
           PC application begin reading/writing to the USB device using the WriteFile() and ReadFile() functions.

           Getting the device path is a multi-step round about process, which requires calling several of the
           SetupDixxx() functions provided by setupapi.dll.
           */

            //Device path we are trying to get
            String devicePath;
            // The device ID from the registry should contain this string (when ignoring upper/lower case)
            string DeviceIdSubstring = string.Format("Vid_{0:X4}&Pid_{1:X4}", dev.Vid, dev.Pid);
            DeviceIdSubstring = DeviceIdSubstring.ToLowerInvariant();
            // The device path should contain this string (when ignoring upper/lower case)
            string DevicePathSubstring = dev.DeviceID.Replace(@"\", @"#");
            DevicePathSubstring = DevicePathSubstring.ToLowerInvariant();

            try
            {
                IntPtr DeviceInfoTable = IntPtr.Zero;
                SP_DEVICE_INTERFACE_DATA InterfaceDataStructure = new SP_DEVICE_INTERFACE_DATA();
                SP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                SP_DEVINFO_DATA DevInfoData = new SP_DEVINFO_DATA();

                uint InterfaceIndex = 0;
                uint dwRegType = 0;
                uint dwRegSize = 0;
                uint dwRegSize2 = 0;
                uint StructureSize = 0;
                IntPtr PropertyValueBuffer = IntPtr.Zero;
                uint ErrorStatus;
                uint LoopCounter = 0;

                //First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
                DeviceInfoTable = SetupDiGetClassDevs(ref InterfaceClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (DeviceInfoTable != IntPtr.Zero)
                {
                    //Now look through the list we just populated.  We are trying to see if any of them match our device. 
                    while (true)
                    {
                        InterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(InterfaceDataStructure);
                        if (SetupDiEnumDeviceInterfaces(DeviceInfoTable, IntPtr.Zero, ref InterfaceClassGuid, InterfaceIndex, ref InterfaceDataStructure))
                        {
                            ErrorStatus = (uint)Marshal.GetLastWin32Error();
                            if (ErrorStatus == ERROR_NO_MORE_ITEMS) //Did we reach the end of the list of matching devices in the DeviceInfoTable?
                            {   //Cound not find the device.  Must not have been attached.
                                SetupDiDestroyDeviceInfoList(DeviceInfoTable);  //Clean up the old structure we no longer need.
                                return null;
                            }
                        }
                        else    //Else some other kind of unknown error ocurred...
                        {
                            ErrorStatus = (uint)Marshal.GetLastWin32Error();
                            SetupDiDestroyDeviceInfoList(DeviceInfoTable);  //Clean up the old structure we no longer need.
                            return null;
                        }

                        //Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
                        //check to see if it is the correct device or not.

                        //Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
                        DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
                        SetupDiEnumDeviceInfo(DeviceInfoTable, InterfaceIndex, ref DevInfoData);

                        //First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
                        SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, IntPtr.Zero, 0, ref dwRegSize);

                        //Allocate a buffer for the hardware ID.
                        //Should normally work, but could throw exception "OutOfMemoryException" if not enough resources available.
                        PropertyValueBuffer = Marshal.AllocHGlobal((int)dwRegSize);

                        //Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
                        //REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
                        //buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
                        //format "Vid_04d8&Pid_003f".
                        SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, PropertyValueBuffer, dwRegSize, ref dwRegSize2);

                        //Now check if the first string in the hardware ID matches the device ID of the USB device we are trying to find.
                        String DeviceIDFromRegistry = Marshal.PtrToStringUni(PropertyValueBuffer); //Make a new string, fill it with the contents from the PropertyValueBuffer

                        Marshal.FreeHGlobal(PropertyValueBuffer);		//No longer need the PropertyValueBuffer, free the memory to prevent potential memory leaks

                        //Now check if the hardware ID we are looking at contains the correct VID/PID (ignore upper/lower case)
                        if (DeviceIDFromRegistry.ToLowerInvariant().Contains(DeviceIdSubstring))
                        {
                            //Device must have been found.  In order to open I/O file handle(s), we will need the actual device path first.
                            //We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
                            //time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
                            //get the structure (after we have allocated enough memory for the structure.)
                            DetailedInterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(DetailedInterfaceDataStructure);
                            //First call populates "StructureSize" with the correct value
                            SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, IntPtr.Zero, 0, ref StructureSize, IntPtr.Zero);
                            //Need to call SetupDiGetDeviceInterfaceDetail() again, this time specifying a pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA buffer with the correct size of RAM allocated.
                            //First need to allocate the unmanaged buffer and get a pointer to it.
                            IntPtr pUnmanagedDetailedInterfaceDataStructure = IntPtr.Zero;  //Declare a pointer.
                            pUnmanagedDetailedInterfaceDataStructure = Marshal.AllocHGlobal((int)StructureSize);    //Reserve some unmanaged memory for the structure.
                            DetailedInterfaceDataStructure.cbSize = 6; //Initialize the cbSize parameter (4 bytes for DWORD + 2 bytes for unicode null terminator)
                            Marshal.StructureToPtr(DetailedInterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, false); //Copy managed structure contents into the unmanaged memory buffer.

                            //Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the device path in the structure at pUnmanagedDetailedInterfaceDataStructure.
                            if (SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, StructureSize, IntPtr.Zero, IntPtr.Zero))
                            {
                                //Need to extract the path information from the unmanaged "structure".  The path starts at (pUnmanagedDetailedInterfaceDataStructure + sizeof(DWORD)).
                                IntPtr pToDevicePath = new IntPtr((uint)pUnmanagedDetailedInterfaceDataStructure.ToInt32() + 4);  //Add 4 to the pointer (to get the pointer to point to the path, instead of the DWORD cbSize parameter)
                                devicePath = Marshal.PtrToStringUni(pToDevicePath); //Now copy the path information into the globally defined DevicePath String.

                                //Now check if the device path we are looking at contains the substring (ignore upper/lower case) 
                                if(devicePath.ToLowerInvariant().Contains(DevicePathSubstring))
                                {
                                    //We now have the proper device path, and we can finally use the path to open I/O handle(s) to the device.
                                    SetupDiDestroyDeviceInfoList(DeviceInfoTable);  //Clean up the old structure we no longer need.
                                    Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                                    return devicePath;    //Returning the device path
                                }
                            }
                            else //Some unknown failure occurred
                            {
                                uint ErrorCode = (uint)Marshal.GetLastWin32Error();
                                SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure.
                                Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                                return null;
                            }
                        }

                        InterfaceIndex++;
                        //Keep looping until we either find a device with matching VID and PID, or until we run out of devices to check.
                        //However, just in case some unexpected error occurs, keep track of the number of loops executed.
                        //If the number of loops exceeds a very large number, exit anyway, to prevent inadvertent infinite looping.
                        LoopCounter++;
                        if (LoopCounter == 10000)    //Surely there aren't more than 10'000 devices attached to any forseeable PC...
                        {
                            return null;
                        }
                    }//end of while(true)
                }
                return null;
            }//end of try
            catch
            {
                //Something went wrong if PC gets here.  Maybe a Marshal.AllocHGlobal() failed due to insufficient resources or something.
                return null;
            }
        } //findDevice
    

        public void UsbThread_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] OutBufferArray = new byte[65];
            byte[] InBufferArray = new byte[65];
            UsbBuffer OutBuffer = new UsbBuffer(OutBufferArray);
            UsbBuffer InBuffer = new UsbBuffer(InBufferArray);
            uint BytesWritten = 0;
            uint BytesRead = 0;

            while (true)
            {
                //Do not try to use the read/write handles unless the USB device is attached and ready
                if (ConnectionStatus == UsbConnectionStatus.Connected)
                {

                    // Raise SendPacket event if there are any subscribers
                    if (RaiseSendPacketEvent != null)
                    {
                        //Ask the application if a packet should be sent and let it prepare the data to be sent
                        RaiseSendPacketEvent(this, OutBuffer);
                    }

                    //Send packet if the application requested so
                    if(OutBuffer.RequestTransfer)
                    {
                        try
                        {
                            /*
                            byte[] buf = new byte[65];
                            buf[0] = OutBuffer.buffer[0];
                            buf[1] = OutBuffer.buffer[1];
                            buf[2] = OutBuffer.buffer[2];
                            */
                            OutBuffer.TransferSuccessful = WriteFile(WriteHandleToUSBDevice, OutBufferArray, 65, ref BytesWritten, IntPtr.Zero);
                        }
                        catch
                        {
                            OutBuffer.TransferSuccessful = false;
                        }

                        // A packet has been sent (or the transfer has failed)
                        // Inform the application by raising a PacketSent event if there are any subscribers
                        if (RaisePacketSentEvent != null)
                        {
                            RaisePacketSentEvent(this, OutBuffer);
                        }
                    }

                    // Raise ReceivePacket event if there are any subscribers
                    if (RaiseReceivePacketEvent != null)
                    {
                        // Ask the application if a packet should be requested
                        RaiseReceivePacketEvent(this, InBuffer);
                    }

                    // Receive packet if the application requested so
                    if (InBuffer.RequestTransfer)
                    {
                        try
                        {
                            InBuffer.TransferSuccessful = ReadFileManagedBuffer(ReadHandleToUSBDevice, InBufferArray, 65, ref BytesRead, IntPtr.Zero);
                        }
                        catch
                        {
                            InBuffer.TransferSuccessful = false;
                        }

                        // A packet has been received (or the transfer has failed)
                        // Inform the application by raising a PacketReceived event if there are any subscribers
                        if (RaisePacketReceivedEvent != null)
                        {
                            RaisePacketReceivedEvent(this, InBuffer);
                        }
                    }

                    
                } // end of: if(AttachedState == true)
                else
                {
                    Thread.Sleep(5); // Add a small delay to avoid unnecessary CPU utilization
                }
            } // end of while(true) loop
        } // end of ReadWriteThread_DoWork


        //--------------------------------------------------------------------------------------------------------------------------
        //FUNCTION:	ReadFileManagedBuffer()
        //PURPOSE:	Wrapper function to call ReadFile()
        //
        //INPUT:	Uses managed versions of the same input parameters as ReadFile() uses.
        //
        //OUTPUT:	Returns boolean indicating if the function call was successful or not.
        //          Also returns data in the byte[] INBuffer, and the number of bytes read. 
        //
        //Notes:    Wrapper function used to call the ReadFile() function.  ReadFile() takes a pointer to an unmanaged buffer and deposits
        //          the bytes read into the buffer.  However, can't pass a pointer to a managed buffer directly to ReadFile().
        //          This ReadFileManagedBuffer() is a wrapper function to make it so application code can call ReadFile() easier
        //          by specifying a managed buffer.
        //--------------------------------------------------------------------------------------------------------------------------
        public unsafe bool ReadFileManagedBuffer(SafeFileHandle hFile, byte[] INBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped)
        {
            IntPtr pINBuffer = IntPtr.Zero;

            try
            {
                pINBuffer = Marshal.AllocHGlobal((int)nNumberOfBytesToRead);    //Allocate some unmanged RAM for the receive data buffer.

                if (ReadFile(hFile, pINBuffer, nNumberOfBytesToRead, ref lpNumberOfBytesRead, lpOverlapped))
                {
                    Marshal.Copy(pINBuffer, INBuffer, 0, (int)lpNumberOfBytesRead);    //Copy over the data from unmanged memory into the managed byte[] INBuffer
                    Marshal.FreeHGlobal(pINBuffer);
                    return true;
                }
                else
                {
                    Marshal.FreeHGlobal(pINBuffer);
                    return false;
                }

            }
            catch
            {
                if (pINBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pINBuffer);
                }
                return false;
            }
        }


        
        public void OpenDevice(String DevicePath)
        {
            uint ErrorStatusWrite;
            uint ErrorStatusRead;
            // Close device first
            CloseDevice();
            // Open WriteHandle
            WriteHandleToUSBDevice = CreateFile(DevicePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            ErrorStatusWrite = (uint)Marshal.GetLastWin32Error();
            // Open ReadHandle
            ReadHandleToUSBDevice = CreateFile(DevicePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            ErrorStatusRead = (uint)Marshal.GetLastWin32Error();
            // Check if both handles were opened successfully
            if ((ErrorStatusWrite == ERROR_SUCCESS) && (ErrorStatusRead == ERROR_SUCCESS))
            {
                ConnectionStatus = UsbConnectionStatus.Connected;
            }
            else // For some reason the device was physically plugged in, but one or both of the read/write handles didn't open successfully
            {
                ConnectionStatus = UsbConnectionStatus.NotWorking;
                if (ErrorStatusWrite == ERROR_SUCCESS)
                {
                    WriteHandleToUSBDevice.Close();
                }
                    
                if (ErrorStatusRead == ERROR_SUCCESS)
                {
                    ReadHandleToUSBDevice.Close();
                }  
            }
            // Raise event if there are any subscribers
            if (RaiseConnectionStatusChangedEvent != null)
            {
                RaiseConnectionStatusChangedEvent(this, new ConnectionStatusEventArgs(ConnectionStatus));
            }
            // Start async thread if connection has been established
            if (ConnectionStatus == UsbConnectionStatus.Connected)
            {
                //UsbThread.RunWorkerAsync();
            }
        }

        // Close connection to the USB device
        public void CloseDevice()
        {
            // Save current status
            UsbConnectionStatus previousStatus = ConnectionStatus;
            // Close write and read handles if a device is connected
            if (ConnectionStatus==UsbConnectionStatus.Connected)      
            {
                WriteHandleToUSBDevice.Close();
                ReadHandleToUSBDevice.Close();
            }
            // Set status to disconnected
            ConnectionStatus = UsbConnectionStatus.Disconnected;
            // Stop async thread if connection has been established
            //UsbThread.CancelAsync();
            // Raise event if the status has changed and if there are any subscribers
            if (ConnectionStatus!=previousStatus)
            {
                if (RaiseConnectionStatusChangedEvent != null)
                {
                    RaiseConnectionStatusChangedEvent(this, new ConnectionStatusEventArgs(ConnectionStatus));
                }
            }
        }
        

    }//hid_utility

} //namespace hid