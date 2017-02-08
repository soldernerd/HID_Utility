using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Management;
using System.Text.RegularExpressions;

namespace HidDemoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            InitializeComponent();
            /*
            tblk_debug.AppendText(string.Format("\n{0}: System started", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
            List<Device> devices = new List<Device>();
            //List<Device> devices = getDeviceList();
            //devices = getDeviceList(49242, 1133);
            devices.Add(getDevice(49242, 1133));
            foreach (Device dev in devices)
            {
                tblk_debug.AppendText("\n" + dev.caption);
                tblk_debug.AppendText("\n" + dev.deviceId);
                tblk_debug.AppendText("\n" + dev.name);
                tblk_debug.AppendText("\n" + dev.description);
                tblk_debug.AppendText("\n" + dev.manufacturer);
                tblk_debug.AppendText("\n" + dev.pid);
                tblk_debug.AppendText("\n" + dev.vid);
                tblk_debug.AppendText("\n" + "--------------");
            }
            */
            //tblk_debug.AppendText("\n" + diffs.getCount());
            //tblk_debug.AppendText("\n" + diffs.getHostDiff(123).getString());

        }
    }
}
