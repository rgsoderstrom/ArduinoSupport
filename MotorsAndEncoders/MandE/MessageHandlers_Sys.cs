using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading.Tasks;
using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using Common;
using ArduinoInterface;
using Plot2D_Embedded;

namespace ShaftEncoders
{
    public partial class MainWindow
    {
        void MessageProcessing (object arg1, object arg2)
        {
            try
            {
                Socket sender    = arg1 as Socket;
                byte [] msgBytes = arg2 as byte [];

                if (msgBytes == null)
                {
                    Print ("msgBytes == null");
                    return;
                }

                ushort MsgId  = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("MessageId"));

                switch (MsgId)
                {
                    case (ushort) ArduinoMessageIDs.TextMsgId:          TextMessageHandler          (msgBytes); break;
                    case (ushort) ArduinoMessageIDs.StatusMsgId:        StatusMessageHandler        (msgBytes); break;
                    case (ushort) ArduinoMessageIDs.AcknowledgeMsgId:   AcknowledgeMessageHandler   (msgBytes); break;
                    case (ushort) ArduinoMessageIDs.EncoderCountsMsgId: EncoderCountsMessageHandler (msgBytes); break;

                    default: Print ("Unrecognized message ID"); break;
                }
            }

            catch (Exception ex)
            {
                Print (String.Format ("MessageProcessing Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        
        private void AcknowledgeMessageHandler (byte [] msgBytes)
        {
            AcknowledgeMessage msg = new AcknowledgeMessage (msgBytes);

            bool found = messageQueue.MessageAcknowledged (msg.data.MsgSequenceNumber);

            if (found == false)
                Print ("Ack'd message not found: " + msg.data.MsgSequenceNumber.ToString ());
        }

        //*******************************************************************************************************

        private void TextMessageHandler (byte[] msgBytes)
        {
            TextMessage msg = new TextMessage (msgBytes);
            string str = new string (msg.text);
            RemotePrint (str);
        }

        //*******************************************************************************************************

        delegate void MsgProcessingDelegate (object arg1, object arg2);

        void SocketMessageHandler (Socket sender, byte [] messageBytes)
        {
            object [] args = new object [2];
            args [0] = sender;
            args [1] = messageBytes;

            // this will run MessageProcessing in the main thread, i.e. the one that can
            // access WPF objects
            Dispatcher.BeginInvoke ((MsgProcessingDelegate) MessageProcessing, args);
        }

        //*******************************************************************************************************


        private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ServerSocket.NumberClients > 0)
            {
               ArduinoInterface.KeepAliveMsg msg = new KeepAliveMsg ();
               messageQueue.AddMessage (msg.ToBytes ());
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void SocketServer_newConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) GainedClient);
        }

        private void GainedClient ()
        {
            Print (string.Format ("Gained Client, {0} total", ServerSocket.NumberClients));
            ReadyCommunicateEllipse.Fill = Brushes.Green;

            //StartMotors_Button.IsEnabled   = true;
            //StopMotors_Button.IsEnabled    = true;
            //DisableMotors_Button.IsEnabled = true;
            //Disconnect_Button.IsEnabled    = true;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void SocketServer_closedConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) LostClient);
        }

        private void LostClient ()
        {
            Print (string.Format ("Lost Client, {0} remaining", ServerSocket.NumberClients));
            ReadyCommunicateEllipse.Fill = Brushes.White;

            //if (ServerSocket.NumberClients == 0)
            //{
            //    StartMotors_Button.IsEnabled   = false;
            //    StopMotors_Button.IsEnabled    = false;
            //    DisableMotors_Button.IsEnabled = false;
            //    Disconnect_Button.IsEnabled    = false;
            //}
        }

    }
}
