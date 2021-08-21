using System;
using System.Windows;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using Common;
using ArduinoInterface;
using Plot2D_Embedded;

//
// Application specific code
//  - for MotorsOnly project
//

namespace MotorsOnly
{
    public partial class MainWindow
    {
        SpeedProfile profile;

        //**************************************************************************************

        private void SendButton_Click (object sender, RoutedEventArgs e)
        {
            ClearProfileMsg msg = new ClearProfileMsg ();
            ServerSocket.SendToAllClients (msg.ToBytes ());
            StartSendingProfile ();
            RunButton.IsEnabled = true;
        }

        private int profileGet = 0;

        private void StartSendingProfile ()
        {
            profileGet = 0;
            SendNextProfileSegment ();
        }

        private void SendNextProfileSegment ()
        {
            if (profileGet < profile.NumberProfileSamples)
            {
                List<double> left = new List<double> ();
                List<double> right = new List<double> ();

                int count = profile.GetMessageData (profileGet, ProfileSection.MaxNumberValues, left, right);

                if (count > 0)
                {
                    ProfileSectionMsg msg = new ProfileSectionMsg (profileGet, left, right);
                    profileGet += count;
                    ServerSocket.SendToAllClients (msg.ToBytes ());
                }
            }
        }

        //**************************************************************************************

        private void RunButton_Click (object sender, RoutedEventArgs e)
        {
            RunProfileMsg msg = new RunProfileMsg ();
            ServerSocket.SendToAllClients (msg.ToBytes ());
        }

        //**************************************************************************************

        private void LoadButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = ScenarioFileName.Text;
                profile = new SpeedProfile (ScenarioFilePath + fileName);

                List<Point> left = new List<Point> (); 
                List<Point> right = new List<Point> ();

                profile.GetPlotData (ref left, ref right);

                LineView lv1 = new LineView (left);
                lv1.Color = Brushes.Red;

                LineView lv2 = new LineView (right);
                lv2.Color = Brushes.Green;

                PlotArea.Hold = true;
                PlotArea.Plot (lv1);
                PlotArea.Plot (lv2);
                PlotArea.RectangularGridOn = true;

                SendButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                Print (ex.Message);
            }
        }

        //**************************************************************************************

        private void MessageProcessing (ushort msgID, ushort seqNumber, byte[] messageBytes)
        {
            switch (msgID)
            {
                case (ushort)ArduinoMessageIDs.TextMsgId:
                    TextMessage tm = new TextMessage (messageBytes);
                    string str = new string (tm.text);
                    Print (str);
                    break;

                case (ushort)ArduinoMessageIDs.ProfileSectionRcvdMsgId:
                    Print ("received ack");
                    SendNextProfileSegment ();
                    break;

                default:
                    Print ("Unrecognized message ID");
                    break;
            }


        }
    }

    //*************************************************************
}
