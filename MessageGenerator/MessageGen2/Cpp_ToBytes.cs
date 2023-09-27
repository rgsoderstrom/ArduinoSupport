
//
// Cpp_ToBytes - Convert list of variables to a C++ method 
//               that writes those variables to a byte stream
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cpp_ToBytes
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle the different types
        //
        static Dictionary<string, VariableTypeToCode> ToByteRules = new Dictionary<string, VariableTypeToCode> ()
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

        static Dictionary<string, VariableTypeArrayToCode> ArrayToByteRules = new Dictionary<string, VariableTypeArrayToCode> ()
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

        public Cpp_ToBytes (string msgName, List<string []> memberTokens)
        {
            MethodText.Add ("");
            MethodText.Add ("//");
            MethodText.Add ("// member function ToBytes ()");
            MethodText.Add ("//");
            MethodText.Add ("void " + msgName + "::ToBytes (byte *byteArray)");
            MethodText.Add ("{");
            MethodText.Add ("    int put = 0;");
            MethodText.Add ("    byteArray [put++] = header.Sync;");
            MethodText.Add ("    byteArray [put++] = header.Sync >> 8;");
            MethodText.Add ("    byteArray [put++] = header.ByteCount;");
            MethodText.Add ("    byteArray [put++] = header.ByteCount >> 8;");
            MethodText.Add ("    byteArray [put++] = header.MsgId;");
            MethodText.Add ("    byteArray [put++] = header.MsgId >> 8;");
            MethodText.Add ("    byteArray [put++] = header.SequenceNumber;");
            MethodText.Add ("    byteArray [put++] = header.SequenceNumber >> 8;");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, ToByteRules, ArrayToByteRules);

            foreach (string str in code)
                MethodText.Add (str);

            MethodText.Add ("}");
        }

        public Cpp_ToBytes (StreamWriter sw, string msgName, List<string []> memberTokens) : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static private void OneByteType_ToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("    byteArray [put++] = data." + name + ";");
        }

        static private void OneByteTypeArray_ToBytes (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
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

        static private void TwoByteTypeArray_ToBytes (string name, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
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

        static private void FourByteTypeArray_ToBytes (string memberVariable, string count, List<string> results)
        {
            string ccount = count.Replace ("Data.", "Data::");

            results.Add ("");
            results.Add ("    for (int i=0; i<" + ccount + "; i++)");
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
