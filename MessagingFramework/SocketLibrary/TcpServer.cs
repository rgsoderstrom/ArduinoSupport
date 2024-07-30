
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

        Socket listeningSocket0 = null; // listens for connections from clients
        Socket listeningSocket1 = null; // listens for connections from clients

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

                //foreach (IPAddress ipAddress in ipHostInfo.AddressList)
                //{
                //    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                //    {
                //        print (string.Format ("IP address:   {0}", ipAddress));
                //        IPEndPoint  localEndPoint = new IPEndPoint (ipAddress, CommandPort);

                //        listeningSocket = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //        listeningSocket.Bind (localEndPoint);
                //        listeningSocket.Listen (5);

                //      //
                //      // start an asynchronous task to accept connections
                //      //            
                //        Callback functionPtr = new Callback (AcceptConnections);
                //        functionPtr.BeginInvoke (null, null);
                //    }
                //}

                int index = 0;

                for ( ; index < ipHostInfo.AddressList.Length; index++)
                { 
                    IPAddress ipAddress = ipHostInfo.AddressList [index];

                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    { 
                        print (string.Format ("IP address:   {0}", ipAddress));
                        IPEndPoint localEndPoint = new IPEndPoint (ipAddress, CommandPort);

                        listeningSocket0 = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listeningSocket0.Bind (localEndPoint);
                        listeningSocket0.Listen (5);

                        Callback functionPtr = new Callback (AcceptConnections0);
                        functionPtr.BeginInvoke (null, null);

                        break;
                    }
                }


                for (index++ ; index < ipHostInfo.AddressList.Length; index++)
                { 
                    IPAddress ipAddress = ipHostInfo.AddressList [index];

                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    { 
                        print (string.Format ("IP address:   {0}", ipAddress));
                        IPEndPoint localEndPoint = new IPEndPoint (ipAddress, CommandPort);

                        listeningSocket1 = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listeningSocket1.Bind (localEndPoint);
                        listeningSocket1.Listen (5);

                        Callback functionPtr = new Callback (AcceptConnections1);
                        functionPtr.BeginInvoke (null, null);

                        break;
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
        void AcceptConnections0 ()
        {        
            try
            { 
                while (true)  // loop here accepting connections
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset ();

                    PrintHandler?.Invoke ("Ready for connections");
                    listeningSocket0.BeginAccept  (new AsyncCallback (AcceptCallback0), listeningSocket0);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne ();
                }
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptConnection0 exception: {0}", ex.Message));
            }
        }

        void AcceptConnections1 ()
        {        
            try
            { 
                while (true)  // loop here accepting connections
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset ();

                    PrintHandler?.Invoke ("Ready for connections");
                    listeningSocket1.BeginAccept  (new AsyncCallback (AcceptCallback1), listeningSocket1);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne ();
                }
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptConnection1 exception: {0}", ex.Message));
            }
        }

        //*********************************************************************************************************

        void AcceptCallback0 (IAsyncResult ar)
        {
            try
            {
                PrintHandler?.Invoke ("Accepted connection 0");

                // Signal the main thread to continue.
                allDone.Set ();

                // Get the socket that will handle messages to/from the client
                Socket clientSocket = listeningSocket0.EndAccept (ar);
                NewConnectionHandler?.Invoke (clientSocket);
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptCallback0 exception: {0}", ex.Message)); 
            }
        }

        void AcceptCallback1 (IAsyncResult ar)
        {
            try
            {
                PrintHandler?.Invoke ("Accepted connection 1");

                // Signal the main thread to continue.
                allDone.Set ();

                // Get the socket that will handle messages to/from the client
                Socket clientSocket = listeningSocket1.EndAccept (ar);
                NewConnectionHandler?.Invoke (clientSocket);
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptCallback1 exception: {0}", ex.Message)); 
            }
        }

        //************************************************************************************************

    } 
}
