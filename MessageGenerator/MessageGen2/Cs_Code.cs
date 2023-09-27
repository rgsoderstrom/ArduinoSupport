using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            code.Add ("using SocketLib;");
            code.Add ("");
            code.Add ("namespace " + namespaceName);
            code.Add ("{");
        }

        static internal void OpenClass (List<string> code, string msgName)
        {
            code.Add ("    public partial class " + msgName);
            code.Add ("    {");
        }
    }
}
