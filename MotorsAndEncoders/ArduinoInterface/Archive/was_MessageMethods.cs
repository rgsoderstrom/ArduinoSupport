using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; // for StructLayout

using SocketLib;

namespace ArduinoInterface
{
 //****************************************************************************************************************************

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

    public partial class MotorSpeedMsg
    {
        public MotorSpeedMsg (int v1, int v2) //, int s1, int s2, int d1, int d2)
        {
            header = new Header ();
            header.MessageId = (ushort)CommandMessageIDs.MotorSpeed;
            header.ByteCount = (ushort) Marshal.SizeOf (this);
            Velocity1 = (short) v1;
            Velocity2 = (short) v2;
            //Start1Time = (uint) s1;
            //Start2Time = (uint) s2;
            //Duration1 =  (uint) d1;
            //Duration2 =  (uint) d2;
        }

        public MotorSpeedMsg (byte[] fromBytes) //, PrintCallback print) // convert received byte stream to message
        {
            header  = new Header (fromBytes);

            Velocity1  = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Velocity1"));
            Velocity2  = BitConverter.ToInt16  (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Velocity2"));
            //Start1Time = BitConverter.ToUInt32 (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Start1Time"));
            //Start2Time = BitConverter.ToUInt32 (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Start2Time"));
            //Duration1  = BitConverter.ToUInt32 (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Duration1"));
            //Duration2  = BitConverter.ToUInt32 (fromBytes, (int) Marshal.OffsetOf<MotorSpeedMsg> ("Duration2"));
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Velocity1));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Velocity2));

            //dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Start1Time));
            //dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Start2Time));

            //dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Duration1));
            //dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Duration2));

          // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = "";
            str += string.Format ("Velocity1  = {0}, Velocity2  = {1}\n", Velocity1,  Velocity2);
            //str += string.Format ("Start1Time = {0}, Start2Time = {1}\n", Start1Time, Start2Time);
            //str += string.Format ("Duration1  = {0}, Duration2  = {1}\n", Duration1,  Duration2);
            return str;
        }

    }

 //****************************************************************************************************************************

    public partial class StartCollectionMsg
    {
        public StartCollectionMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.StartCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    public partial class StopCollectionMsg
    {
        public StopCollectionMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.StopCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    public partial class SendFirstCollectionMsg
    {
        public SendFirstCollectionMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.SendFirstCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    public partial class SendNextCollectionMsg
    {
        public SendNextCollectionMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.SendNextCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    public partial class ClearCollectionMsg
    {
        public ClearCollectionMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.ClearCollection;
            ByteCount = (ushort) Marshal.SizeOf (this);
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
    }

 //****************************************************************************************************************************
 //****************************************************************************************************************************
 //****************************************************************************************************************************

    public partial class ClearProfileMsg
    {
        public ClearProfileMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.ClearProfile;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

 //****************************************************************************************************************************

    public partial class ProfileSectionMsg
    {
        public ProfileSectionMsg (int indexOfFirst, List<double> leftSpeeds, List<double> rightSpeeds)
        {
            if (leftSpeeds.Count != rightSpeeds.Count) throw new Exception ("ProfileSectionMsg ctor: left and right profile data must be same length");

            header = new Header ();
            data = new ProfileSection ();

            header.MessageId = (ushort) CommandMessageIDs.ProfileSection;
            header.ByteCount = (ushort) (Marshal.SizeOf (header)
                                       + Marshal.SizeOf (data.index)
                                       + Marshal.SizeOf (data.numberValues)
                                       + 2 * ProfileSection.MaxNumberValues * Marshal.SizeOf (data.LeftSpeed [0]));

            data.index = (short) indexOfFirst;
            data.numberValues = (short) Math.Min (ProfileSection.MaxNumberValues, leftSpeeds.Count);

            for (int i=0; i<data.numberValues; i++)
            {
                data.LeftSpeed  [i] = (short) leftSpeeds [i];
                data.RightSpeed [i] = (short) rightSpeeds [i];
            }

            for (int i=data.numberValues; i<ProfileSection.MaxNumberValues; i++) // zero any unused entries
                data.LeftSpeed [i] = data.RightSpeed [i] = 0;
        }

        public ProfileSectionMsg (byte[] fromBytes) // convert received byte stream to message
        {
            try
            {
                header  = new Header (fromBytes);
                data = new ProfileSection ();

                int offset = (int) Marshal.OffsetOf<ProfileSectionMsg> ("data") + (int) Marshal.OffsetOf<ProfileSection> ("index");
                data.index        = BitConverter.ToInt16  (fromBytes, offset);

                offset = (int) Marshal.OffsetOf<ProfileSectionMsg> ("data") + (int) Marshal.OffsetOf<ProfileSection> ("numberValues");
                data.numberValues = BitConverter.ToInt16  (fromBytes, offset);

                for (int i=0; i<data.numberValues; i++)
                {
                    offset = (int) Marshal.OffsetOf<ProfileSectionMsg> ("data") + (int) Marshal.OffsetOf<ProfileSection> ("LeftSpeed") + i * sizeof (short);
                    data.LeftSpeed [i] = BitConverter.ToInt16  (fromBytes, offset);
                }

                for (int i=0; i<data.numberValues; i++)
                {
                    offset = (int) Marshal.OffsetOf<ProfileSectionMsg> ("data") + (int) Marshal.OffsetOf<ProfileSection> ("LeftSpeed") + (data.numberValues + i) * sizeof (short);
                    data.RightSpeed [i] = BitConverter.ToInt16  (fromBytes, offset);
                }
            }

            catch (Exception ex)
            {
                string str = ex.Message;
            }

        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.index));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.numberValues));

            for (int i=0; i<data.numberValues; i++)
                dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.LeftSpeed [i]));

            for (int i=0; i<data.numberValues; i++)
                dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.RightSpeed [i]));

          // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));
            
            return msgBytes;
        }
    }

    //*******************************************************************************************************

    public partial class RunProfileMsg
    {
        public RunProfileMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.RunProfile;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    public partial class StopRunMsg
    {
        public StopRunMsg () : base ()
        {
            MessageId = (ushort) CommandMessageIDs.StopRun;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }
        
    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    //*******************************************************************************************************
    //*******************************************************************************************************
    //*******************************************************************************************************

    public partial class TextMessage
    {
        public TextMessage (string txt)
        {
            header = new Header ();
            header.ByteCount = (ushort) (Marshal.SizeOf<Header> () + txt.Length);
            header.MessageId = (ushort)ArduinoMessageIDs.TextMsgId;
            text = txt.ToCharArray ();
        }

        //********************************************************************************

        public TextMessage (byte[] fromBytes) // for byte stream received from Arduino
        {
            header  = new Header (fromBytes);

            int dataOffset = (int)Marshal.OffsetOf<TextMessage> ("text");
            int charCount = fromBytes.Length - dataOffset;

            text = new char [charCount];

            for (int i = 0; i<charCount; i++)
                text [i] = (char) fromBytes [dataOffset + i];
        }

        //********************************************************************************

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] headerBytes = header.ToBytes ();
            byte[] textBytes   = System.Text.Encoding.GetEncoding ("UTF-8").GetBytes (text);

            // append data bytes to header bytes
            byte [] msgBytes = new byte [headerBytes.Length];

            headerBytes.CopyTo (msgBytes, 0);
            textBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }
    }

    //*******************************************************************************************************
    //*******************************************************************************************************
    //*******************************************************************************************************

    public partial class BufferStatusMessage
    {
        public BufferStatusMessage (int d)
        {
            header = new Header ();
            header.ByteCount = (ushort) Marshal.SizeOf<BufferStatusMessage> ();
            header.MessageId = (ushort) ArduinoMessageIDs.BufferStatusMsgId;
            data = (byte) d;
        }

        //********************************************************************************

        public BufferStatusMessage (byte [] fromBytes) // for byte stream received from Arduino
        {
            header  = new Header (fromBytes);
            int dataOffset = (int)Marshal.OffsetOf<BufferStatusMessage> ("data");
            data = fromBytes [dataOffset];
        }

        //********************************************************************************

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] headerBytes = header.ToBytes ();

            // append data bytes to header bytes
            byte [] msgBytes = new byte [headerBytes.Length];

            headerBytes.CopyTo (msgBytes, 0);
            msgBytes [Marshal.SizeOf (header)] = data;

            return msgBytes;
        }
    }

    //*******************************************************************************************************
    //*******************************************************************************************************
    //*******************************************************************************************************

    public delegate void PrintCallback (string str);

    public partial class CollectionData
    {
        public CollectionData () 
        {
            put = 0;
        }
    }

    public partial class CollectionDataMessage
    {
        public CollectionDataMessage ()
        {
            header = new Header ();
            data   = new CollectionData ();

            header.MessageId = (ushort) ArduinoMessageIDs.CollectedDataMsgId;
            ushort s1 = (ushort) Marshal.SizeOf (header);            
            ushort s2 = (ushort)(Marshal.SizeOf (data.put) + CollectionData.MsgBufferSize * Marshal.SizeOf<EncoderCounts> ());
            header.ByteCount = (ushort) (s1 + s2);
        }

        //*****************************************************************************

        public bool Add (uint tt, short e1, short e2, byte s1, byte s2)
        {
            if (data.put < CollectionData.MsgBufferSize)
            {
                data.counts [data.put].time = tt;
                data.counts [data.put].enc1 = e1;
                data.counts [data.put].enc2 = e2;
                data.counts [data.put].s1 = s1;
                data.counts [data.put].s2 = s2;
                data.put++;
            }

            return (data.put < CollectionData.MsgBufferSize); // false when no more room
        }

        public bool Add (EncoderCounts ec)
        {
            if (data.put < CollectionData.MsgBufferSize)
            {
                data.counts [data.put] = ec;
                data.put++;
            }

            return (data.put < CollectionData.MsgBufferSize); // false when no more room
        }

        //*****************************************************************************

        public void Clear ()
        {
            data.put = 0;
        }

        //*****************************************************************************

    //    public CollectionDataMessage (byte[] fromBytes, PrintCallback print) // for byte stream received from Arduino
    //    {
    //        header  = new Header (fromBytes);

    //        try
    //        {
    //            int dataOffset = (int)Marshal.OffsetOf<CollectionDataMessage> ("data");

    //            data = new CollectionData ();
    //            data.put = BitConverter.ToInt16  (fromBytes, dataOffset + (int) Marshal.OffsetOf<CollectionData> ("put"));

    //            int firstRecordStart = (int)Marshal.OffsetOf<CollectionData> ("counts");

    //            int recordSize = (int)Marshal.SizeOf<EncoderCounts> ();

    //            for (int i = 0; i<CollectionData.MsgBufferSize; i++)
    //            {
    //                int thisRecordStart = dataOffset + firstRecordStart + i * recordSize;

    //                data.counts [i].time = BitConverter.ToUInt32 (fromBytes, thisRecordStart + (int)Marshal.OffsetOf<EncoderCounts> ("time"));
    //                data.counts [i].enc1 = BitConverter.ToInt16  (fromBytes, thisRecordStart + (int)Marshal.OffsetOf<EncoderCounts> ("enc1"));
    //                data.counts [i].enc2 = BitConverter.ToInt16  (fromBytes, thisRecordStart + (int)Marshal.OffsetOf<EncoderCounts> ("enc2"));
    //                data.counts [i].s1   = fromBytes [thisRecordStart + (int)Marshal.OffsetOf<EncoderCounts> ("s1")];
    //                data.counts [i].s2   = fromBytes [thisRecordStart + (int)Marshal.OffsetOf<EncoderCounts> ("s2")];
    //            }
    //        }

    //        catch (Exception ex)
    //        {
    //            print (string.Format ("Exception in EncoderCountsMessage ctor: {0}", ex.Message));
    //        }
    //    }

    //    public byte[] ToBytes () // convert to byte stream to be sent out socket
    //    {
    //        byte[] msgBytes = header.ToBytes ();

    //        List<byte> dataBytes = new List<byte> ();
    //        dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.put));

    //        for (int i = 0; i<CollectionData.MsgBufferSize; i++)
    //        {
    //            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.counts [i].time));
    //            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.counts [i].enc1));
    //            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (data.counts [i].enc2));
    //            dataBytes.Add (data.counts [i].s1);
    //            dataBytes.Add (data.counts [i].s2);
    //        }

    //      // append data bytes to header bytes
    //        dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

    //        return msgBytes;
    //    }

    //    public override string ToString ()
    //    {
    //        string str = ""; //  header.ToString ();

    //        for (int i = 0; i<CollectionData.MsgBufferSize; i++)
    //        {
    //            str += string.Format ("{0},  ",   data.counts [i].time);
    //            str += string.Format ("{0},  ",   data.counts [i].enc1);
    //            str += string.Format ("{0},  ",   data.counts [i].enc2);
    //            str += string.Format ("{0},  ",   data.counts [i].s1);
    //            str += string.Format ("{0};",     data.counts [i].s2);

    //            if (i<CollectionData.MsgBufferSize - 1)
    //                str += "\n";

    //            //str += string.Format ("Time, milliseconds = {0}\n", data.counts [i].time);
    //            //str += string.Format ("enc1 = {0}\n", data.counts [i].enc1);
    //            //str += string.Format ("enc2 = {0}\n", data.counts [i].enc2);
    //            //str += string.Format ("s11 =  {0}\n", data.counts [i].s1);
    //            //str += string.Format ("s22 =  {0}\n", data.counts [i].s2);
    //        }

    //        return str;
    //    }
    //}

    //*******************************************************************************************************
    //*******************************************************************************************************
    //*******************************************************************************************************

    //public partial class CollSendCompleteMessage
    //{
    //    public CollSendCompleteMessage ()
    //    {
    //        MessageId = (ushort) ArduinoMessageIDs.CollSendCompleteMsgId;
    //        ByteCount = (ushort) Marshal.SizeOf<Header> ();
    //    }

    //    public CollSendCompleteMessage (byte[] fromBytes)
    //    {
    //        Header temp = new Header (fromBytes);
    //        MessageId = temp.MessageId;
    //        ByteCount = temp.ByteCount;
    //    }

    //    public new byte[] ToBytes ()
    //    {
    //        byte[] msgBytes = base.ToBytes ();
    //        return msgBytes;
    //    }
    //}

    //*******************************************************************************************************
    //*******************************************************************************************************
    //*******************************************************************************************************

    public partial class ProfileSectionRcvdMessage
    {
        public ProfileSectionRcvdMessage ()
        {
            MessageId = (ushort) ArduinoMessageIDs.ProfileSectionRcvdMsgId;
            ByteCount = (ushort) Marshal.SizeOf<Header> ();
        }

        public ProfileSectionRcvdMessage (byte[] fromBytes)
        {
            Header temp = new Header (fromBytes);
            MessageId = temp.MessageId;
            ByteCount = temp.ByteCount;
        }

        public new byte[] ToBytes ()
        {
            byte[] msgBytes = base.ToBytes ();
            return msgBytes;
        }
    }
}
