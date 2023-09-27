
//
// Cpp_FromBytes.cs
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cpp_FromBytes
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle various types
        //
        static Dictionary<string, VariableTypeToCode> FromByteRules = new Dictionary<string, VariableTypeToCode> ()
        {
            {"char",           OneByteType_FromBytes},
            {"byte",           OneByteType_FromBytes},
            {"unsigned char",  OneByteType_FromBytes},
            {"unsigned byte",  OneByteType_FromBytes},
            {"int",            TwoByteType_FromBytes},
            {"short",          TwoByteType_FromBytes},
            {"unsigned int",   TwoByteType_FromBytes},
            {"unsigned short", TwoByteType_FromBytes},
            {"float",          FourByteType_FromBytes},
         //   {"Sample", SampleFromBytes},
        };

        static Dictionary<string, VariableTypeArrayToCode> ArrayFromByteRules = new Dictionary<string, VariableTypeArrayToCode> ()
        {
            {"char",           OneByteTypeArray_FromBytes},
            {"unsigned char",  OneByteTypeArray_FromBytes},
            {"byte",           OneByteTypeArray_FromBytes},
            {"int",            TwoByteTypeArray_FromBytes},
            {"short",          TwoByteTypeArray_FromBytes},
            {"float",          FourByteTypeArray_FromBytes},
         //   {"Sample", SampleArrayFromBytes},
        };

        //**********************************************************************

        // ctor

        public Cpp_FromBytes (string msgName, List<string []> memberTokens)
        {
            MethodText.Add ("");
            MethodText.Add ("//");
            MethodText.Add ("// from-bytes constructor");
            MethodText.Add ("//");
            MethodText.Add (msgName + "::" + msgName + " (byte *msgBytes)");
            MethodText.Add ("{");
            MethodText.Add ("    memset (this, 0, sizeof (" + msgName + "));");
            MethodText.Add ("");
            MethodText.Add ("    header.Sync           = (msgBytes [1] << 8) | msgBytes [0];");
            MethodText.Add ("    header.ByteCount      = (msgBytes [3] << 8) | msgBytes [2];");
            MethodText.Add ("    header.MsgId          = (msgBytes [5] << 8) | msgBytes [4];");
            MethodText.Add ("    header.SequenceNumber = (msgBytes [7] << 8) | msgBytes [6];");
            MethodText.Add ("");
            MethodText.Add ("    int get = 8;");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, FromByteRules, ArrayFromByteRules);

            foreach (string str in code)
                MethodText.Add (str);

            MethodText.Add ("}");
        }

        public Cpp_FromBytes (StreamWriter sw, string msgName, List<string []> memberTokens) : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static private void OneByteType_FromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    data." + name + " = msgBytes [get]; get += 1;");
        }

        static private void OneByteTypeArray_FromBytes (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
            results.Add ("    {");
            results.Add ("         data." + name + " [i] = msgBytes [get]; get += 1;");
            results.Add ("    }");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static private void TwoByteType_FromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    *(((byte *) &data." + name + ") + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 1) = msgBytes [get]; get += 1;");
        }

        static private void TwoByteTypeArray_FromBytes (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
            results.Add ("    {");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 0) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 1) = msgBytes [get]; get += 1;");
            results.Add ("    }");
        }

        //**********************************************************************

        // float name;      
        // float name [8];

        static private void FourByteType_FromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    *(((byte *) &data." + name + ") + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 1) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 2) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 3) = msgBytes [get]; get += 1;");
        }

        static private void FourByteTypeArray_FromBytes (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
            results.Add ("    {");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 0) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 1) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 2) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 3) = msgBytes [get]; get += 1;");
            results.Add ("    }");
        }

        //**********************************************************************

        // Custom code for this structure

        // struct Sample
        // {
        //      float enc1;
        //      float enc2;
        //  }
        //
        //  TypeData data
        //  {
        //      Sample sample;
        //  }

        //static private void SampleFromBytes (string name, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("    *(((byte *) &data." + name + ".enc1) + 0) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc1) + 1) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc1) + 2) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc1) + 3) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc2) + 0) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc2) + 1) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc2) + 2) = msgBytes [get]; get += 1;");
        //    results.Add ("    *(((byte *) &data." + name + ".enc2) + 3) = msgBytes [get]; get += 1;");
        //}

        //static private void SampleArrayFromBytes (string name, string max, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("    for (int i=0; i<data." + max + "; i++)");
        //    results.Add ("    {");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 0) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 1) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 2) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 3) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 0) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 1) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 2) = msgBytes [get]; get += 1;");
        //    results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 3) = msgBytes [get]; get += 1;");
        //    results.Add ("    }");
        //}

    }
}
