using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ArduinoInterface;
using SocketLibrary;

namespace ArduinoSimulator
{
    public class ArduinoSim_AppSpecific : ArduinoSimBase
    {
        public ArduinoSim_AppSpecific (string name, 
                                       SocketLibrary.TcpClient sock,
                                       PrintCallback ptl) : base (name, sock, ptl)
        {            
            thisClientSocket.MessageHandler += MessageHandler;
        }

		//**********************************************************************************
		//**********************************************************************************
		//**********************************************************************************
		
        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            try
            {
                MessageHeader header = new MessageHeader (msgBytes);

                if (Verbose)
                {
                    PrintToLog ("");

                    PrintToLog (string.Format ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber));

                    for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                        PrintToLog (string.Format (" {0}", msgBytes [i]));
                }

                //**************************************************************************
                //
                // Acknowledge before handling
                //

                AcknowledgeMsg_Auto ackMsg = new AcknowledgeMsg_Auto ();
                ackMsg.data.MsgSequenceNumber = header.SequenceNumber;
                thisClientSocket.Send (ackMsg.ToBytes ());

                //**************************************************************************

                switch (header.MessageId)
                {
                    case (ushort)ArduinoMessageIDs.Button1MsgId:
                        if (Verbose) PrintToLog ("Button1 message received");
                        Button1MessageHandler (msgBytes);
                        break;

                    case (ushort)ArduinoMessageIDs.Button2MsgId:
                        if (Verbose) PrintToLog ("Button2 message received");
                        Button2MessageHandler (msgBytes);
                        break;

                    case (ushort)ArduinoMessageIDs.Button3MsgId:
                        if (Verbose) PrintToLog ("Button3 message received");
                        Button3MessageHandler (msgBytes);
                        break;

                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        if (Verbose) PrintToLog ("Keep-Alive message received");
                        break;
                        
                    default:
                        PrintToLog ("Received unrecognized message, Id: " + header.MessageId.ToString ());
                        break;
                }
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception: " + ThisArduinoName + ", " + ex.Message);
            }
        }
		
        //***************************************************************************************************************
        //***************************************************************************************************************
        //***************************************************************************************************************

        private void Button1MessageHandler (byte [] msgBytes)
        {
            Button1Msg_Auto msg = new Button1Msg_Auto (msgBytes);
            PrintToLog ("Button1 message handler");
        }

        private void Button2MessageHandler (byte [] msgBytes)
        {
            Button2Msg_Auto msg = new Button2Msg_Auto (msgBytes);
            PrintToLog ("Button2 message handler");

            for (int i=0; i<msg.data.Count; i++)
                PrintToLog (msg.data.Sample [i].ToString ());
        }

        private void Button3MessageHandler (byte [] msgBytes)
        {
            Button3Msg_Auto msg = new Button3Msg_Auto (msgBytes);
            PrintToLog ("Button3 message handler");

            PrintToLog ("param1 = " + msg.data.Param1);
            PrintToLog ("param2 = " + msg.data.Param2);
            PrintToLog ("param3 = " + msg.data.Param3);
        }
    }
}
