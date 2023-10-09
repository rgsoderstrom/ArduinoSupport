using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class StatusMessage : StatusMsg_Auto
    {
        public StatusMessage () : base ()
        {
            data.name [0] = '0';
            data.DataReceived = 0;
            data.DataReady = 0;
        }

        public StatusMessage (byte [] fromBytes) : base (fromBytes)
        {
        }

        public string Name 
        {
            get {return new string (data.name);}

            set
            {
                if (value.Length > Data.MaxNameLength - 1) 
                    value = value.Remove (Data.MaxNameLength - 1); // remove characters if it's too long

                data.name = value.PadRight (Data.MaxNameLength, '\0').ToCharArray (); // pad to right length if it's short
            }
        }

        public bool DataReceived {get {return data.DataReceived == 1;}
                                  set {data.DataReceived = (byte) (value == true ? 1 : 0);}}

        public bool DataReady {get {return data.DataReady == 1;}
                               set {data.DataReady = (byte) (value == true ? 1 : 0);}}
    }
}
