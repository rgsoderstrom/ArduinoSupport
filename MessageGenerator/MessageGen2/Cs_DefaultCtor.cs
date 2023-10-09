using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class Cs_DefaultCtor
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle the different types
        //
        private Dictionary<string, VariableTypeToCode> ByteCounts = new Dictionary<string, VariableTypeToCode> ()
        {
            {"char",           sizeOfChar},   // 8 bbits
            {"unsigned char",  sizeOfUChar},  // "
            {"byte",           sizeOfUChar},  // "
            {"int",            sizeOfShort},  // 16 bits
            {"unsigned int",   sizeOfUShort}, // "
            {"short",          sizeOfShort},
            {"unsigned short", sizeOfUShort},
            {"float",          sizeOfFloat},
            //{"Sample", SampleToConsole},
        };

        Dictionary<string, VariableTypeArrayToCode> ArrayByteCounts = new Dictionary<string, VariableTypeArrayToCode> ()
        {
            {"char",           sizeOfCharArray},
            {"unsigned char",  sizeOfUCharArray},
            {"byte",           sizeOfUCharArray},
            {"int",            sizeOfShortArray},
            {"unsigned int",   sizeOfUShortArray},
            {"short",          sizeOfUShortArray},
            {"unsigned short", sizeOfUShortArray},
            {"float",          sizeOfFloatArray},
            //{"Sample", SampleArrayToConsole},
        };

        //**********************************************************************

        internal Cs_DefaultCtor (string messageName, List<string []> dataMemberTokens)
        {
            string msgName = messageName.Replace ("_Auto", string.Empty) + "Id";

            MethodText.Add ("        //");
            MethodText.Add ("        // Default ctor");
            MethodText.Add ("        //");
            MethodText.Add ("        public " + messageName + " ()");
            MethodText.Add ("        {");
            MethodText.Add ("             header = new MessageHeader ();");
            MethodText.Add ("             data = new Data ();");
            MethodText.Add ("");
            MethodText.Add ("             header.MessageId = (ushort) ArduinoMessageIDs." + msgName + ";");

            //
            // byte count
            //
            MethodText.Add ("             header.ByteCount = (ushort)(Marshal.SizeOf (header)");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (dataMemberTokens, ByteCounts, ArrayByteCounts);

            foreach (string str in code)
                MethodText.Add (str);

            MethodText [MethodText.Count - 1] += ");"; // terminate byte-count addition

            //***********************************************************

            MethodText.Add ("         }");
            MethodText.Add ("");
        }

        public Cs_DefaultCtor (StreamWriter sw, string msgName, List<string []> memberTokens)
            : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        static string spaces = "\t\t\t\t\t\t\t  ";

        static void sizeOfChar   (string name, List<string> results) {results.Add (spaces + "+ sizeof (byte)");}
        static void sizeOfUChar  (string name, List<string> results) {results.Add (spaces + "+ sizeof (byte)");}
        static void sizeOfShort  (string name, List<string> results) {results.Add (spaces + "+ sizeof (Int16)");}
        static void sizeOfUShort (string name, List<string> results) {results.Add (spaces + "+ sizeof (UInt16)");}
        static void sizeOfFloat  (string name, List<string> results) {results.Add (spaces + "+ sizeof (float)");}

        static void sizeOfCharArray   (string name, string count, List<string> results) {results.Add (spaces + "+ sizeof (byte) * " + count);}
        static void sizeOfUCharArray  (string name, string count, List<string> results) {results.Add (spaces + "+ sizeof (byte) * " + count);}
        static void sizeOfShortArray  (string name, string count, List<string> results) {results.Add (spaces + "+ sizeof (Int16) * " + count);}
        static void sizeOfUShortArray (string name, string count, List<string> results) {results.Add (spaces + "+ sizeof (Int16) * " + count);}
        static void sizeOfFloatArray  (string name, string count, List<string> results) {results.Add (spaces + "+ sizeof (float) * " + count);}
    }
}
