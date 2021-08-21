
//
// Messages.cs - 
//
 
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; // for StructLayout

using SocketLib;

namespace ArduinoInterface
{
    //**********************************************************************
    //
    // PC -> Arduino message IDs
    //
        public enum CommandMessageIDs {KeepAlive        = 0,  // no action required, just keeps socket open
                                       SendStatus       = 1,
                                       StartSampling    = 2,
                                       StopSampling     = 3,
                                       ClearHistory     = 4, 
                                       SendHistory      = 5, 
                                       //SendContinuously = 6,
                                       //StopSending      = 7,
                                       Disconnect       = 8};

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class KeepAliveMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendStatusCmdMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StartSamplingCmdMsg
    {
        public Header header;
        public ushort period; // sample period in seconds
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StopSamplingCmdMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ClearHistoryCmdMsg : Header
    {
    }
    
    //**********************************************************************
    
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendHistoryCmdMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendContinuouslyCmdMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StopSendingCmdMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class DisconnectCmdMsg : Header
    {
    }

    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    public enum ArduinoMessageIDs {StatusMsgId = 1,
                                   TemperatureMsgId = 2};
    
    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StatusMessage
    {
        public Header header;     
        public byte  sampling;
        public ushort numberRamSamples;
    };

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct TemperatureSample
    {
        public uint  time;  // 32 bit milliseconds, from start time
        public float temperature;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class TemperatureMessage
    {
        public static uint startTime;

        public Header   header;
        public ushort   numberSamples;
        public TemperatureSample[] Samples;      
    }
}
