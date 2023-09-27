using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts; // for Synchronization attribute
using System.Runtime.InteropServices;   // for Marshal

using Common;

namespace SocketLib
{
    // internal and client callbacks
    public delegate void Callback ();
    public delegate void MessageCallback (Socket src, byte[] msg);
    public delegate void PrintCallback (string str);

    //*********************************************************************************************************

    // State object for asynchronous socket tasks

    [Synchronization]
    public class StateObject : ContextBoundObject
    {
        public Socket workSocket    = null; // Client  socket.
        public const int BufferSize = 1024; // Size of receive buffer.
        public byte [] buffer       = new byte [BufferSize]; // bytes just received

        public List<byte> pendingMsgBytes = new List<byte> (); // bytes wait here until complete message received
    }

    //*********************************************************************************************************
    //
    // Class for utility functions
    //
    internal static class TcpUtils
    {
        internal static void ExtractMessage (StateObject state, int bytesRead, MessageCallback callback)
        {
            try
            {
                // append to any old not yet processed bytes
                for (int i = 0; i<bytesRead; i++)
                    state.pendingMsgBytes.Add (state.buffer [i]);

                // if there are enough bytes for a message
                while (state.pendingMsgBytes.Count >= Marshal.SizeOf (typeof (MessageHeader)))
                {
                    // first 2 bytes should be sync word
                    ushort first2Bytes = (ushort)(state.pendingMsgBytes [1] << 8 | state.pendingMsgBytes [0]);

                    do
                    {
                        if (first2Bytes == Message.Sync)
                            break;
                        else
                            state.pendingMsgBytes.RemoveAt (0);

                        first2Bytes = (ushort)(state.pendingMsgBytes [1] << 8 | state.pendingMsgBytes [0]);

                    } while (state.pendingMsgBytes.Count >= Marshal.SizeOf (typeof (MessageHeader)));

                    // see if we have a complete message pass to handler
                    if (first2Bytes == Message.Sync && state.pendingMsgBytes.Count >= 8)
                    {
                        ushort msgByteCount = (ushort)(state.pendingMsgBytes [3] << 8 | state.pendingMsgBytes [2]);

                        if (state.pendingMsgBytes.Count >= msgByteCount) // if we have the entire message
                        {
                            byte [] msg = new byte [msgByteCount];
                            state.pendingMsgBytes.CopyTo (0, msg, 0, msgByteCount);
                            state.pendingMsgBytes.RemoveRange (0, msgByteCount);

                            callback?.Invoke (state.workSocket, msg);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("TCP Utils.ExtractMessage Exception: {0}", ex.Message));
            }
        }
    }
}
