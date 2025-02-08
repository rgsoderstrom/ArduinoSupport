
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

        StartSamplingMsgId = 20, // PC -> Arduino
        SendSamplesMsgId   = 21,

        DoneSamplingMsgId  = 30, // Arduino -> PC 
        SensorDataMsgId    = 31,
        ReadyMsgId         = 32,
    };
}
