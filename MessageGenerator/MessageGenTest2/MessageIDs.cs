using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoInterface
{
    public enum ArduinoMessageIDs : ushort
    {
        KeepAlive     = 1,  // no action required, just keeps socket open
        StatusRequest = 2,
        StatusMsgId     = 4,

        LoopbackData = 9,
        Acknowledge = 10,
    };
}
