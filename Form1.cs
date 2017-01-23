
using System;
using System.Windows.Forms;
using hid;

namespace HID_PnP_Demo
{

    public partial class Form1 : Form
    {
        HidUtility myHidUtility;

        //Variables used by the application/form updates.
        byte lastCommand = 0x81;
        bool waitingForDevice = false;
        bool PushbuttonPressed = false;     //Updated by ReadWriteThread, read by FormUpdateTimer tick handler (needs to be atomic)
        bool ToggleLEDsPending = false;     //Updated by ToggleLED(s) button click event handler, used by ReadWriteThread (needs to be atomic)
        uint ADCValue = 0;			//Updated by ReadWriteThread, read by FormUpdateTimer tick handler (needs to be atomic)

        String DeviceIDToFind = "Vid_04d8&Pid_0054";

        // Callback functions
        public bool sendPacket(ref byte[] outBuffer)
        {
            if (ToggleLEDsPending == true)
            {
                outBuffer[0] = 0;               //The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                outBuffer[1] = 0x80;            //0x80 is the "Toggle LED(s)" command in the firmware
                for (uint i = 2; i < 65; i++)
                {
                    outBuffer[i] = 0xFF;
                }
                lastCommand = 0x80;
            }
            else if (lastCommand==0x81)
            {
                outBuffer[0] = 0x00;    //The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                outBuffer[1] = 0x37;    //READ_POT command (see the firmware source code), gets 10-bit ADC Value
                                        //Initialize the rest of the 64-byte packet to "0xFF".  Binary '1' bits do not use as much power, and do not cause as much EMI
                                        //when they move across the USB cable.  USB traffic is "NRZI" encoded, where '1' bits do not cause the D+/D- signals to toggle states.
                                        //This initialization is not strictly necessary however.
                for (uint i = 2; i < 65; i++)
                {
                    outBuffer[i] = 0xFF;
                }
                lastCommand = 0x37;

            }
            else
            {
                //Get the pushbutton state from the microcontroller firmware.
                outBuffer[0] = 0x00;           //The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                outBuffer[1] = 0x81;        //0x81 is the "Get Pushbutton State" command in the firmware
                for (uint i = 2; i < 65; i++)
                {
                    outBuffer[i] = 0xFF;
                }
                lastCommand = 0x81;
            }
            return true;
        }

        public void packetSent(bool success)
        {
            if(lastCommand==0x80)
            {
                if(success)
                {
                    ToggleLEDsPending = false;
                }
            }
            else
            {
                waitingForDevice = success;
            }
        }

        public bool receivePacket()
        {
            return waitingForDevice;
        }

        public void packetReceived(ref byte[] inBuffer)
        {
            waitingForDevice = false;

            if (inBuffer[1] == 0x37)
            {
                ADCValue = (uint)(inBuffer[3] << 8) + inBuffer[2];  //Need to reformat the data from two unsigned chars into one unsigned int.
            }
            if (inBuffer[1] == 0x81)
            {
                if (inBuffer[2] == 0x01)
                {
                    PushbuttonPressed = false;
                }
                if (inBuffer[2] == 0x00)
                {
                    PushbuttonPressed = true;
                }   
            }
        }
          

        //Need to check "Allow unsafe code" checkbox in build properties to use unsafe keyword.  Unsafe is needed to
        //properly interact with the unmanged C++ style APIs used to find and connect with the USB device.
        public unsafe Form1()
        {
            InitializeComponent();

            // Instantiate the delegates.
            sendPacket_delegate sendPacket_handler = sendPacket;
            packetSent_delegate packetSent_handler = packetSent;
            receivePacket_delegate receivePacket_handler = receivePacket;
            packetReceived_delegate packetReceived_handler = packetReceived;
            deviceChange_delegate deviceChange_handler = deviceChange;

            //Initialize tool tips, to provide pop up help when the mouse cursor is moved over objects on the form.
            ANxVoltageToolTip.SetToolTip(this.ANxVoltage_lbl, "If using a board/PIM without a potentiometer, apply an adjustable voltage to the I/O pin.");
            ANxVoltageToolTip.SetToolTip(this.progressBar1, "If using a board/PIM without a potentiometer, apply an adjustable voltage to the I/O pin.");
            ToggleLEDToolTip.SetToolTip(this.ToggleLEDs_btn, "Sends a packet of data to the USB device.");
            PushbuttonStateTooltip.SetToolTip(this.PushbuttonState_lbl, "Try pressing pushbuttons on the USB demo board/PIM.");

            //Get an instance of HidUtility
            myHidUtility = new HidUtility(sendPacket_handler, packetSent_handler, receivePacket_handler, packetReceived_handler);

            //Sign up for device change notifications
            //Code is located in file Form1_EventNotifications
            InitializeDeviceChangeNotifications(deviceChange_handler);

            //Now make an initial attempt to find the USB device, if it was already connected to the PC and enumerated prior to launching the application.
            //If it is connected and present, we should open read and write handles to the device so we can communicate with it later.
            //If it was not connected, we will have to wait until the user plugs the device in, and the WM_DEVICECHANGE callback function can process
            //the message and again search for the device.
            //if(CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
            if (myHidUtility.FindDevice(DeviceIDToFind))
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
        } //Form1()

        //This is a callback function that gets called when a WM_DEVICECHANGE message is received by the form.
        public void deviceChange()
        {
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
            else    //Device must not be connected (or not programmed with correct firmware)
            {
                myHidUtility.closeDevice();
            }
        }


        private void ToggleLEDs_btn_Click(object sender, EventArgs e)
        {
            ToggleLEDsPending = true;	//Will get used asynchronously by the ReadWriteThread
        }


        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            //This timer tick event handler function is used to update the user interface on the form, based on data
            //obtained asynchronously by the ReadWriteThread and the WM_DEVICECHANGE event handler functions.

            //Check if user interface on the form should be enabled or not, based on the attachment state of the USB device.
            if (myHidUtility.getAttachedState() == true)
            {
                //Device is connected and ready to communicate, enable user interface on the form 
                //StatusBox_txtbx.Text = "Device Found: AttachedState = TRUE";
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
                //myHidUtility.setADCValue(0);
                ADCValue = 0;
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
                if (PushbuttonPressed == false)
                    PushbuttonState_lbl.Text = "Pushbutton State: Not Pressed";		//Update the pushbutton state text label on the form, so the user can see the result 
                else
                    PushbuttonState_lbl.Text = "Pushbutton State: Pressed";			//Update the pushbutton state text label on the form, so the user can see the result 
                if(ToggleLEDsPending)
                    PushbuttonState_lbl.Text += ", LED toggle pending";
                //Update the ANxx/POT Voltage indicator value (progressbar)
                progressBar1.Value = (int) ADCValue;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    } //public partial class Form1 : Form
} //namespace HID_PnP_Demo