
using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    public class TcpServer
    {
        //
        // callbacks to methods in class hosting this socket
        //
        public event Callback2       NewConnectionHandler;
        public event PrintCallback   PrintHandler;

        //***************************************************

        private int CommandPort = 11000;   // commands to Arduino, response back

        Socket listeningSocket = null; // listens for connections from clients

        //****************************************************************************************

        // Thread signal.
        static ManualResetEvent allDone = new ManualResetEvent (false);

        public TcpServer (PrintCallback print)
        {
            try
            {
                string      machineName   = Dns.GetHostName (); // this machine
                IPHostEntry ipHostInfo    = Dns.GetHostEntry (machineName);

                print (string.Format ("Machine name: {0}", machineName));
                print (string.Format ("Port:         {0}", CommandPort));

              // open server sockets at all IPv4 address

                foreach (IPAddress ipAddress in ipHostInfo.AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        print (string.Format ("IP address:   {0}", ipAddress));
                        IPEndPoint  localEndPoint = new IPEndPoint (ipAddress, CommandPort);

                        listeningSocket = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listeningSocket.Bind (localEndPoint);
                        listeningSocket.Listen (1);

                      //
                      // start an asynchronous task to accept connections
                      //            
                        Callback functionPtr = new Callback (AcceptConnections);
                        functionPtr.BeginInvoke (null, null);
                    }
                }
            }

            catch (Exception ex)
            {
                print (string.Format ("TcpServer ctor exception: {0}", ex.Message));
            }
        }

        //*********************************************************************************************************
        //
        //  Loops here until program teminates
        //
        void AcceptConnections ()
        {        
            while (true)  // loop here accepting connections
            {
                // Set the event to nonsignaled state.
                allDone.Reset ();

                PrintHandler?.Invoke ("Ready for connections");
                listeningSocket.BeginAccept  (new AsyncCallback (AcceptCallback), listeningSocket);

                // Wait until a connection is made before continuing.
                allDone.WaitOne ();
            }
        }

        //*********************************************************************************************************

        void AcceptCallback (IAsyncResult ar)
        {
            try
            {
                PrintHandler?.Invoke ("Accepted connection");

                // Signal the main thread to continue.
                allDone.Set ();

                // Get the socket that will handle messages to/from the client
                Socket clientSocket = listeningSocket.EndAccept (ar);
                NewConnectionHandler?.Invoke (clientSocket);
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptCallback exception: {0}", ex.Message)); 
            }
        }

        //************************************************************************************************

    } 
}
