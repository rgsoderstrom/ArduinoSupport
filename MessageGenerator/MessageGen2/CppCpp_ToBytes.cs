
//
// CppCpp_ToBytes - C++ code in and C++ code out
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
        static Dictionary<string, VariableTypeToFromBytes> CppToByteRules = new Dictionary<string, VariableTypeToFromBytes> ()
        {
            {"char",           OneByteType_ToBytes},
            {"unsigned char",  OneByteType_ToBytes},
            {"byte",           OneByteType_ToBytes},
            {"int",            TwoByteType_ToBytes},
            {"unsigned int",   TwoByteType_ToBytes},
            {"short",          TwoByteType_ToBytes},
            {"unsigned short", TwoByteType_ToBytes},
            {"float",          FourByteType_ToBytes},
        //  {"Sample", SampleToBytes},
        };

        static Dictionary<string, VariableTypeArrayToFromBytes> CppArrayToByteRules = new Dictionary<string, VariableTypeArrayToFromBytes> ()
        {
            {"char",           OneByteTypeArray_ToBytes},
            {"unsigned char",  OneByteTypeArray_ToBytes},
            {"byte",           OneByteTypeArray_ToBytes},
            {"int",            TwoByteTypeArray_ToBytes},
            {"unsigned int",   TwoByteTypeArray_ToBytes},
            {"short",          TwoByteTypeArray_ToBytes},
            {"unsigned short", TwoByteTypeArray_ToBytes},
            {"float",          FourByteTypeArray_ToBytes},
        //  {"Sample", SampleArrayToBytes},
        };

        //**********************************************************************

        // ctor

        static void CppCpp_ToBytes (StreamWriter sw, string msgName, List<string []> memberTokens)// : base (members, CToByteRules, CArrayToByteRules)
        {
            sw.WriteLine ("");
            sw.WriteLine ("// member function ToBytes ()");
            sw.WriteLine ("void " + msgName + "::ToBytes (byte *byteArray)");
            sw.WriteLine ("{");
            sw.WriteLine ("    int put = 0;");
            sw.WriteLine ("    byteArray [put++] = header.Sync;");
            sw.WriteLine ("    byteArray [put++] = header.Sync >> 8;");
            sw.WriteLine ("    byteArray [put++] = header.ByteCount;");
            sw.WriteLine ("    byteArray [put++] = header.ByteCount >> 8;");
            sw.WriteLine ("    byteArray [put++] = header.MsgId;");
            sw.WriteLine ("    byteArray [put++] = header.MsgId >> 8;");
            sw.WriteLine ("    byteArray [put++] = header.SequenceNumber;");
            sw.WriteLine ("    byteArray [put++] = header.SequenceNumber >> 8;");

            List<string> code = CodeGenerator_Variables (memberTokens, CppToByteRules, CppArrayToByteRules);

            foreach (string str in code)
                sw.WriteLine (str);

            sw.WriteLine ("}");
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static private void OneByteType_ToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    byteArray [put++] = data." + name + ";");
        }

        static private void OneByteTypeArray_ToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = data." + name + " [i];");
            results.Add ("    }");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static private void TwoByteType_ToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    byteArray [put++] = data." + name + ";");
            results.Add ("    byteArray [put++] = data." + name + " >> 8;");
        }

        static private void TwoByteTypeArray_ToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = data." + name + " [i];");
            results.Add ("        byteArray [put++] = data." + name + " [i] >> 8;");
            results.Add ("    }");
        }

        //**********************************************************************

        static private void FourByteType_ToBytes (string memberVariable, List<string> results)
        {
            results.Add ("");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >>  0)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >>  8)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >> 16)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >> 24)  & 0xff;");
        }

        static private void FourByteTypeArray_ToBytes (string memberVariable, string Count, List<string> results)
        {
            results.Add ("");
            results.Add ("    for (int i=0; i<data." + Count + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >>  0)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >>  8)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >> 16)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >> 24)  & 0xff;");
            results.Add ("    }");
        }

        //**********************************************************************

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
        //
        //static private void SampleToBytes (string name, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >>  0)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >>  8)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >> 16)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >> 24)  & 0xff;");

        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >>  0)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >>  8)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >> 16)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >> 24)  & 0xff;");
        //}

        //static private void SampleArrayToBytes (string name, string Count, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("    for (int i=0; i<data." + Count + "; i++)");
        //    results.Add ("    {");

        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >>  0)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >>  8)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >> 16)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >> 24)  & 0xff;");

        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >>  0)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >>  8)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >> 16)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >> 24)  & 0xff;");

        //    results.Add ("    }");
        //}
    }
}
