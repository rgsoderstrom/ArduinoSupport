﻿
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
        //SendFirstCollection = 8,
        //SendNextCollection  = 9,
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
            public short speed;     // -127 -> 127
            public short duration;  // tenths of second, 0 -> 25.5
        }

        public Header  header;
        public Segment data;
    }

    //**********************************************************************

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class ClearSpeedProfileMsg : Header
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

    public enum ArduinoMessageIDs 
    {
        AcknowledgeMsgId  = 1,
        TextMsgId   = 2,
        StatusMsgId = 3,
        //EncoderCountsMsgId = 4,
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
            public ushort MsgSequenceNumber;
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
        public partial class StatusData
        {
            static public readonly int MaxNameLength = 8;

            public char [] name = new char [MaxNameLength];
            public short readyForMessages;
            public short readyToRun; 
            public short motorsRunning;
            public short readyToSend;
        }

        public Header     header;
        public StatusData data;
    }

    //************************************************************************************************
    
    //[StructLayout(LayoutKind.Sequential, Pack=1)]
    //public partial class EncoderCountsMessage
    //{
    //    [StructLayout(LayoutKind.Sequential, Pack=1)]
    //    public partial class Batch
    //    {
    //        [StructLayout(LayoutKind.Sequential, Pack=1)]
    //        public struct Sample
    //        {
    //            public byte enc1; 
    //            public byte enc2;
    //        }

    //        static public readonly int MaxNumberSamples = 16;

    //        public short put;  // number of samples in this batch
    //        public short lastBatch; // non-zero means this is last batch
    //        public Sample [] counts = new Sample [MaxNumberSamples];
    //    }

    //    public Header header;
    //    public Batch  data;      
    //}

    //************************************************************************************************
    
}

