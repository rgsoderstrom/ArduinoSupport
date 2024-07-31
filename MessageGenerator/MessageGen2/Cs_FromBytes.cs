
//
// Cs_FromBytes.cs
//

using System;
using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cs_FromBytes 
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        //
        // Dictionary of methods to handle different types
        //
        static Dictionary<string,VariableTypeToCode> CsFromByteRules = new Dictionary<string, VariableTypeToCode> ()
        {
            {"char",           CharFromBytes},
            {"unsigned char",  ByteFromBytes},
            {"byte",           ByteFromBytes},
            {"int",            IntFromBytes},
            {"unsigned int",   IntFromBytes},
            {"short",          IntFromBytes},
            {"unsigned short", UIntFromBytes},
            {"float",          FloatFromBytes},
          //{"Sample", SampleFromBytes},
        };

        static Dictionary<string, VariableTypeArrayToCode> CsArrayFromByteRules = new Dictionary<string, VariableTypeArrayToCode> ()
        {
            {"char",           CharArrayFromBytes},
            {"unsigned char",  ByteArrayFromBytes},
            {"byte",           ByteArrayFromBytes},
            {"int",            IntArrayFromBytes},
            {"unsigned int",   IntArrayFromBytes},
            {"short",          IntArrayFromBytes},
            {"unsigned short", UIntArrayFromBytes},
            {"float",          FloatArrayFromBytes},
          //{"Sample", SampleArrayFromBytes},
        };

        //***************************************************************************************

        public Cs_FromBytes (string msgName, List<string []> memberTokens)
        {
            MethodText.Add ("        //********************************************************");
            MethodText.Add ("        //");
            MethodText.Add ("        // from-bytes constructor");
            MethodText.Add ("        //");
            MethodText.Add ("        public " + msgName + " (byte [] fromBytes)");
            MethodText.Add ("        {");
            MethodText.Add ("            header = new MessageHeader (fromBytes);");
            MethodText.Add ("            data = new Data ();");
            MethodText.Add ("            int byteIndex = Marshal.SizeOf (typeof (MessageHeader));");
            MethodText.Add ("");
            //MethodText.Add ("            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, CsFromByteRules, CsArrayFromByteRules);

            foreach (string str in code)
                MethodText.Add (str);

            MethodText.Add ("        }");
        }

        public Cs_FromBytes (string msgName)
        {
            MethodText.Add ("        //********************************************************");
            MethodText.Add ("        //");
            MethodText.Add ("        // from-bytes constructor");
            MethodText.Add ("        //");
            MethodText.Add ("        public " + msgName + " (byte [] fromBytes)");
            MethodText.Add ("        {");
            MethodText.Add ("            header = new MessageHeader (fromBytes);");
            MethodText.Add ("            int byteIndex = Marshal.SizeOf (typeof (MessageHeader));");
            MethodText.Add ("");
            //MethodText.Add ("            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            //MethodText.Add ("            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            MethodText.Add ("        }");
        }

        public Cs_FromBytes (StreamWriter sw, string msgName, List<string []> memberTokens) : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void CharFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + " = (char) fromBytes [byteIndex++];");
        }

        static internal void CharArrayFromBytes (string name, string count, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + count + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = (char) fromBytes [byteIndex++];"); 
            results.Add ("            }");
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void ByteFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + " = fromBytes [byteIndex++];");
        }

        static internal void ByteArrayFromBytes (string name, string count, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + count + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = fromBytes [byteIndex++];");
            results.Add ("            }");
        }

        //**********************************************************************

        static internal void IntFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + " = BitConverter.ToInt16 (fromBytes, byteIndex); byteIndex += 2;");
        }

        static internal void IntArrayFromBytes (string name, string count, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + count + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToInt16 (fromBytes, byteIndex); byteIndex += 2;");
            results.Add ("            }");
        }

        //**********************************************************************

        static internal void UIntFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + " = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
        }

        static internal void UIntArrayFromBytes (string name, string count, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + count + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            results.Add ("            }");
        }

        //**********************************************************************

        static internal void FloatFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + " = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        }

        static internal void FloatArrayFromBytes (string name, string count, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + count + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            }");
        }

        ////**********************************************************************

        //// Custom code for Sample structure

        //static internal void SampleFromBytes (string name, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("            data." + name + ".enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        //    results.Add ("            data." + name + ".enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        //}

        //static internal void SampleArrayFromBytes (string name, string max, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("            for (int i=0; i<" + max + "; i++)");
        //    results.Add ("            {");
        //    results.Add ("                data." + name + " [i].enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        //    results.Add ("                data." + name + " [i].enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        //    results.Add ("            }");
        //}
    }
}
