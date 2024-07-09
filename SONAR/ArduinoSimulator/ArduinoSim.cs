using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SocketLibrary;
using ArduinoInterface;

namespace ArduinoSimulator
{
    public class ArduinoSim
    {
        bool Verbose = false;

        SocketLibrary.TcpClient thisClientSocket = null;

        string Name;

        //****************************************************************************

        int seconds = 100;

        public ArduinoSim (string name, int sec)
        {
            Name = name;
            seconds = sec;
        }

        //****************************************************************************

        public void Run ()
        {
            try
            {
                Console.WriteLine ("Connecting to server");
                thisClientSocket = new SocketLibrary.TcpClient (PrintToConsole); 

                if (thisClientSocket.Connected == false)
                {
                    Console.WriteLine ("\n\nFailed to connect to server");

                    while (true)
                        Thread.Sleep (1000);
                }

                thisClientSocket.MessageHandler += MessageHandler;
                thisClientSocket.PrintHandler   += PrintToLog; // PrintToConsole;


                ReadyMsg_Auto readyMsg = new ReadyMsg_Auto ();
                thisClientSocket.Send (readyMsg.ToBytes ());



                //TextMsg_Auto tm = new TextMsg_Auto ();
                //tm.data.text = "Arduino Ready".ToCharArray ();
                //thisClientSocket.Send (tm.ToBytes ());



                while (true)
                { 
                    Thread.Sleep (1000);
                }

                //PrintToConsole (Name + " closing socket");

                //thisClientSocket.Close ();

                //while (true)
                //    Thread.Sleep (1000);               
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in Main.Run: " + ex.Message);
            }
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        private void PrintToLog (string str)
        {
            Console.WriteLine (str);
        }

        //************************************************************************************

        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            try
            {
                MessageHeader header = new MessageHeader (msgBytes);

                if (Verbose)
                {
                    Console.Write ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);

                    for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                        Console.Write (" {0}", msgBytes [i]);

                    Console.WriteLine ();
                }

                switch (header.MessageId)
                {
                    case (ushort) ArduinoMessageIDs.ClearMsgId:
                        Console.WriteLine ("Clear message received");

                        TextMessage tm = new TextMessage ("Arduino received Clear message");
                        thisClientSocket.Send (tm.ToBytes ());

                        break;
                        
                    case (ushort) ArduinoMessageIDs.CollectMsgId:
                        Console.WriteLine ("Collect message received");
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendMsgId:
                        Console.WriteLine ("Send message received");
                        break;
                        
                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        break;
                        
                    default:
                        Console.WriteLine ("Received unrecognized message, Id: " + header.MessageId.ToString ());
                        break;
                }

                MessageHeader hdr = new MessageHeader (msgBytes);

                AcknowledgeMsg_Auto ackMsg = new AcknowledgeMsg_Auto ();
                ackMsg.data.MsgSequenceNumber = hdr.SequenceNumber;
                thisClientSocket.Send (ackMsg.ToBytes ());

                //ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
                //thisClientSocket.Send (rdyMsg.ToBytes ());
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + Name + ", " + ex.Message);
            }
        }    
    }
}


