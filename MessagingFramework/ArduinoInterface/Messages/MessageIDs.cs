

namespace ArduinoInterface
{
    public enum PCMessageIDs : ushort
    {
        KeepAlive     = 1,  // no action required, just keeps socket open
        StatusRequest = 2,
        Disconnect    = 3,

        LoopbackDataMsgId = 9,
        RunLoopbackTestMsgId = 10,
        SendLoopbackDataMsgId = 11,
    };

    public enum ArduinoMessageIDs : ushort
    {
        AcknowledgeMsgId  = 1,
        TextMsgId   = 2,
        StatusMsgId = 3,
        LoopbackDataMsgId = 9,
    };
}
