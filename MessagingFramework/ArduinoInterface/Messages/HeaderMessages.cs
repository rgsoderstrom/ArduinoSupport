
//
// HeaderMessages.cs - messages with header only, no additional data
//

using System.Runtime.InteropServices; // for StructLayout

using SocketLibrary;

namespace ArduinoInterface
{
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class KeepAliveMsg : MessageHeader
    {
        public KeepAliveMsg ()
        {
            MessageId = (ushort) PCMessageIDs.KeepAlive;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StatusRequestMsg : MessageHeader
    {
        public StatusRequestMsg ()
        {
            MessageId = (ushort) PCMessageIDs.StatusRequest;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class RunLoopbackTestMsg : MessageHeader
    {
        public RunLoopbackTestMsg ()
        {
            MessageId = (ushort) PCMessageIDs.RunLoopbackTestMsgId;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendLoopbackTestResultsMsg : MessageHeader
    {
        public SendLoopbackTestResultsMsg ()
        {
            MessageId = (ushort) PCMessageIDs.SendLoopbackDataMsgId;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class DisconnectMsg : MessageHeader
    {
    }

    //************************************************************************************************
}

