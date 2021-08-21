using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SocketLib;
using ArduinoInterface;

namespace ControlConsoleTest
{
    public class ArduinoSim
    {
        Timer Timer1 = null;
        SampleManager SampleStore = null;

        SocketLib.TcpClient thisClientSocket = null;

        //****************************************************************************

        public ArduinoSim ()
        {

        }

        //****************************************************************************

        DateTime startTime = DateTime.Now;

        UInt32 millis ()
        {
            DateTime now = DateTime.Now;
            TimeSpan messageTimeTag = now - startTime;
            return (UInt32) messageTimeTag.TotalMilliseconds;
        }

        //****************************************************************************

        static void Timer1Interrupt (object state)
        {
            ArduinoSim arduino = (ArduinoSim)state;
            arduino.Sample ();
        }

        void Sample ()
        {
            //Console.Write ("S");

            const float ampl = 5;
            const float baseTemperature = 60;  

            DateTime now = DateTime.Now;
            TimeSpan runTime = now - startTime;

            float temperature = (float) (baseTemperature + ampl * Math.Sin (2 * Math.PI * runTime.TotalSeconds / 180) * Math.Exp (-1 * runTime.TotalSeconds / 1200));
            UInt32 time = (UInt32) runTime.TotalMilliseconds;

            SampleStore.Store (time, temperature);
        }

        //****************************************************************************

        public void SendToConsole (TemperatureMessage msg)
        {
            //Console.WriteLine ("  Send");
            thisClientSocket.Send (msg.ToBytes ());
        }

        //****************************************************************************

        public void Run ()
        {
            try
            {
                Console.WriteLine ("Connect to server");
                thisClientSocket = new SocketLib.TcpClient (PrintToConsole);
            }

            catch (Exception)
            {
                Console.WriteLine ("Exception in Main");
            }

            if (thisClientSocket.Connected == false)
            {
                Console.WriteLine ("\n\nFailed to connect to server");

                while (true)
                    Thread.Sleep (1000);
            }

            thisClientSocket.MessageHandler += MessageHandler;
            thisClientSocket.PrintHandler   += PrintToConsole;

            Timer1 = new Timer (Timer1Interrupt, this, Timeout.Infinite, Timeout.Infinite);
            SampleStore = new SampleManager (this);

            while (true)
                Thread.Sleep (1000);
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        //************************************************************************************

        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            Header header = new Header (msgBytes);

            /*
            Console.Write ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);

            for (int i=Marshal.SizeOf (header); i<header.ByteCount; i++)
                Console.Write (" {0}", msgBytes [i]);

            Console.WriteLine ();
            */

            switch (header.MessageId)
            {
                case (ushort) CommandMessageIDs.KeepAlive:
                {
                    Console.WriteLine ("Received KeepAlive msg");
                }
                break;

                case (ushort)CommandMessageIDs.SendStatus:
                {
                    Console.WriteLine ("Received SendStatus cmnd");
                    ArduinoInterface.StatusMessage status = new ArduinoInterface.StatusMessage ();
                    status.sampling = true ? 1 : 0;
                    status.numberRamSamples = 0x3210;
                    src.Send (status.ToBytes ());  
                }
                break;

                case (ushort)CommandMessageIDs.StartSampling:
                {
                    Console.WriteLine ("Received StartSampling cmnd");
                    ArduinoInterface.StartSamplingCmdMsg startMsg = new ArduinoInterface.StartSamplingCmdMsg (msgBytes);

                    TimeSpan delay = new TimeSpan (0, 0, 1); // wait 1 second
                    TimeSpan period = new TimeSpan (0, 0, startMsg.period);

                    Timer1.Change (delay, period);
                }
                break;

                case (ushort) CommandMessageIDs.StopSampling:
                {
                    Console.WriteLine ("Received StopSampling cmnd");
                    SampleStore.SendPartialMessage ();
                    Timer1.Change (Timeout.Infinite, Timeout.Infinite);
                }
                break;

              
                /***

                case (ushort) CommandMessageIDs.ClearHistory:
                {
                    Console.WriteLine ("Received ClearHistory cmnd");
                }
                break;

                case (ushort) CommandMessageIDs.SendHistory:
                {
                    Console.WriteLine ("Received SendHistory cmnd");
                }
                break;

                case (ushort) CommandMessageIDs.SendContinuously:
                {
                    Console.WriteLine ("Received SendContinuously cmnd");
                }
                break;
                ***/

                case (ushort) CommandMessageIDs.Disconnect:
                {
                    Console.WriteLine ("Received Disconnect cmnd");
                    thisClientSocket.client.Disconnect (false);                    
                }
                break;
                

                default:
                     Console.WriteLine ("Received unrecognized message");
                break;
            }
        }

    }
}
