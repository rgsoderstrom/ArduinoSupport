

namespace ArduinoInterface
{
    public enum PCMessageIDs : ushort
    {
        KeepAlive     = 1,  // no action required, just keeps socket open
        StatusRequest = 2,
        Disconnect    = 3,

        LoopbackDataMsgId = 100,
        RunLoopbackTestMsgId = 101,
        SendLoopbackDataMsgId = 102,
    };

    public enum ArduinoMessageIDs : ushort
    {
        AcknowledgeMsgId  = 1,
        TextMsgId   = 2,
        StatusMsgId = 3,
        LoopbackDataMsgId = 100,
    };
}
