using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    public class Cs_Code
    {
        //
        // Methods needed by both "format" and "methods" file generators
        //
        static internal void OpenNamespace (List<string> code, string namespaceName, string msgName)
        {
            code.Add ("//");
            code.Add ("// auto-generated code for message " + msgName);
            code.Add ("//");
            code.Add ("");
            code.Add ("using System;");
            code.Add ("using System.Text;");
            code.Add ("using System.Collections.Generic;");
            code.Add ("using System.Runtime.InteropServices;");
            code.Add ("using SocketLibrary;");
            code.Add ("");
            code.Add ("namespace " + namespaceName);
            code.Add ("{");
        }

        static internal void OpenClass (List<string> code, string msgName)
        {
            code.Add ("    public partial class " + msgName + " : IMessage_Auto");
            code.Add ("    {");

            code.Add ("        public ushort Sync           {get {return header.Sync;}}");
            code.Add ("        public ushort ByteCount      {get {return header.ByteCount;}}");
            code.Add ("        public ushort MessageId      {get {return header.MessageId;}}");
            code.Add ("        public ushort SequenceNumber {get {return header.SequenceNumber;}}");
            code.Add ("");
        }
    }
}
