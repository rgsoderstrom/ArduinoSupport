
//
// ArduinoSimBase - abstract base class for application specific simulator
//

using System;
using System.Threading;

using SocketLibrary;
using ArduinoInterface;

namespace ArduinoSimulator
{
    public abstract class ArduinoSimBase
    {
        protected bool Verbose = true;

        protected SocketLibrary.TcpClient thisClientSocket = null;
        protected PrintCallback PrintToLog;

        protected readonly string ThisArduinoName;

        //****************************************************************************
        //
        // protected ctor, invoked by derived class
        //
        protected ArduinoSimBase (string name, 
                                  SocketLibrary.TcpClient sock,
                                  PrintCallback ptl)
        {            
            ThisArduinoName  = name;
            thisClientSocket = sock;
            PrintToLog = ptl;
        }

        //****************************************************************************

        bool Running = true;

        public void Run ()
        {
            try
            {
                ReadyMsg_Auto readyMsg = new ReadyMsg_Auto ();
                thisClientSocket.Send (readyMsg.ToBytes ());
                
                TextMessage msg2 = new TextMessage ("Arduino sim ready");
                thisClientSocket.Send (msg2.ToBytes ());

                while (Running)
                { 
                    Thread.Sleep (1000);
                }

                PrintToLog (ThisArduinoName + " closing socket");
                thisClientSocket.Close ();

                while (true)
                    Thread.Sleep (1000);
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception in Main.Run: " + ex.Message);
            }
        }
    }
}


