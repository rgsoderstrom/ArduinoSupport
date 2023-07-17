using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class StatusMessage
    {
        public class Data
        {
            public const int MaxNameLength = 18;

            public char [] name = new char [MaxNameLength];
            public byte DataReceived;
            public byte DataReady;
        }

        public Header header;
        public Data data;

        public StatusMessage ()
        {
            header = new Header ();
            data = new Data ();

            header.MessageId = (ushort) ArduinoMessageIDs.StatusMsgId;
            header.ByteCount = (ushort)(Marshal.SizeOf<Header> () + Data.MaxNameLength + 2); // Marshal.SizeOf<Data> ());

            data.name [0] = '0';
            data.DataReceived = 0;
            data.DataReady = 0;
        }

        public string Name 
        {
            get {return new string (data.name);}

            set
            {
                if (value.Length > Data.MaxNameLength - 1) 
                    value = value.Remove (Data.MaxNameLength - 1); // remove characters if it's too long

                data.name = value.PadRight (Data.MaxNameLength).ToCharArray (); // pad to right length if it's short
            }

        }

        public bool DataReceived {get {return data.DataReceived == 1 ? true : false;}
                                  set {data.DataReceived = (byte) (value == true ? 1 : 0);}}

        public bool DataReady {get {return data.DataReady == 1 ? true : false;}
                               set {data.DataReady = (byte) (value == true ? 1 : 0);}}

        public override string ToString ()
        {
            string str = header.ToString ();
            str += "Name: " + Name + "\n";
            str += "DataReceived: " + DataReceived.ToString () + "\n";
            str += "DataReady: " + DataReady.ToString () + "\n";
            return str;
        }
    }
}
