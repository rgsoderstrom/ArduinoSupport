using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class Cs_ToString
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle the different types
        //
        private Dictionary<string, VariableTypeToCode> CsToStringRules = new Dictionary<string, VariableTypeToCode> ()
        {
            {"char",           VarToString},
            {"unsigned char",  VarToString},
            {"byte",           VarToString},
            {"int",            VarToString},
            {"unsigned int",   VarToString},
            {"short",          VarToString},
            {"unsigned short", VarToString},
            {"float",          VarToString},
            //{"Sample", SampleToConsole},
        };

        Dictionary<string, VariableTypeArrayToCode> CsArrayToStringRules = new Dictionary<string, VariableTypeArrayToCode> ()
        {
            {"char",           VarArrayToString},
            {"unsigned char",  VarArrayToString},
            {"byte",           VarArrayToString},
            {"int",            VarArrayToString},
            {"unsigned int",   VarArrayToString},
            {"unsigned short", VarArrayToString},
            {"float",          VarArrayToString},
            //{"Sample", SampleArrayToConsole},
        };

        //**********************************************************************


            //str += "Sync      = " + header.Sync + "\n";
            //str += "ByteCount = " + header.ByteCount + "\n";
            //str += "ID        = " + header.MessageId + "\n";
            //str += "Seq Numb  = " + header.SequenceNumber + "\n";


        public Cs_ToString (string msgName, List<string []> memberTokens)
        {


            MethodText.Add ("        //********************************************************");
            MethodText.Add ("        //");
            MethodText.Add ("        // member function ToString ()");
            MethodText.Add ("        //");
            MethodText.Add ("        public override string ToString ()");
            MethodText.Add ("        {");
            MethodText.Add ("            string str = \"\";");
            MethodText.Add ("            str += \"Sync      = \"" + " + header.Sync"           + " + " + "\"\\n\";");
            MethodText.Add ("            str += \"ByteCount = \"" + " + header.ByteCount"      + " + " + "\"\\n\";");
            MethodText.Add ("            str += \"ID        = \"" + " + header.MessageId"      + " + " + "\"\\n\";");
            MethodText.Add ("            str += \"SeqNumb   = \"" + " + header.SequenceNumber" + " + " + "\"\\n\";");
            MethodText.Add ("");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, CsToStringRules, CsArrayToStringRules);
            MethodText.AddRange (code);

            MethodText.Add ("");
            MethodText.Add ("            return str;");
            MethodText.Add ("        }");
        }

        public Cs_ToString (StreamWriter sw, string msgName, List<string []> memberTokens)
            : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static void VarToString (string name, List<string> results)
        {
            results.Add ("            str += \"" + name + " = \" + data." + name + " + " + "\"\\n\";");
        }

        static void VarArrayToString (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 str += \"" + name + " [\" + i + \"] = \";");
            results.Add ("                 str += data." + name + " [i];");
            results.Add ("                 str += \"\\n\";" );
            results.Add ("            }");
        }
    }
}
