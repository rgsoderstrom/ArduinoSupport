using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLib;

namespace MessageGenerator
{
    internal class Cpp_Code
    {
        internal Cpp_Code (StreamWriter sw, string messageName, List<string []> dataMemberTokens)
        {
            try
            {
                // file header comment and standard includes
                CppBeginCodeFile (sw, messageName);

                // default constructor 
                CppDefaultConstructor (sw, messageName);

                // from-bytes ctor
                Cpp_FromBytes fromBytesCode = new Cpp_FromBytes (sw, messageName, dataMemberTokens);

                // ToBytes ()
                Cpp_ToBytes toBytesCode = new Cpp_ToBytes (sw, messageName, dataMemberTokens);

                // ToConsole ()
                Cpp_ToConsole toConsoleCode = new Cpp_ToConsole (sw, messageName, dataMemberTokens);
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception generating C++ code: " + ex.Message);
            }
        }

        //
        // Methods to write C++ file
        //
        static void CppBeginCodeFile (StreamWriter sw, string msgName)
        {
            sw.WriteLine ("//");
            sw.WriteLine ("// auto-generated code for message " + msgName);
            sw.WriteLine ("//");
            sw.WriteLine ("");
            sw.WriteLine ("#include <Arduino.h>");
            sw.WriteLine ("#include \"" + msgName + ".h\"");
        }

        static void CppDefaultConstructor (StreamWriter sw, string msgName)
        {
            sw.WriteLine ("");
            sw.WriteLine ("//");
            sw.WriteLine ("// Default constructor");
            sw.WriteLine ("//");
            sw.WriteLine (msgName + "::" + msgName + " ()");
            sw.WriteLine ("{");
            sw.WriteLine ("    memset (this, 0, sizeof (" + msgName + "));");
            sw.WriteLine ("");
            sw.WriteLine ("    header.Sync           = " + SocketLib.Message.Sync + ";");
            sw.WriteLine ("    header.ByteCount      = sizeof (header) + sizeof (data);");
            sw.WriteLine ("    header.MsgId          = " + msgName + "ID;");
            sw.WriteLine ("    header.SequenceNumber = NextSequenceNumber++;");
            sw.WriteLine ("}");
        }
    }
}
