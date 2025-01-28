using System;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media;

using System.Collections.Generic;
using System.Windows.Interop;

using Common;
//using ArduinoInterface;
using SocketLibrary;
using Plot2D_Embedded;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Linq;

namespace PioneerSensors
{
    public partial class ArduinoWindow : Window
    {
        public ArduinoWindow ()
        {
            InitializeComponent ();
        }

        public ArduinoWindow (Socket sock)
        {
            InitializeComponent ();
        }

        //********************************************************************************

        private void ArduinoWindow_Loaded (object sender, RoutedEventArgs e)
        {

        }

        private void ZoomOptionButton_Checked (object sender, RoutedEventArgs e)
        {

        }

        private void ClearButton_Click (object sender, RoutedEventArgs e)
        {

        }

        private void PingButton_Click (object sender, RoutedEventArgs e)
        {

        }

        private void SendSamplesButton_Click (object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click (object sender, RoutedEventArgs e)
        {

        }

        private void ClearPlotButton_Click (object sender, RoutedEventArgs e)
        {

        }
    }
}
