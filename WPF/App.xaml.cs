using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using hid;




namespace HidDemoWpf
{

    /*
     *  The Model 
     */
    public class Communicator
    {
        public HidUtility HidUtil;
        private ushort _Vid;
        public ushort Vid
        {
            get
            {
                return _Vid;
            }
            set
            {
                if(value!=_Vid)
                {
                    _Vid = value;
                    HidUtil.SelectDevice(new Device(_Vid, _Pid));
                }
            }
        }
        private ushort _Pid;
        public ushort Pid
        {
            get
            {
                return _Pid;
            }
            set
            {
                if (value != _Pid)
                {
                    _Pid = value;
                    HidUtil.SelectDevice(new Device(_Vid, _Pid));
                }
            }
        }
        public bool LedTogglePending { get;  private set; } = false;

        /*
         * Local function definitions
         */




        /*
        // Add a line to the activity log text box
        void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                LogTextBox.Text = string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
            // Add new line
            else
            {
                LogTextBox.Text += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
            // Scroll to bottom
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            LogTextBox.ScrollToCaret();
        }

        // Try to convert a (hexadecimal) string to an unsigned 16-bit integer
        // Return 0 if the conversion fails
        // This function is used to parse the PID and VID text boxes
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
        */

        public Communicator()
        {
            HidUtil = new HidUtility();
            _Vid = 0x04D8;
            _Pid = 0x0054;
            HidUtil.SelectDevice(new Device(_Vid, _Pid));
        }

        public bool RequestLedToggleValid()
        {
            return !LedTogglePending;
        }

        public void RequestLedToggle()
        {
            LedTogglePending = true;
        }
    }

    /*
     *  The Command Class
     */

    public class UiCommand : ICommand
    {
        private Action _Execute;
        private Func<bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public UiCommand(Action Execute, Func<bool> CanExecute)
        {
            _Execute = Execute;
            _CanExecute = CanExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _CanExecute();
        }
        public void Execute(object parameter)
        {
            _Execute();
        }
    }

    /*
     *  The ViewModel 
     */
    public class CommunicatorViewModel : INotifyPropertyChanged
    {
        private Communicator communicator;
        private UiCommand buttonCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public CommunicatorViewModel()
        {
            communicator = new Communicator();
            communicator.HidUtil.RaiseDeviceAddedEvent += DeviceAddedEventHandler;
            communicator.HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedEventHandler;
            communicator.HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;

            buttonCommand = new UiCommand(this.RequestLedToggle, communicator.RequestLedToggleValid);
                
        }

        public void RequestLedToggle()
        {
            communicator.RequestLedToggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("LedTogglePendingTxt"));
            }
        }

        public  ICommand ToggleClick
        {
            get
            {
                return buttonCommand;
            }
        }
        
        public void DeviceAddedEventHandler(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DeviceListTxt"));
            }
        }

        public void DeviceRemovedEventHandler(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DeviceListTxt"));
            }
        }

        public void ConnectionStatusChangedHandler(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ConnectionStatusTxt"));
            }
        }

        public string LedTogglePendingTxt
        {
            get
            {
                if (communicator.LedTogglePending)
                    return "Toggle pending";
                else
                    return "No action pending";
            }
        }

        public string DeviceListTxt
        {
            get
            {
                string txt = "";
                foreach (Device dev in communicator.HidUtil.DeviceList)
                {
                    string devString = string.Format("VID=0x{0:X4} PID=0x{1:X4}: {2} ({3})", dev.Vid, dev.Pid, dev.Caption, dev.Manufacturer);
                    txt += devString + Environment.NewLine;
                }
                return txt.TrimEnd('\n');
            }
            
        }

        // Try to convert a (hexadecimal) string to an unsigned 16-bit integer
        // Return 0 if the conversion fails
        // This function is used to parse the PID and VID text boxes
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

        public string VidTxt
        {
            get
            {
                return string.Format("0x{0:X4}", communicator.Vid);
            }
            set
            {
                communicator.Vid = ParseHex(value);
            }
        }

        public string PidTxt
        {
            get
            {
                return string.Format("0x{0:X4}", communicator.Pid);
            }
            set
            {
                communicator.Pid = ParseHex(value);
            }
        }

        public string ConnectionStatusTxt
        {
            get
            {
                return string.Format("Connection Status: {0}", communicator.HidUtil.ConnectionStatus.ToString());
            }

        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

    }
}
