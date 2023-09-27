
//
// Cpp_ToConsole - C++ code in and C++ code out
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cpp_ToConsole
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle the different types
        //
        private Dictionary<string, VariableTypeToCode> CppToConsoleRules = new Dictionary<string, VariableTypeToCode> ()
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

        Dictionary<string, VariableTypeArrayToCode> CppArrayToConsoleRules = new Dictionary<string, VariableTypeArrayToCode> ()
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

        public Cpp_ToConsole (string msgName, List<string []> memberTokens)
        {
            MethodText.Add ("");
            MethodText.Add ("//");
            MethodText.Add ("// member function ToConsole () - write message content to serial port");
            MethodText.Add ("//");
            MethodText.Add ("void " + msgName + "::ToConsole ()");
            MethodText.Add ("{");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, CppToConsoleRules, CppArrayToConsoleRules);

            foreach (string str in code)
                MethodText.Add (str);

            MethodText.Add ("}");
        }

        public Cpp_ToConsole (StreamWriter sw, string msgName, List<string []> memberTokens) : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static void VarToConsole (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    Serial.print   (\"" + name + " = \"" + ");");
            results.Add ("    Serial.println (data." + name + ");");
        }

        static void VarArrayToConsole (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
            results.Add ("    {");
            results.Add ("        Serial.print   (\"" + name + " [\"" + ");");
            results.Add ("        Serial.print   (i);");
            results.Add ("        Serial.print   (\"] = \");");
            results.Add ("        Serial.println (data." + name + " [i]);");
            results.Add ("    }");
        }
    }
}
