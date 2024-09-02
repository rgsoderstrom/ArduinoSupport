
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
        KeepAliveMsgId   = 1,  // no action required, just keeps socket open
        TextMsgId        = 8,
        AcknowledgeMsgId = 10,

		ClearMsgId   = 100, // PC->Arduino->FPGA
		CollectMsgId = 101,
		SendMsgId    = 102,

        AnalogGainMsgId = 103,
        SampleRateMsgId = 104,
    
		SampleDataMsgId = 200, // FPGA->Arduino->PC  
		ReadyMsgId      = 201,
		AllSentMsgId    = 202, 

        HeaderOnlyMsgId  = 999,
    };
}
