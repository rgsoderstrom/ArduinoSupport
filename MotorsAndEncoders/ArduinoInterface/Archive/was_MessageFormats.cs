
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
    public enum CommandMessageIDs 
    {
        KeepAlive       = 0,  // no action required, just keeps socket open
        MotorSpeed      = 1,
        StartCollection = 2,
        StopCollection  = 3,
        ClearCollection = 4,
        SendFirstCollection = 5,
        SendNextCollection  = 6,
        Disconnect          = 8,

        ClearProfile   = 100,  // for MotorOnly
        ProfileSection = 101,  // for MotorOnly
        RunProfile     = 102,  // for MotorOnly
        StopRun        = 103,  // for MotorOnly
    };

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class KeepAliveMsg : Header
    {
        
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class MotorSpeedMsg
    {
        readonly Header header;
        readonly short  Velocity1;
        readonly short  Velocity2;
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StartCollectionMsg : Header
    {        
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StopCollectionMsg : Header
    {        
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ClearCollectionMsg : Header
    {        
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendFirstCollectionMsg : Header
    {        
    }
    
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendNextCollectionMsg : Header
    {        
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class DisconnectMsg : Header
    {
    }
    
    //**********************************************************************
    //**********************************************************************
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ClearProfileMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public class ProfileSection
    {
        static public readonly int MaxNumberValues = 10;
        
        public short index; // of this msg Values [0] in the entire profile
        public short numberValues;
        public short[] LeftSpeed  = new short [MaxNumberValues];
        public short[] RightSpeed = new short [MaxNumberValues];
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ProfileSectionMsg
    {
        Header header;
        ProfileSection data;
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class RunProfileMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StopRunMsg : Header
    {
    }
    
    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    public enum ArduinoMessageIDs {StatusMsgId = 1,
                                   TextMsgId   = 2,
                                   BufferStatusMsgId  = 3,
                                   CollectedDataMsgId = 4,
                                   CollSendCompleteMsgId = 5,

                                   ProfileSectionRcvdMsgId = 101,
    };

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    //[StructLayout(LayoutKind.Sequential, Pack=1)]
    //public partial class StatusMessage
    //{
    //    public Header header;     
    //    public byte  sampling;
    //    public ushort numberRamSamples;
    //};

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class TextMessage
    {
        public Header header;
        public char[] text; 
    };

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class BufferStatusMessage
    {
        public Header header;
        public byte   data;
    };

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct EncoderCounts
    {
        public uint  time;   // 32 bit millis, from time "run" command received
        public short enc1; 
        public short enc2;
        public byte  s1;  // was char
        public byte  s2;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class CollectionData
    {
        static public readonly int MsgBufferSize = 10;

        public short put;
        public EncoderCounts [] counts = new EncoderCounts [MsgBufferSize];
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class CollectionDataMessage
    {
        public bool Empty {get {return data.put == 0;}}

        public Header         header;
        public CollectionData data;      
    }

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class CollSendCompleteMessage : Header
    {
    };

    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class ProfileSectionRcvdMessage : Header
    {
    };

    //************************************************************************************************

    //[StructLayout(LayoutKind.Sequential, Pack=1)]
    //public partial class StatusMessage
    //{
    //    public Header header;     
    //    public byte  sampling;
    //    public ushort numberRamSamples;
    //};


}

