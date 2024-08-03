using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class Cs_InterfaceFile
    {
        public Cs_InterfaceFile (StreamWriter sw)
        {
            sw.WriteLine ("//");
            sw.WriteLine ("//auto-generated interface");
            sw.WriteLine ("//");
            sw.WriteLine ("");
            sw.WriteLine ("namespace ArduinoInterface");
            sw.WriteLine ("{");
            sw.WriteLine ("    public interface IMessage_Auto");
            sw.WriteLine ("    {");
            sw.WriteLine ("        byte[] ToBytes ();");
            sw.WriteLine ("");
            sw.WriteLine ("        ushort Sync           {get;}");
            sw.WriteLine ("        ushort ByteCount      {get;}");
            sw.WriteLine ("        ushort MessageId      {get;}");
            sw.WriteLine ("        ushort SequenceNumber {get;}");
            sw.WriteLine ("    }");
            sw.WriteLine ("}");
        }
    }
}
