using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class Cs_FormatFile
    {
        public List<string> FormatText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle the different types
        //

        private Dictionary<string, VariableTypeToCode> DefineVariables = new Dictionary<string, VariableTypeToCode> ()
        {
            {"char",           DefineChar},   // 8 bits
            {"unsigned char",  DefineUChar},  // "
            {"byte",           DefineUChar},  // 8 bits, byte & uchar same
            {"int",            DefineInt},  // 16 bits
            {"unsigned int",   DefineUInt}, // "
            {"short",          DefineShort},
            {"unsigned short", DefineUShort},
            {"float",          DefineFloat},
            //{"Sample", SampleToConsole},
        };

        Dictionary<string, VariableTypeArrayToCode> DefineVariableArrays = new Dictionary<string, VariableTypeArrayToCode> ()
        {
            {"char",           DefineCharArray},
            {"unsigned char",  DefineUCharArray},
            {"byte",           DefineUCharArray},
            {"int",            DefineIntArray},
            {"unsigned int",   DefineUIntArray},
            {"short",          DefineShortArray},
            {"unsigned short", DefineUShortArray},
            {"float",          DefineFloatArray},
            //{"Sample", SampleArrayToConsole},
        };

        //***************************************************************************************************

        public Cs_FormatFile (string messageNameSpace, string messageName, List<string []> constMemberTokens, List<string []> dataMemberTokens)
        {
            FormatText.Add ("using System;");
  //          FormatText.Add ("using System.Collections.Generic;");
//            FormatText.Add ("using System.Linq;");
            FormatText.Add ("using System.Runtime.InteropServices;");
    //        FormatText.Add ("using System.Runtime.Remoting.Messaging;");
     //       FormatText.Add ("using System.Text;");
  //          FormatText.Add ("using System.Threading.Tasks;");
            FormatText.Add ("");
            FormatText.Add ("//");
            FormatText.Add ("// auto generated message format code");
            FormatText.Add ("//");
            FormatText.Add ("");
            FormatText.Add ("using SocketLib;");

            FormatText.Add ("namespace " + messageNameSpace);
            FormatText.Add ("{");
            FormatText.Add ("    public partial class " + messageName);
            FormatText.Add ("    {");
            FormatText.Add ("        [StructLayout (LayoutKind.Sequential, Pack = 1)]");
            FormatText.Add ("        public class Data");
            FormatText.Add ("        {");

            try
            {
                DefineConstants (constMemberTokens);
                FormatText.Add ("");

                List<string> code = MessageCodeGenerator.CodeGenerator_Variables (dataMemberTokens, DefineVariables, DefineVariableArrays);
                FormatText.AddRange (code);
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception generating Cs format file: " + ex.Message);
            }


            FormatText.Add ("        };");
            FormatText.Add ("");
            FormatText.Add ("        public MessageHeader header;");
            FormatText.Add ("        public Data data;");
            FormatText.Add ("    }");
            FormatText.Add ("}");
        }

        public Cs_FormatFile (StreamWriter sw, string messageNameSpace, string messageName, List<string []> constMemberTokens, List<string []> dataMemberTokens)
            : this (messageNameSpace, messageName, constMemberTokens, dataMemberTokens)
        {
            foreach (string str in FormatText)
                sw.WriteLine (str);
        }

        //**********************************************************************************

        protected void DefineConstants (List<string []> constants)
        {
            foreach (string [] token in constants)
            {
                if (token [0] == "unsigned char") token [0] = "byte";
                if (token [0] == "unsigned int")  token [0] = "UInt16";

                // char aaa = 7; => char aaa = (char) 7;
                if (token [0] == "char" && char.IsDigit (token [2] [0]))
                    token [2] = "(char) " + token [2];


                FormatText.Add ("            static public " + token [0] + " " + token [1] + " = " + token [2] + ";");
            }
        }

        //**********************************************************************************

        //"char aaa",  becomes  public char aaa;

        //"short ppp;",
        // becomes
        //public short ppp;

        //"unsigned char bbbb [Data.CCC];",
        // becomes
        //public byte [] bbbb = new byte [CCC];

        //"int LBData [Data.WordCount];",
        // becomes
        //public short [] LBData = new short [WordCount];

        //"int ddd;",
        // becomes
        //public short ddd;

        //"float dddd [Data.N];"
        // becomes
        //public float [] dddd = new float [N];


        //**********************************************************************************

        static protected void DefineScalar (string type, string name, List<string> results)
        {
            results.Add ("            public " + type + " " + name + ";");
        }

        static protected void DefineArray (string type, string name, string count, List<string> results)
        {
            results.Add ("            public " + type + " [] " + name + " = new " + type + "  [" + count + "];");
        }

        //**********************************************************************************

        static protected void DefineChar   (string name, List<string> results) {DefineScalar ("char",   name, results);}
        static protected void DefineUChar  (string name, List<string> results) {DefineScalar ("byte",   name, results);}
        static protected void DefineShort  (string name, List<string> results) {DefineScalar ("short",  name, results);}
        static protected void DefineUShort (string name, List<string> results) {DefineScalar ("UInt16", name, results);}
        static protected void DefineInt    (string name, List<string> results) {DefineScalar ("short",  name, results);}
        static protected void DefineUInt   (string name, List<string> results) {DefineScalar ("UInt16", name, results);}
        static protected void DefineFloat  (string name, List<string> results) {DefineScalar ("float",  name, results);}

        static protected void DefineCharArray   (string name, string count, List<string> results) {DefineArray ("char",   name, count, results);}
        static protected void DefineUCharArray  (string name, string count, List<string> results) {DefineArray ("byte",   name, count, results);}
        static protected void DefineShortArray  (string name, string count, List<string> results) {DefineArray ("short",  name, count, results);}
        static protected void DefineUShortArray (string name, string count, List<string> results) {DefineArray ("UInt16", name, count, results);}
        static protected void DefineIntArray    (string name, string count, List<string> results) {DefineArray ("short",  name, count, results);}
        static protected void DefineUIntArray   (string name, string count, List<string> results) {DefineArray ("UInt16", name, count, results);}
        static protected void DefineFloatArray  (string name, string count, List<string> results) {DefineArray ("float",  name, count, results);}

        //**********************************************************************************
    }
}


