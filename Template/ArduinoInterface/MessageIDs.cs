
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
        //
        // PC<->Arduino
        //
        KeepAliveMsgId   = 1,  // no action required, just keeps socket open
        TextMsgId        = 8,
        AcknowledgeMsgId = 10,
		ReadyMsgId       = 11, 

        Button1MsgId = 20,
        Button2MsgId = 21,
        Button3MsgId = 22,


    };
}
