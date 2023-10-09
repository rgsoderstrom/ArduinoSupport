using System;
using System.Runtime.InteropServices;

//
// hand-written code for LoopbackDataMsg
//  - this file only needed if data members are added to the base class
//

using SocketLibrary;
namespace ArduinoInterface
{
    public partial class LoopbackDataMsg : LoopbackDataMsg_Auto
    {
        public LoopbackDataMsg () : base ()
        {
        }

        public LoopbackDataMsg (byte [] fromBytes) : base (fromBytes)
        {
        }

        public int Get (int i) { return data.dataWords [i]; }
        public int Count {get { return 12; } }
    }
}
