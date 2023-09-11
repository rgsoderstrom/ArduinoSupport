
//
// CppCpp_ToBytes - C++ code in and C# code out
//

using System.Collections.Generic;

namespace MessageGenerator
{
    internal class CppCs_ToBytes : MsgGenBase
    {
        static Dictionary<string, BuiltInTypeToFromBytes> CToByteRules = new Dictionary<string, BuiltInTypeToFromBytes> ()
        {
            {"char",  CharToBytes},
            {"byte",  ByteToBytes},
            {"int",   IntToBytes},
            {"short", IntToBytes},
            {"unsigned short", IntToBytes},
            {"float", FloatToBytes},
            {"Sample", SampleToBytes},
        };

        static Dictionary<string, BuiltInTypeArrayToFromBytes> CArrayToByteRules = new Dictionary<string, BuiltInTypeArrayToFromBytes> ()
        {
            {"char",  CharArrayToBytes},
            {"byte",  ByteArrayToBytes},
            {"int",   IntArrayToBytes},
            {"short", IntArrayToBytes},
            {"unsigned short", IntArrayToBytes},
            {"float", FloatArrayToBytes},
            {"Sample", SampleArrayToBytes},
        };

        //*******************************************************************************

        internal CppCs_ToBytes (List<string> members) : base (members, CToByteRules, CArrayToByteRules)
        {
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

        static internal void SampleToBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + ".enc1));");
            results.Add ("            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + ".enc2));");
        }

        static internal void SampleArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i].enc1));");
            results.Add ("                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data." + name + " [i].enc2));");
            results.Add ("            }");           
        }
    }
}
