
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

        //********************************************************

        //
        // Common to all
        //

        // FPGA->Arduino->PC  
		ReadyMsgId = 100, 

        // PC->Arduino->FPGA
		ClearSamplesMsgId = 150,
		SendSamplesMsgId  = 151,

        //********************************************************

        //
        // A2D_Test (i.e. Merc2ADC_Test3)
		//

        // FPGA->Arduino->PC  
        SampleDataMsgId = 201,

        // PC->Arduino->FPGA
		BeginSamplingMsgId = 251,
        AnalogGainMsgId    = 252,
        SampleRateMsgId    = 253,

        //********************************************************

        //
        // Sonar1Chan
        //

        // FPGA->Arduino->PC  
		PingReturnDataMsgId = 301,
        PingReturnMfDataMsgId = 302,

        // PC->Arduino->FPGA
		BeginPingCycleMsgId  = 351,
        SonarParametersMsgId = 352,
    };
}
