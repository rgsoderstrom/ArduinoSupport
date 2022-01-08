
//
// MessageMethods.cs - messages between Laptop and Arduino
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; // for StructLayout

using SocketLib;

public delegate void PrintCallback (string str);

namespace ArduinoInterface
{
    //*******************************************************************************************************
    // PC -> Arduino messages *******************************************************************************
    //*******************************************************************************************************

    public partial class KeepAliveMsg
    {
        public KeepAliveMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.KeepAlive;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public KeepAliveMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class StatusRequestMsg
    {
        public StatusRequestMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.StatusRequest;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public StatusRequestMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class MotorSpeedProfileMsg
    {
        public MotorSpeedProfileMsg (short _index,     // 0 -> (MaxNumberSegments - 1)
                                     short _motorID,   // 1 or 2, left or right
                                     short _speed,     // -15 -> 15
                                     short _duration)  // tenths of second, 0 -> 25.5
        {
            header = new Header ();
            data = new Segment ();

            header.MessageId = (ushort)CommandMessageIDs.MotorProfileSegment;
            header.ByteCount = (ushort) (Marshal.SizeOf (header) + Marshal.SizeOf (data));

            data.index = _index;
            data.motorID = _motorID;
            data.speed = _speed;
            data.duration = _duration;
        }

        public MotorSpeedProfileMsg (byte[] fromBytes) // convert received byte stream to message
        {
            header  = new Header (fromBytes);
            data    = new Segment ();

            data.index    = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedProfileMsg> ("data") + (int) Marshal.OffsetOf<Segment> ("index"));
            data.motorID  = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedProfileMsg> ("data") + (int) Marshal.OffsetOf<Segment> ("motorID"));
            data.speed    = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedProfileMsg> ("data") + (int) Marshal.OffsetOf<Segment> ("speed"));
            data.duration = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedProfileMsg> ("data") + (int) Marshal.OffsetOf<Segment> ("duration"));
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.index));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.motorID));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.speed));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.duration));

          // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));
            
            return msgBytes;
        }

        public override string ToString ()
        {
            return header.ToString () + data.index + ", " + data.motorID + ", " + data.speed + ", " + data.duration;
        }
    }

 //****************************************************************************************************************************

    public partial class ClearSpeedProfile
    {
        public ClearSpeedProfile ()
        {
            MessageId = (ushort)CommandMessageIDs.ClearMotorProfile;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public ClearSpeedProfile (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class RunMotorsMsg
    {
        public RunMotorsMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.RunMotors;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public RunMotorsMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class SlowStopMotorsMsg
    {
        public SlowStopMotorsMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.SlowStopMotors;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public SlowStopMotorsMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class FastStopMotorsMsg
    {
        public FastStopMotorsMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.FastStopMotors;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public FastStopMotorsMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class SendFirstCollectionMsg
    {
        public SendFirstCollectionMsg ()
        {
            MessageId = (ushort)CommandMessageIDs.SendFirstCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public SendFirstCollectionMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class SendNextCollectionMsg
    {
        public SendNextCollectionMsg ()
        {
            MessageId = (ushort)CommandMessageIDs.SendNextCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public SendNextCollectionMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

    //****************************************************************************************************************************

    public partial class DisconnectMsg
    {
        public DisconnectMsg () : base ()
        {
            MessageId = (ushort)CommandMessageIDs.Disconnect;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public DisconnectMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    public partial class AcknowledgeMessage
    {
        public AcknowledgeMessage (ushort sequenceNumber)
        {
            header = new Header ();
            data   = new AckData ();

            header.MessageId = (ushort) ArduinoMessageIDs.AcknowledgeMsgId;
            header.ByteCount = (ushort) Marshal.SizeOf (this);

            data.MsgSequenceNumber = sequenceNumber;
        }

        public AcknowledgeMessage (byte[] fromBytes)
        {
            try
            {
                header = new Header (fromBytes);
                data   = new AckData ();

                int offset = (int) Marshal.OffsetOf<AcknowledgeMessage> ("data") + (int) Marshal.OffsetOf<AckData> ("MsgSequenceNumber");
                data.MsgSequenceNumber = BitConverter.ToUInt16  (fromBytes, offset);
            }

            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket 
        {
            byte [] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.MsgSequenceNumber));

            // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = header.ToString ();
            str += data.MsgSequenceNumber.ToString ();
            return str;
        }
    }

    //*******************************************************************************************************

    public partial class StatusMessage
    {        
        public string Name {get {return data.name.ToString ();} set {data.name = value.ToCharArray (0, StatusData.MaxNameLength);}}

        public StatusMessage ()
        {
            header = new Header ();
            data   = new StatusData ();

            header.MessageId = (ushort) ArduinoMessageIDs.StatusMsgId;

            header.ByteCount = (ushort) (Marshal.SizeOf (header) +
                                         StatusData.MaxNameLength * Marshal.SizeOf<char> ()  +
                                         Marshal.SizeOf (data.readyForMessages) + 
                                         Marshal.SizeOf (data.readyToRun) + 
                                         Marshal.SizeOf (data.motorsRunning) + 
                                         Marshal.SizeOf (data.readyToSend));
        }

        public StatusMessage (byte[] fromBytes) // for byte stream received from Arduino
        {
            try
            {
                header  = new Header (fromBytes);
                data = new StatusData ();

                int offset = (int) Marshal.OffsetOf<StatusMessage> ("data") + (int)Marshal.OffsetOf<StatusData> ("name");
                
                for (int i=0; i<StatusData.MaxNameLength; i++)
                {
                    if (fromBytes [offset + i] == 0)
                        break;

                    char c = (char) (fromBytes [offset + i]);

                    if (Char.IsLetterOrDigit (c))
                        data.name [i] = c;
                }

                offset = (int) Marshal.OffsetOf<StatusMessage> ("data") 
                       - 4 + StatusData.MaxNameLength * Marshal.SizeOf<char> () 
                       + (int) Marshal.OffsetOf<StatusData> ("readyForMessages");

                data.readyForMessages  = BitConverter.ToInt16  (fromBytes, offset);

                offset += Marshal.SizeOf<short> ();
                data.readyToRun = BitConverter.ToInt16  (fromBytes, offset);

                offset += Marshal.SizeOf<short> ();
                data.motorsRunning  = BitConverter.ToInt16  (fromBytes, offset);

                offset += Marshal.SizeOf<short> ();
                data.readyToSend  = BitConverter.ToInt16  (fromBytes, offset);
            }

            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket 
        {
            byte [] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();

            int length = Math.Min (StatusData.MaxNameLength, data.name.Length);

            for (int i=0; i<length; i++)
                dataBytes.Add ((byte) data.name [i]);

            int pad = StatusData.MaxNameLength - length;

            for (int i=0; i<pad; i++)
                dataBytes.Add (0);

            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.readyForMessages));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.readyToRun));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.motorsRunning));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.readyToSend));

            // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = header.ToString ()
                       + "Name = " + new string (data.name).TrimEnd ('\0') 
                       + ", Ready for messages = " + data.readyForMessages 
                       + ", Ready to run = "       + data.readyToRun 
                       + ", Running = "            + data.motorsRunning
                       + ", Ready To Send = "      + data.readyToSend;

            return str;
        }
    }

    //*******************************************************************************************************

    public partial class EncoderCountsMessage
    {
        public EncoderCountsMessage ()
        {
            header = new Header ();
            data   = new Batch ();

            header.MessageId = (ushort) ArduinoMessageIDs.EncoderCountsMsgId;

            header.ByteCount = (ushort) (Marshal.SizeOf (header) +
                                         Marshal.SizeOf (data.put) + Marshal.SizeOf (data.more) + Batch.MaxNumberSamples * Marshal.SizeOf<EncoderCountsMessage.Batch.Sample> ());
        }

        public EncoderCountsMessage (EncoderCountsMessage.Batch batch)
        {
            header = new Header ();
            data   = batch;

            header.MessageId = (ushort) ArduinoMessageIDs.EncoderCountsMsgId;

            header.ByteCount = (ushort) (Marshal.SizeOf (header) +
                                         Marshal.SizeOf (data.put) + Marshal.SizeOf (data.more) + Batch.MaxNumberSamples * Marshal.SizeOf<EncoderCountsMessage.Batch.Sample> ());
        }

        //*****************************************************************************

        public bool More    {get {return data.more != 0;} set {data.more = value == true ? (short) 1 : (short) 0;}}
        //public bool IsEmpty {get {return data.put == 0;}}
        //public bool IsFull  {get {return data.put == Batch.MaxNumberSamples;}}

        //*****************************************************************************

        public bool Add (byte e1, byte e2)
        {
            if (data.put < Batch.MaxNumberSamples)
            {
                data.counts [data.put].enc1 = e1;
                data.counts [data.put].enc2 = e2;
                data.put++;
            }

            return (data.put < Batch.MaxNumberSamples); // false when no more room
        }

        //*****************************************************************************

        public void Clear ()
        {
            data.put = 0;
        }

        //*****************************************************************************

        public EncoderCountsMessage (byte [] fromBytes)
        {
            header  = new Header (fromBytes);

            int dataOffset = (int)Marshal.OffsetOf<EncoderCountsMessage> ("data");

            data = new Batch ();
            data.put  = BitConverter.ToInt16 (fromBytes, dataOffset + (int)Marshal.OffsetOf<Batch> ("put"));
            data.more = BitConverter.ToInt16 (fromBytes, dataOffset + (int)Marshal.OffsetOf<Batch> ("more"));

            int firstRecordStart = (int)Marshal.OffsetOf<Batch> ("counts");

            int recordSize = (int)Marshal.SizeOf<EncoderCountsMessage.Batch.Sample> ();

            for (int i = 0; i<Batch.MaxNumberSamples; i++)
            {
                int thisRecordStart = dataOffset + firstRecordStart + i * recordSize;

                data.counts [i].enc1 = fromBytes [thisRecordStart + (int)Marshal.OffsetOf<EncoderCountsMessage.Batch.Sample> ("enc1")];
                data.counts [i].enc2 = fromBytes [thisRecordStart + (int)Marshal.OffsetOf<EncoderCountsMessage.Batch.Sample> ("enc2")];
            }
        }

        public byte [] ToBytes () // convert to byte stream to be sent out socket
        {
            byte [] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.put));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.more));

            for (int i = 0; i<Batch.MaxNumberSamples; i++)
            {
                dataBytes.Add (data.counts [i].enc1);
                dataBytes.Add (data.counts [i].enc2);
            }

            // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = header.ToString ();
            str += "put = " + data.put + '\n';
            str += "more = " + data.more + '\n';

            for (int i = 0; i<Batch.MaxNumberSamples; i++)
            {
                str += string.Format ("{0},  ", data.counts [i].enc1);
                str += string.Format ("{0},  ", data.counts [i].enc2);

                if (i<Batch.MaxNumberSamples - 1)
                    str += "\n";
            }

            return str;
        }
    }

    //*******************************************************************************************************

    public partial class CollSendCompleteMessage
    {
        public CollSendCompleteMessage ()
        {
            MessageId = (ushort)ArduinoMessageIDs.EncoderCountsCompleteMsgId;
            ByteCount = (ushort)Marshal.SizeOf<Header> ();
        }

        public CollSendCompleteMessage (byte [] fromBytes)
        {
            Header temp = new Header (fromBytes);
            MessageId = temp.MessageId;
            ByteCount = temp.ByteCount;
        }

        public new byte [] ToBytes ()
        {
            byte [] msgBytes = base.ToBytes ();
            return msgBytes;
        }
    }

    //*******************************************************************************************************

    public partial class TextMessage
    {
        public TextMessage (string txt)
        {
            header = new Header ();
            header.ByteCount = (ushort)(Marshal.SizeOf<Header> () + txt.Length);
            header.MessageId = (ushort)ArduinoMessageIDs.TextMsgId;
            text = txt.ToCharArray ();
        }

        //********************************************************************************

        public TextMessage (byte [] fromBytes) // for byte stream received from Arduino
        {
            header  = new Header (fromBytes);

            int dataOffset = (int)Marshal.OffsetOf<TextMessage> ("text");
            int charCount = fromBytes.Length - dataOffset;

            text = new char [charCount];

            for (int i = 0; i<charCount; i++)
                text [i] = (char)fromBytes [dataOffset + i];
        }

        //********************************************************************************

        public byte [] ToBytes () // convert to byte stream to be sent out socket
        {
            byte [] headerBytes = header.ToBytes ();
            byte [] textBytes = System.Text.Encoding.GetEncoding ("UTF-8").GetBytes (text);

            // append data bytes to header bytes
            byte [] msgBytes = new byte [headerBytes.Length];

            headerBytes.CopyTo (msgBytes, 0);
            textBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }
    }
}
