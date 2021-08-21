using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; // for StructLayout

using SocketLib;

namespace ArduinoInterface
{
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

    public partial class SendStatusCmdMsg
    {
        public SendStatusCmdMsg ()
        {     
            MessageId = (ushort) CommandMessageIDs.SendStatus;
            ByteCount = (ushort) Marshal.SizeOf (this);  
        }

        public SendStatusCmdMsg (byte[] fromBytes) : base (fromBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class StartSamplingCmdMsg
    {
        public StartSamplingCmdMsg () 
        {
            header = new Header ();
            header.MessageId = (ushort) CommandMessageIDs.StartSampling;
            header.ByteCount = (ushort) Marshal.SizeOf (this);

            period = 1;
        }

        public StartSamplingCmdMsg (byte [] fromBytes) // construct from byte stream received over socket
        {
            header = new Header (fromBytes);
            period = BitConverter.ToUInt16 (fromBytes, (int)Marshal.OffsetOf<StartSamplingCmdMsg> ("period"));
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (period));

          // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = "";
            str += header.ToString ();
            str += string.Format ("Period: {0}\n", period);

            return str;
        }
    }

 //****************************************************************************************************************************

    public partial class StopSamplingCmdMsg
    {
        public StopSamplingCmdMsg () 
        {
            MessageId = (ushort) CommandMessageIDs.StopSampling;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }

        public StopSamplingCmdMsg (byte[] msgBytes) : base (msgBytes)
        {
        }
    }

 //****************************************************************************************************************************

    public partial class ClearHistoryCmdMsg
    {
        public ClearHistoryCmdMsg () 
        {
            MessageId = (ushort) CommandMessageIDs.ClearHistory;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

 //****************************************************************************************************************************

    public partial class SendHistoryCmdMsg
    {
        public SendHistoryCmdMsg () 
        {
            MessageId = (ushort) CommandMessageIDs.SendHistory;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

 //****************************************************************************************************************************

    /***
    public partial class SendContinuouslyCmdMsg
    {
        public SendContinuouslyCmdMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.SendContinuously;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }
    ***/

 //****************************************************************************************************************************

    /***
    public partial class StopSendingCmdMsg
    {
        public StopSendingCmdMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.StopSending;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }
    ***/

 //****************************************************************************************************************************

    public partial class DisconnectCmdMsg
    {
        public DisconnectCmdMsg ()
        {
            MessageId = (ushort) CommandMessageIDs.Disconnect;
            ByteCount = (ushort) Marshal.SizeOf (this);
        }
    }

    //*******************************************************************************************************
    // Arduino -> PC messages *******************************************************************************
    //*******************************************************************************************************

    public partial class StatusMessage
    {
        public StatusMessage () // used by test driver
        {
            header = new Header ();
            header.ByteCount = (ushort) Marshal.SizeOf (this);
            header.MessageId = (ushort) ArduinoMessageIDs.StatusMsgId;
        }

        public StatusMessage (byte[] fromBytes) // for byte stream received from Arduino
        {
            header              = new Header (fromBytes);
            sampling            = (byte) BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<StatusMessage> ("sampling"));
            numberRamSamples    = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<StatusMessage> ("numberRamSamples"));
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[] msgBytes = header.ToBytes ();

            List<byte> dataBytes = new List<byte> ();

            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (sampling));
            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (numberRamSamples));

          // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = header.ToString ();

            str += String.Format ("sampling              = {0}{1}", sampling, Environment.NewLine);
            str += String.Format ("Number RAM Samples    = {0}{1}", numberRamSamples, Environment.NewLine);

            return str;
        }
    }
    
    //*******************************************************************************************************

    public partial class TemperatureMessage
    {
        static readonly public int MaxNumberSamples = 10;
        public ushort NumberSamples {get {return numberSamples;}}
        public bool   Full  {get {return numberSamples == MaxNumberSamples;}}
        public bool   Empty {get {return numberSamples == 0;}}

        public TemperatureMessage ()
        {
            header = new Header ();
            header.MessageId = (ushort) ArduinoMessageIDs.TemperatureMsgId;

            header.ByteCount = (ushort)(Marshal.SizeOf (header) 
                                      + Marshal.SizeOf (numberSamples) 
                                      + MaxNumberSamples * Marshal.SizeOf (typeof (TemperatureSample)));
            numberSamples = 0;
            Samples = new TemperatureSample [MaxNumberSamples];
        }

        public void Clear ()
        {
            numberSamples = 0;
        }

        public bool Add (TemperatureSample sam)
        {
            if (Full == false)
            {
                Samples [numberSamples++] = sam;
                return true;
            }

            return false;
        }
            
        /**
        public TemperatureSample GetSample (int i)
        {
            if (i < 0 || i > NumberSamples)
                throw new IndexOutOfRangeException ();

            TemperatureSample Copy = new TemperatureSample ();

            Copy.time        = Samples [i].time;
            Copy.temperature = Samples [i].temperature;

            return Copy;
        }
        **/

        public TemperatureMessage (byte[] fromBytes) // for byte stream received from Arduino
        {
            header  = new Header (fromBytes);
            Samples = new TemperatureSample [MaxNumberSamples];

            numberSamples = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<TemperatureMessage> ("numberSamples"));

            int sampleOffset = (int) Marshal.OffsetOf<TemperatureMessage> ("Samples");

            for (int i = 0; i<numberSamples; i++)
            {
                Samples [i].time        = BitConverter.ToUInt32 (fromBytes, sampleOffset + (int) Marshal.OffsetOf<TemperatureSample> ("time"));
                Samples [i].temperature = BitConverter.ToSingle (fromBytes, sampleOffset + (int) Marshal.OffsetOf<TemperatureSample> ("temperature"));
                sampleOffset += Marshal.SizeOf<TemperatureSample> (); // or += Marshal.Sizeof (Typeof (Sample));
            }
        }
      
        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            byte[]     msgBytes  = header.ToBytes ();
            List<byte> dataBytes = new List<byte> ();

            dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (numberSamples));

            for (int i=0; i<MaxNumberSamples; i++)
            {
                dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Samples [i].time));
                dataBytes.InsertRange (dataBytes.Count, BitConverter.GetBytes (Samples [i].temperature));
            }

            // append data bytes to header bytes
            dataBytes.CopyTo (msgBytes, Marshal.SizeOf (header));

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = header.ToString ();

            str += string.Format ("Numb Samples = {0}\n", numberSamples);

            for (int i=0; i<numberSamples; i++)
            {
                str += string.Format ("{0} : ", Samples [i].time);                
                str += string.Format ("{0}\n", Samples [i].temperature);                
            }

            return str;
        }
    }
}
