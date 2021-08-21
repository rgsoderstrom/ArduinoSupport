using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices; // for StructLayout, Marshal
using System.Runtime.Remoting.Contexts; // for Synchronization attribute


namespace SocketLib
{
    public class TcpServer
    {
        //
        // callbacks to methods in class hosting this socket
        //
        public event MessageCallback MessageHandler;
        public event Callback        NewConnectionHandler;
        public event Callback        ClosedConnectionHandler;
        public event PrintCallback   PrintHandler;

        //***************************************************

        private int CommandPort = 11000;   // commands to Arduino, response back

        Socket listeningSocket = null; // listens for connections from clients

        static readonly object messageBytesLock = new object ();

        List<Socket> allClients = new List<Socket> ();

        public int NumberClients {get {return allClients.Count;}}

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

        void AcceptConnections ()
        {
          //
          // loop here accepting connections
          //
            while (true)
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

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept (ar);

                allClients.Add (handler);

                // Create the state object.
                StateObject state = new StateObject ();
                state.workSocket = handler;

                NewConnectionHandler?.Invoke ();

                handler.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("AcceptCallback exception: {0}", ex.Message)); 
            }
        }

        //*********************************************************************************************************
        //
        // ReceiveCallback - 
        //    

        void ReceiveCallback (IAsyncResult ar)
        {
            try
            {
                //PrintHandler?.Invoke ("Message received");

                String content = String.Empty;

              // Retrieve the state object and the handler socket from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

              // Read data from the socket. 
                int bytesRead = handler.EndReceive (ar);

                //PrintHandler?.Invoke (string.Format ("{0} bytes", bytesRead));

                if (bytesRead == 0)
                {
                    allClients.Remove (state.workSocket);
                    state.workSocket.Close ();
                    ClosedConnectionHandler?.Invoke ();
                }

                else if (bytesRead > 0)
                {
                    lock (messageBytesLock)
                    {
                        TcpUtils.ExtractMessage (state, bytesRead, MessageHandler);
                    }

                  // get ready for next receive
                    handler.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);
                }
            }

            catch (SocketException)
            {
                StateObject state = (StateObject)ar.AsyncState;
                allClients.Remove (state.workSocket);
                state.workSocket.Close ();
                ClosedConnectionHandler?.Invoke ();
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("ReadCallback exception: {0}", ex.Message));
            }
        }

        //************************************************************************************************

        public void SendToAllClients (byte[] msgBytes)
        {
            foreach (Socket sock in allClients)
                if (sock.Connected)
                    sock.Send (msgBytes);
        }
    } 
}
