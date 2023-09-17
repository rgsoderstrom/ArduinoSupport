
//
// CppCpp_FromBytes
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    internal partial class MessageCodeGenerator
    {
        //
        // Dictionary of methods to handle various types
        //
        static Dictionary<string, VariableTypeToFromBytes> CFromByteRules = new Dictionary<string, VariableTypeToFromBytes> ()
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

        static Dictionary<string, VariableTypeArrayToFromBytes> CArrayFromByteRules = new Dictionary<string, VariableTypeArrayToFromBytes> ()
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

        // was ctor

        static void CppCpp_FromBytes (StreamWriter sw, string msgName, List<string []> memberTokens)
        {
            sw.WriteLine ("");
            sw.WriteLine ("// from-bytes constructor");
            sw.WriteLine (msgName + "::" + msgName + " (byte *msgBytes)");
            sw.WriteLine ("{");
            sw.WriteLine ("    memset (this, 0, sizeof (" + msgName + "));");
            sw.WriteLine ("");
            sw.WriteLine ("    header.Sync           = (msgBytes [1] << 8) | msgBytes [0];");
            sw.WriteLine ("    header.ByteCount      = (msgBytes [3] << 8) | msgBytes [2];");
            sw.WriteLine ("    header.MsgId          = (msgBytes [5] << 8) | msgBytes [4];");
            sw.WriteLine ("    header.SequenceNumber = (msgBytes [7] << 8) | msgBytes [6];");
            sw.WriteLine ("");
            sw.WriteLine ("    int get = 8;");

            List<string> code = CodeGenerator_Variables (memberTokens, CFromByteRules, CArrayFromByteRules);

            foreach (string str in code)
                sw.WriteLine (str);

            sw.WriteLine ("}");
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static private void OneByteType_FromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    data." + name + " = msgBytes [get]; get += 1;");
        }

        static private void OneByteTypeArray_FromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
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

        static private void TwoByteTypeArray_FromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
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

        static private void FourByteTypeArray_FromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
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
