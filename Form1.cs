
using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
//using System.Threading;

using hid;


namespace HID_PnP_Demo
{
    public partial class Form1 : Form
    {
        String DeviceIDToFind = "Vid_04d8&Pid_0054";
        //String DeviceIDToFind = "Vid_04d8&Pid_003f";
        //String DeviceIDToFind = "Vid_04D9&Pid_1603";
        HidUtility myHidUtility = new HidUtility();


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

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

        //--------------- Global Varibles Section ------------------

        //Variables used by the application/form updates.
        //bool PushbuttonPressed = false;		//Updated by ReadWriteThread, read by FormUpdateTimer tick handler (needs to be atomic)
        //bool ToggleLEDsPending = false;		//Updated by ToggleLED(s) button click event handler, used by ReadWriteThread (needs to be atomic)
        //Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
        Guid InterfaceClassGuid = new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30); 
	    //--------------- End of Global Varibles ------------------

        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


        //Need to check "Allow unsafe code" checkbox in build properties to use unsafe keyword.  Unsafe is needed to
        //properly interact with the unmanged C++ style APIs used to find and connect with the USB device.
        public unsafe Form1()
        {
            InitializeComponent();
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
            //Additional constructor code

            //Initialize tool tips, to provide pop up help when the mouse cursor is moved over objects on the form.
            ANxVoltageToolTip.SetToolTip(this.ANxVoltage_lbl, "If using a board/PIM without a potentiometer, apply an adjustable voltage to the I/O pin.");
            ANxVoltageToolTip.SetToolTip(this.progressBar1, "If using a board/PIM without a potentiometer, apply an adjustable voltage to the I/O pin.");
            ToggleLEDToolTip.SetToolTip(this.ToggleLEDs_btn, "Sends a packet of data to the USB device.");
            PushbuttonStateTooltip.SetToolTip(this.PushbuttonState_lbl, "Try pressing pushbuttons on the USB demo board/PIM.");

            //Register for WM_DEVICECHANGE notifications.  This code uses these messages to detect plug and play connection/disconnection events for USB devices
            DEV_BROADCAST_DEVICEINTERFACE DeviceBroadcastHeader = new DEV_BROADCAST_DEVICEINTERFACE();
            DeviceBroadcastHeader.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            DeviceBroadcastHeader.dbcc_size = (uint)Marshal.SizeOf(DeviceBroadcastHeader);
            DeviceBroadcastHeader.dbcc_reserved = 0;	//Reserved says not to use...
            DeviceBroadcastHeader.dbcc_classguid = InterfaceClassGuid;

            //Need to get the address of the DeviceBroadcastHeader to call RegisterDeviceNotification(), but
            //can't use "&DeviceBroadcastHeader".  Instead, using a roundabout means to get the address by 
            //making a duplicate copy using Marshal.StructureToPtr().
            IntPtr pDeviceBroadcastHeader = IntPtr.Zero;  //Make a pointer.
            pDeviceBroadcastHeader = Marshal.AllocHGlobal(Marshal.SizeOf(DeviceBroadcastHeader)); //allocate memory for a new DEV_BROADCAST_DEVICEINTERFACE structure, and return the address 
            Marshal.StructureToPtr(DeviceBroadcastHeader, pDeviceBroadcastHeader, false);  //Copies the DeviceBroadcastHeader structure into the memory already allocated at DeviceBroadcastHeaderWithPointer
            RegisterDeviceNotification(this.Handle, pDeviceBroadcastHeader, DEVICE_NOTIFY_WINDOW_HANDLE);
 
			//Now make an initial attempt to find the USB device, if it was already connected to the PC and enumerated prior to launching the application.
			//If it is connected and present, we should open read and write handles to the device so we can communicate with it later.
			//If it was not connected, we will have to wait until the user plugs the device in, and the WM_DEVICECHANGE callback function can process
			//the message and again search for the device.
			//if(CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
            if(myHidUtility.FindDevice(DeviceIDToFind))
			{
                myHidUtility.openDevice();

				if(myHidUtility.getAttachedState())
				{
                    if (myHidUtility.getAttachedButBroken())
                    {
                        StatusBox_txtbx.Text = "Device found but not working";
                    }
                    else
                    {
                        StatusBox_txtbx.Text = "Device Found, AttachedState = TRUE";
                    }
                        
				}
                else
                {
                    StatusBox_txtbx.Text = "Device not found, verify connect/correct firmware";
                }

                //Update list of attached devices
                myHidUtility.ScanDevices();
                string txt = myHidUtility.getDeviceListAsString();
                if (txt != textBox1.Text)
                {
                    textBox1.Text = txt;
                }

			}

			ReadWriteThread.RunWorkerAsync();   //Recommend performing USB read/write operations in a separate thread.  Otherwise,
                                                //the Read/Write operations are effectively blocking functions and can lock up the
                                                //user interface if the I/O operations take a long time to complete.

            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        } //Form1()


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
        //This is a callback function that gets called when a Windows message is received by the form.
        //We will receive various different types of messages, but the ones we really want to use are the WM_DEVICECHANGE messages.
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

                    //if (CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
                    if (myHidUtility.FindDevice(DeviceIDToFind))
                    {
                        myHidUtility.openDevice();

                        if (myHidUtility.getAttachedState())
                        {
                            if (myHidUtility.getAttachedButBroken())
                            {
                                StatusBox_txtbx.Text = "Device found but not working";
                            }
                            else
                            {
                                StatusBox_txtbx.Text = "Device Found, AttachedState = TRUE";
                            }

                        }
                        else
                        {
                            StatusBox_txtbx.Text = "Device not found, verify connect/correct firmware";
                        }

                        //else we did find the device, but AttachedState was already true.  In this case, don't do anything to the read/write handles,
                        //since the WM_DEVICECHANGE message presumably wasn't caused by our USB device.  
                    }
                    else	//Device must not be connected (or not programmed with correct firmware)
                    {
                        myHidUtility.closeDevice();
                    }

                }
            } //end of: if(m.Msg == WM_DEVICECHANGE)

            base.WndProc(ref m);
        } //end of: WndProc() function
        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void ToggleLEDs_btn_Click(object sender, EventArgs e)
        {
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
            //ToggleLEDsPending = true;	//Will get used asynchronously by the ReadWriteThread
            myHidUtility.setToggleLEDsPending();
            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

        }


        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
            //This timer tick event handler function is used to update the user interface on the form, based on data
            //obtained asynchronously by the ReadWriteThread and the WM_DEVICECHANGE event handler functions.

            //Check if user interface on the form should be enabled or not, based on the attachment state of the USB device.
            if (myHidUtility.getAttachedState() == true)
            {
                //Device is connected and ready to communicate, enable user interface on the form 
                StatusBox_txtbx.Text = "Device Found: AttachedState = TRUE";
                PushbuttonState_lbl.Enabled = true;	//Make the label no longer greyed out
                ANxVoltage_lbl.Enabled = true;
                ToggleLEDs_btn.Enabled = true;
            }
            if ((myHidUtility.getAttachedState() == false) || (myHidUtility.getAttachedButBroken() == true))
            {
                //Device not available to communicate. Disable user interface on the form.
                StatusBox_txtbx.Text = "Device Not Detected: Verify Connection/Correct Firmware";
                PushbuttonState_lbl.Enabled = false;	//Make the label no longer greyed out
                ANxVoltage_lbl.Enabled = false;
                ToggleLEDs_btn.Enabled = false;
                //Update list of attached devices
                int deviceCount = myHidUtility.ScanDevices();
                string txt = myHidUtility.getDeviceListAsString() + Environment.NewLine + deviceCount.ToString();
                if(txt != textBox1.Text)
                {
                    textBox1.Text = txt;
                }

                PushbuttonState_lbl.Text = "Pushbutton State: Unknown";
                myHidUtility.setADCValue(0);
                //ADCValue = 0;
                progressBar1.Value = 0;
            }
            if ((myHidUtility.getAttachedState() == false) && (myHidUtility.getAttachedButBroken() == true))
            {
                StatusBox_txtbx.Text = "Device found but not working";
            }

            //Update the various status indicators on the form with the latest info obtained from the ReadWriteThread()
            if (myHidUtility.getAttachedState() == true)
            {
                //Update the pushbutton state label.
                if (myHidUtility.getPushbuttonPressed() == false)
                    PushbuttonState_lbl.Text = "Pushbutton State: Not Pressed";		//Update the pushbutton state text label on the form, so the user can see the result 
                else
                    PushbuttonState_lbl.Text = "Pushbutton State: Pressed";			//Update the pushbutton state text label on the form, so the user can see the result 
                if(myHidUtility.getToggleLEDsPending())
                    PushbuttonState_lbl.Text += ", LED toggle pending";
                //Update the ANxx/POT Voltage indicator value (progressbar)
                progressBar1.Value = (int) myHidUtility.getADCValue();
            }
            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public void ReadWriteThread_DoWork(object sender, DoWorkEventArgs e)
        {
            myHidUtility.ReadWriteThread_DoWork(sender, e);
        }


    } //public partial class Form1 : Form
} //namespace HID_PnP_Demo