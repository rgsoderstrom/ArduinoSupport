
//
// HeaderMessages.cs - messages with header only, no additional data
//

using System.Runtime.InteropServices; // for StructLayout

using SocketLibrary;

namespace ArduinoInterface
{
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class KeepAliveMsg : Header
    {
        public KeepAliveMsg ()
        {
            MessageId = (ushort) PCMessageIDs.KeepAlive;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StatusRequestMsg : Header
    {
        public StatusRequestMsg ()
        {
            MessageId = (ushort) PCMessageIDs.StatusRequest;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class RunLoopbackTestMsg : Header
    {
        public RunLoopbackTestMsg ()
        {
            MessageId = (ushort) PCMessageIDs.RunLoopbackTestMsgId;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendLoopbackTestResultsMsg : Header
    {
        public SendLoopbackTestResultsMsg ()
        {
            MessageId = (ushort) PCMessageIDs.SendLoopbackDataMsgId;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class DisconnectMsg : Header
    {
    }

    //************************************************************************************************
}

