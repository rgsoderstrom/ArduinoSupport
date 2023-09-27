
//
// Cs_ToBytes - Convert list of variables to a C# method 
//              that writes those variables to a byte stream
//

using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cs_ToBytes
    {
        public List<string> MethodText { get; protected set; } = new List<string> ();

        static Dictionary<string, VariableTypeToCode> CsToByteRules = new Dictionary<string, VariableTypeToCode> ()
            {
                {"char",           CharToBytes},
                {"unsigned char",  ByteToBytes},
                {"byte",           ByteToBytes},
                {"int",            IntToBytes},
                {"unsigned int",   IntToBytes},
                {"short",          IntToBytes},
                {"unsigned short", IntToBytes},
                {"float",          FloatToBytes},
             // {"Sample", SampleToBytes},
            };

        static Dictionary<string, VariableTypeArrayToCode> CsArrayToByteRules = new Dictionary<string, VariableTypeArrayToCode> ()
            {
                {"char",           CharArrayToBytes},
                {"unsigned char",  ByteArrayToBytes},
                {"byte",           ByteArrayToBytes},
                {"int",            IntArrayToBytes},
                {"unsigned int",   IntArrayToBytes},
                {"short",          IntArrayToBytes},
                {"unsigned short", IntArrayToBytes},
                {"float",          FloatArrayToBytes},
             // {"Sample", SampleArrayToBytes},
            };

        //*******************************************************************************

        public Cs_ToBytes (string msgName, List<string []> memberTokens)
        {
            MethodText.Add ("        //********************************************************");
            MethodText.Add ("        //");
            MethodText.Add ("        // member function ToBytes ()");
            MethodText.Add ("        //");
            MethodText.Add ("        public byte[] ToBytes ()");
            MethodText.Add ("        {");
            MethodText.Add ("            List<byte> byteList = new List<byte> ();");
            MethodText.Add ("");
            MethodText.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.Sync));");
            MethodText.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.ByteCount));");
            MethodText.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.MessageId));");
            MethodText.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.SequenceNumber));");

            List<string> code = MessageCodeGenerator.CodeGenerator_Variables (memberTokens, CsToByteRules, CsArrayToByteRules);
            MethodText.AddRange (code);

            MethodText.Add ("");
            MethodText.Add ("            // append data bytes to header bytes");
            MethodText.Add ("            byte[] msgBytes = new byte [byteList.Count];");
            MethodText.Add ("            byteList.CopyTo (msgBytes, 0);");
            MethodText.Add ("            return msgBytes;");
            MethodText.Add ("        }");
            MethodText.Add ("");
        }

        public Cs_ToBytes (StreamWriter sw, string msgName, List<string []> memberTokens)
            : this (msgName, memberTokens)
        {
            foreach (string str in MethodText)
                sw.WriteLine (str);
        }

        //**********************************************************************
        //**********************************************************************
        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void CharToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.Insert (byteList.Count, (byte) data." + name + ");");
        }

        static internal void CharArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.InsertRange (byteList.Count, Encoding.ASCII.GetBytes (data." + name + "));");
        }

        //**********************************************************************

        static internal void ByteToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.Insert (byteList.Count, data." + name + ");");
        }

        static internal void ByteArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.InsertRange (byteList.Count, data." + name + ");");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static internal void IntToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + "));");
        }

        static internal void IntArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i]));");
            results.Add ("            }");
        }

        //**********************************************************************

        static internal void FloatToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + "));");
        }

        static internal void FloatArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i]));");
            results.Add ("            }");
        }

        //**********************************************************************

        //static internal void SampleToBytes (string name, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + ".enc1));");
        //    results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + ".enc2));");
        //}

        //static internal void SampleArrayToBytes (string name, string max, List<string> results)
        //{
        //    results.Add ("");
        //    results.Add ("            for (int i=0; i<" + max + "; i++)");
        //    results.Add ("            {");
        //    results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i].enc1));");
        //    results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i].enc2));");
        //    results.Add ("            }");
        //}
    }
}
