
//
// CppCpp_ToConsole - C++ code in and C++ code out
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    partial class MessageCodeGenerator
    {
        //
        // Dictionary of methods to handle the different types
        //
        static Dictionary<string, VariableTypeToFromBytes> CppToConsoleRules = new Dictionary<string, VariableTypeToFromBytes> ()
        {
            {"char",           VarToConsole},
            {"unsigned char",  VarToConsole},
            {"byte",           VarToConsole},
            {"int",            VarToConsole},
            {"unsigned int",   VarToConsole},
            {"short",          VarToConsole},
            {"unsigned short", VarToConsole},
            {"float",          VarToConsole},
            //{"Sample", SampleToConsole},
        };

        static Dictionary<string, VariableTypeArrayToFromBytes> CppArrayToConsoleRules = new Dictionary<string, VariableTypeArrayToFromBytes> ()
        {
            {"char",           VarArrayToConsole},
            {"unsigned char",  VarArrayToConsole},
            {"byte",           VarArrayToConsole},
            {"int",            VarArrayToConsole},
            {"unsigned int",   VarArrayToConsole},
            {"unsigned short", VarArrayToConsole},
            {"float",          VarArrayToConsole},
            //{"Sample", SampleArrayToConsole},
        };

        //**********************************************************************

        // ctor

        static internal void CppCpp_ToConsole (StreamWriter sw, string msgName, List<string []> memberTokens) // : base (members, CToByteRules, CArrayToByteRules)
        {
            sw.WriteLine ("");
            sw.WriteLine ("// member function ToConsole () - write message content to serial port");
            sw.WriteLine ("void " + msgName + "::ToConsole ()");
            sw.Write ("{");

            List<string> code = CodeGenerator_Variables (memberTokens, CppToConsoleRules, CppArrayToConsoleRules);

            foreach (string str in code)
                sw.WriteLine (str);

            sw.WriteLine ("}");
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static private void VarToConsole (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    Serial.print   (\"" + name + " = \"" + ");");
            results.Add ("    Serial.println (data." + name + ");");
        }

        static private void VarArrayToConsole (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        Serial.print   (\"" + name + " [\"" + ");");
            results.Add ("        Serial.print   (i);");
            results.Add ("        Serial.print   (\"] = \");");
            results.Add ("        Serial.println (data." + name + " [i]);");
            results.Add ("    }");
        }
    }
}
