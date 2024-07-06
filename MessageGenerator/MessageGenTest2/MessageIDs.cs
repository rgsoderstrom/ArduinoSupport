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

        TextMsgId = 8,
        LoopbackDataMsgId = 9,
        AcknowledgeMsgId = 10,

        HeaderOnlyMsgId = 0,  // always overwritten
        SampleDataMsgId = 99,
        AllSentMsgId    = 100
    };
}
