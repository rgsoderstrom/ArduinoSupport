
//
// MessageFormats.cs - messages between Laptop and Arduino
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
        KeepAlive     = 1,  // no action required, just keeps socket open
        StatusRequest = 2,

        MotorProfileSegment = 3, // one duration and speed for one motor
        ClearMotorProfile   = 4, // command to clear all speed/durations

        RunMotors         = 5,  // execute previously sent MotorTimeAndSpeed profile
        SlowStopMotors    = 6,  // slowly stop motors
        FastStopMotors    = 7,  // immediately stop motors
        SendFirstCollection = 8,
        SendNextCollection  = 9,
        Disconnect          = 99,
    };

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class KeepAliveMsg : Header
    {
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StatusRequestMsg : Header
    {
    }

    //**********************************************************************

    // Motor Speed Profile Message

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class MotorSpeedProfileMsg
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        public class Segment
        {
            public short index;     // 0 -> (MaxNumberSegments - 1)
            public short motorID;   // 1 or 2, left or right
            public short speed;     // -15 -> 15
            public short duration;  // tenths of second, 0 -> 25.5
        }

        public Header  header;
        public Segment data;
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ClearSpeedProfile : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class RunMotorsMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SlowStopMotorsMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class FastStopMotorsMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendFirstCollectionMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class SendNextCollectionMsg : Header
    {
    }
    
    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class DisconnectMsg : Header
    {
    }
    
    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    public enum ArduinoMessageIDs {AcknowledgeMsgId  = 1,
                                   TextMsgId   = 2,
                                   StatusMsgId = 3,
                                   CollectedDataMsgId = 4,
                                   CollSendCompleteMsgId = 5,
    };

    //************************************************************************************************
    //************************************************************************************************
    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class AcknowledgeMessage
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public class AckData
        {
            public short MsgSequenceNumber;
        }

        public Header  header;
        public AckData data;
    };

    //************************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class TextMessage
    {
        public Header header;
        public char[] text; 
    };

    //************************************************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class StatusMessage
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public class StatusData
        {
            public short readyForMessages;
            public short motorsRunning; 
        }

        public Header     header;
        public StatusData data;
    }
    //************************************************************************************************
    
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class EncoderCountsMessage
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public partial class Batch
        {
            [StructLayout(LayoutKind.Sequential, Pack=1)]
            public struct Sample
            {
                public byte enc1; 
                public byte enc2;
            }

            static public readonly int MaxNumberSamples = 16;

            public short put;
            public Sample [] counts = new Sample [MaxNumberSamples];
        }

        public bool IsEmpty {get {return data.put == 0;}}

        public Header header;
        public Batch  data;      
    }

    //************************************************************************************************
    
    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class CollSendCompleteMessage : Header
    {
    };
}

