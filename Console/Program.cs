using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HidUtilityNuget;

namespace HidDemoConsole
{


    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            HidDemo demo = new HidDemo(0x04D8, 0x0054);
            demo.Run();
        }
    }

    public class HidDemo
    {
        private HidUtility HidUtil;
        private ushort Vid;
        private ushort Pid;

        private byte LastCommand = 0x81;
        private bool WaitingForDevice = false;
        private bool PushbuttonPressed = false;
        private bool ToggleLedPending = false;
        private uint AdcValue = 0;
        private DateTime ConnectedTimestamp = DateTime.Now;
        private uint TxCount = 0;
        private uint RxCount = 0;

        public HidDemo(ushort Vid, ushort Pid)
        {
            // Display startup message
            Console.WriteLine("Welcome to HID Console Demo");

            // Initialize class variables
            HidUtil = new HidUtility();
            this.Vid = Vid;
            this.Pid = Pid;

            // Set device to connect to
            HidUtil.SelectDevice(new Device(Vid, Pid));

            // Subscribe to events
            HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedHandler;
            HidUtil.RaiseDeviceAddedEvent += DeviceAddedHandler;
            HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;
            HidUtil.RaiseSendPacketEvent += SendPacketHandler;
            HidUtil.RaisePacketSentEvent += PacketSentHandler;
            HidUtil.RaiseReceivePacketEvent += ReceivePacketHandler;
            HidUtil.RaisePacketReceivedEvent += PacketReceivedHandler;

            // Initialization is completed
            Console.WriteLine(string.Format("  Device: Vid=0x{0:X4}, Pid=0x{1:X4}", Vid, Pid));
            Console.WriteLine(string.Format("  Connection status: {0}", HidUtil.ConnectionStatus.ToString()));

            // Print available commands
            Console.WriteLine("Available Commands (all commands are case-insensitive):");
            Console.WriteLine("  v, vid <hex string>: Change vendor ID");
            Console.WriteLine("  p, pid <hex string>: Change product ID");
            Console.WriteLine("  d, devices: List available devices");
            Console.WriteLine("  s, status: Print status information");
            Console.WriteLine("  r, read: Read ADC value and pushbutton status");
            Console.WriteLine("  t, toggle: Toggle LED");
            Console.WriteLine("  q, quit: Exit the application");
        }

        // A USB device has been removed
        // Update the event log and device list
        void DeviceRemovedHandler(object sender, Device dev)
        {
            Console.WriteLine(string.Format("Device removed: {0}", dev.ToString()));
        }

        // A USB device has been added
        // Update the event log and device list
        void DeviceAddedHandler(object sender, Device dev)
        {
            Console.WriteLine(string.Format("Device added: {0}", dev.ToString()));
        }

        // Connection status of our selected device has changed
        // Reset variables
        void ConnectionStatusChangedHandler(object sender, HidUtility.ConnectionStatusEventArgs e)
        {
            Console.WriteLine(string.Format("Connection status changed to: {0}", e.ConnectionStatus.ToString()));
            LastCommand = 0x81;
            WaitingForDevice = false;
            PushbuttonPressed = false;
            ToggleLedPending = false;
            AdcValue = 0;
            ConnectedTimestamp = DateTime.Now;
            TxCount = 0;
            RxCount = 0;
        }

        // HidUtility asks if a packet should be sent to the device
        // Prepare the buffer and request a transfer
        public void SendPacketHandler(object sender, UsbBuffer OutBuffer)
        {
            // Fill entire buffer with 0xFF
            OutBuffer.clear();
            if (ToggleLedPending == true)
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus. Always set = 0.
                OutBuffer.buffer[0] = 0;
                // 0x80 is the "Toggle LED" command in the firmware
                OutBuffer.buffer[1] = 0x80;
                ToggleLedPending = false;
                LastCommand = 0x80;
            }
            else if (LastCommand == 0x81)
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                OutBuffer.buffer[0] = 0x00;
                // READ_POT command (see the firmware source code), gets 10-bit ADC Value
                OutBuffer.buffer[1] = 0x37;
                LastCommand = 0x37;
            }
            else
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                OutBuffer.buffer[0] = 0x00;
                // 0x81 is the "Get Pushbutton State" command in the firmware
                OutBuffer.buffer[1] = 0x81;
                LastCommand = 0x81;
            }
            // Request that this buffer be sent
            OutBuffer.RequestTransfer = true;
        }

        // HidUtility informs us if the requested transfer was successful
        // Schedule to request a packet if the transfer was successful
        public void PacketSentHandler(object sender, UsbBuffer OutBuffer)
        {
            if (LastCommand == 0x80)
            {
                WaitingForDevice = false;
            }
            else
            {
                WaitingForDevice = OutBuffer.TransferSuccessful;
            }
            ++TxCount;
        }

        // HidUtility asks if a packet should be requested from the device
        // Request a packet if a packet has been successfully sent to the device before
        public void ReceivePacketHandler(object sender, UsbBuffer InBuffer)
        {
            InBuffer.RequestTransfer = WaitingForDevice;

        }

        // HidUtility informs us if the requested transfer was successful and provides us with the received packet
        public void PacketReceivedHandler(object sender, UsbBuffer InBuffer)
        {
            WaitingForDevice = false;
            if (InBuffer.buffer[1] == 0x37)
            {
                AdcValue = (uint)(InBuffer.buffer[3] << 8) + InBuffer.buffer[2];
            }
            if (InBuffer.buffer[1] == 0x81)
            {
                if (InBuffer.buffer[2] == 0x01)
                {
                    PushbuttonPressed = false;
                }
                if (InBuffer.buffer[2] == 0x00)
                {
                    PushbuttonPressed = true;
                }
            }
            ++RxCount;
        }

        public void Run()
        {
            while (true)
            {
                // Read command line input
                Console.Write(">> ");
                string consoleInput = Console.ReadLine();
                consoleInput = consoleInput.Trim();
                //Convert to lower case to ease future comparison
                consoleInput = consoleInput.ToLower();
                // Skip any blank commands
                if (string.IsNullOrWhiteSpace(consoleInput))
                {
                    continue;
                }
                // Quit if command equals q or quit
                if(consoleInput=="q" || consoleInput=="quit")
                {
                    break;
                }
                // Try to execute the command
                try
                {
                    // Execute the command
                    Execute(consoleInput);
                }
                catch (Exception ex)
                {
                    // Something went wrong - Write out the problem
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void Execute(string consoleInput)
        {
            //Divide console input into command and parameter(s) (if any)
            string[] input = consoleInput.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            string command = input[0];
            List<string> parameters = input.ToList<string>();
            parameters.RemoveAt(0);
            // Decide what function to call
            switch(command)
            {
                case "v":
                case "vid":
                    CommandVid(parameters);
                    break;
                case "p":
                case "pid":
                    CommandPid(parameters);
                    break;
                case "d":
                case "devices":
                    CommandDevices();
                    break;
                case "s":
                case "status":
                    CommandStatus();
                    break;
                case "r":
                case "read":
                    CommandRead();
                    break;
                case "t":
                case "toggle":
                    CommandToggle();
                    break;
                default:
                    Console.WriteLine("Invalid command");
                    break;
            }
        }

        //Change Vendor ID
        private void CommandVid(List<string> parameters)
        {
            ushort vid = ParseHex(parameters.ElementAt(0));
            if (vid != Vid)
            {
                Vid = vid;  
                Console.WriteLine(string.Format("New device: Vid=0x{0:X4}, Pid=0x{1:X4}", Vid, Pid));
                HidUtil.SelectDevice(new Device(Vid, Pid));
            }
            else
            {
                Console.WriteLine("New VID matches current VID");
            }
        }

        //Change Product ID
        private void CommandPid(List<string> parameters)
        {
            ushort pid = ParseHex(parameters.ElementAt(0));
            if(pid!=Pid)
            {
                Pid = pid;
                Console.WriteLine(string.Format("New device: Vid=0x{0:X4}, Pid=0x{1:X4}", Vid, Pid));
                HidUtil.SelectDevice(new Device(Vid, Pid));
            }
            else
            {
                Console.WriteLine("New PID matches current PID");
            }
        }

        //List available devices
        private void CommandDevices()
        {
            Console.WriteLine(string.Format("{0} devices available:", HidUtil.DeviceList.Count.ToString()));
            foreach(Device dev in HidUtil.DeviceList)
            {
                Console.WriteLine(dev.ToString());
            }
        }

        //Print current connection status
        private void CommandStatus()
        {
            Console.WriteLine(string.Format("Connection status: {0}", HidUtil.ConnectionStatus.ToString()));
        }

        //Read ADC value and 
        private void CommandRead()
        {
            if (HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
            {
                if (PushbuttonPressed)
                {
                    Console.WriteLine(string.Format("ADC value: {0}, pushbutton pressed", AdcValue.ToString()));
                }
                else
                {
                    Console.WriteLine(string.Format("ADC value: {0}, pushbutton not pressed", AdcValue.ToString()));
                }
            }
            else
            {
                Console.WriteLine("Command not valid when not connected");
            }
        }

        //Toggle LED
        private void CommandToggle()
        {
            if (HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
            {
                if (ToggleLedPending)
                {
                    Console.WriteLine("Operation already in progress");
                }
                else
                {
                    ToggleLedPending = true;
                    Console.WriteLine("LED toggle requested");
                }
            }
            else
            {
                Console.WriteLine("Command not valid when not connected");
            }
        }

        // Try to convert a (hexadecimal) string to an unsigned 16-bit integer
        // Return 0 if the conversion fails
        private ushort ParseHex(string input)
        {
            input = input.ToLower();
            if (input.Length >= 2)
            {
                if (input.Substring(0, 2) == "0x")
                {
                    input = input.Substring(2);
                }
            }
            try
            {
                ushort value = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
                return value;
            }
            catch
            {
                return 0;
            }
        }
    }
}