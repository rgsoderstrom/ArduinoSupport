using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts; // for Synchronization attribute
using System.Collections.Generic;

namespace SocketLibrary
{
    public class TcpClient
    {
        //
        // callbacks to methods in class hosting this socket
        //
        public event MessageCallback MessageHandler;
        public event PrintCallback   PrintHandler;


        // true if connected to a server
        public bool Connected {get {if (client == null) return false; else return client.Connected;}}

        const int RetryCount = 10;
        const int ConnectionWait = 5; // seconds to wait for connection

        //***************************************************

        // The port number for the remote device.
        private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = new ManualResetEvent (false);
        private static ManualResetEvent sendDone    = new ManualResetEvent (false);
        private static ManualResetEvent receiveDone = new ManualResetEvent (false);

        public Socket client = null; // copy of socket in StateObject

        static readonly object messageBytesLock = new object ();

        public TcpClient (PrintCallback print)
        {
            for (int count = 0; count<RetryCount; count++)
            {
                try
                {
                  //string machineName = "RandysLaptop";
                    string machineName = "RandysLG";
                    IPHostEntry ipHostInfo = Dns.GetHostEntry (machineName);

                    // find and use the IPv4 address
                    int select = 0;

                    for (; select<ipHostInfo.AddressList.Length; select++)
                        if (ipHostInfo.AddressList [select].AddressFamily == AddressFamily.InterNetwork)
                            break;

                    if (select == ipHostInfo.AddressList.Length)
                        throw new Exception ("No IPv4 address found");

                    IPAddress ipAddress = ipHostInfo.AddressList [select]; // IPv4

                    print (string.Format ("Looking for {0} at {1}", machineName, ipAddress));

                    IPEndPoint remoteEP = new IPEndPoint (ipAddress, port);

                    // Create the TCP/IP socket.
                    client = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // Connect to the remote endpoint.
                    client.BeginConnect (remoteEP, new AsyncCallback (ConnectCallback), client);
                    connectDone.WaitOne (ConnectionWait * 1000);

                    // Create the state object.
                    StateObject state = new StateObject ();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.
                    client.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);
                    print ("Connected");

                    return;
                }
                catch (Exception e)
                {
                    print ("TcpClient failed to connect to server");
                    Thread.Sleep (1000);
                }
            }
        }

        public void Close ()
        {
            if (client.Connected)
                client.Close ();
        }

        //*****************************************************************************************************

        private void ConnectCallback (IAsyncResult ar)
        {
            try
            {
                PrintHandler?.Invoke ("ConnectCallback");

                // Retrieve the socket from the state object.
                Socket client2 = (Socket)ar.AsyncState;

                // Complete the connection.
                client2.EndConnect (ar);

                //PrintHandler?.Invoke (string.Format ("Socket connected to {0}", client2.RemoteEndPoint.ToString ()));

                // Signal that the connection has been made.
                connectDone.Set ();
            }
            catch (Exception)
            {
                PrintHandler?.Invoke ("Exception in Connect Callback");
            }
        }

        //*****************************************************************************************************

        void ReceiveCallback (IAsyncResult ar)
        {
            PrintHandler?.Invoke ("Receive Callback");
 
            try
            {
                String content = String.Empty;

              // Retrieve the state object and the handler socket from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

              // Read data from the socket. 
                int bytesRead = handler.EndReceive (ar);

                //PrintHandler?.Invoke (string.Format ("read count {0}", bytesRead));

                if (bytesRead > 0)
                {
                    lock (messageBytesLock)
                    {
                        TcpUtils.ExtractMessage (state, bytesRead, MessageHandler);
                    }

                  // get ready for next receive
                    handler.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);
                }
                else
                   state.workSocket.Close ();
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("ReadCallback exception: {0}", ex.Message));
            }
        }

        //*****************************************************************************************************

        public void Send (byte [] msg)
        {
            try
            {
                //PrintHandler?.Invoke ("Send");

                // Begin sending the data to the remote device.
                client.BeginSend (msg, 0, msg.Length, 0, new AsyncCallback (SendCallback), client);
            }

            catch (Exception ex)
            {
                PrintHandler?.Invoke (string.Format ("Send exception: {0}", ex.Message));
            }
        }

        //*****************************************************************************************************

        private void SendCallback (IAsyncResult ar)
        {
            try
            {
                //PrintHandler?.Invoke ("SendCallback");

                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend (ar);
                //PrintHandler?.Invoke (string.Format ("Sent {0} bytes to server.", bytesSent));

                // Signal that all bytes have been sent.
                sendDone.Set ();
            }
            catch (Exception e)
            {
                PrintHandler?.Invoke (e.ToString ());
            }
        }

        //*********************************************************************************************

        // return an object that caller will cast to correct message type

        //object bytesArrayToMessageType (byte[] msgBytes, Type msgType)
        //{
        //    object obj = null;
        //    IntPtr ptrObj = IntPtr.Zero;

        //    try
        //    {
        //        int objSize = Marshal.SizeOf (msgType);

        //        if (objSize > 0)
        //        {
        //            if (msgBytes.Length < objSize)
        //                throw new Exception(String.Format("Buffer smaller than needed for creation of object of type {0}", msgType));
            
        //            ptrObj = Marshal.AllocHGlobal(objSize);
        
        //            if (ptrObj != IntPtr.Zero)
        //            {
        //                Marshal.Copy(msgBytes, 0, ptrObj, objSize);
        //                obj = Marshal.PtrToStructure(ptrObj, msgType);
        //            }
        //            else
        //                throw new Exception(String.Format("Couldn't allocate memory to create object of type {0}", msgType));
        //        }
        //    }

        //   catch (Exception ex)
        //   {
        //       PrintHandler?.Invoke (string.Format ("ReceiveCallback Exception: {0}", ex.Message));
        //   }

        //   finally
        //   {
        //       if (ptrObj != IntPtr.Zero)
        //           Marshal.FreeHGlobal(ptrObj);
        //   }

        //    return obj;
        //}
    }
}
