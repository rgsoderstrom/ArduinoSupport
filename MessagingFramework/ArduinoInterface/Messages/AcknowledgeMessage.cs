
using System.Runtime.InteropServices;

namespace ArduinoInterface
{
    public class AcknowledgeMessage : AcknowledgeMsg_Auto
    {
        public AcknowledgeMessage (ushort seqNumber) : base ()
        {
            data.MsgSequenceNumber = seqNumber;
        }

        public AcknowledgeMessage (byte [] fromBytes) : base (fromBytes)
        {
        }
    }
}
