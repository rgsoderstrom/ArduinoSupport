
using System;
using System.Collections.Generic;

namespace MessageGenerator
{
    internal class CppCs_FromBytes : MsgGenBase
    {
        //
        // Dictionary of custom methods to handle built-in types
        //
        static Dictionary<string, BuiltInTypeToFromBytes> CToByteRules = new Dictionary<string, BuiltInTypeToFromBytes> ()
        {
            {"char",  CharFromBytes},
            {"byte",  ByteFromBytes},
            {"int",   IntFromBytes},
            {"short", IntFromBytes},
            {"unsigned short", UIntFromBytes},
            {"float", FloatFromBytes},
            {"Sample", SampleFromBytes},
        };

        static Dictionary<string, BuiltInTypeArrayToFromBytes> CArrayToByteRules = new Dictionary<string, BuiltInTypeArrayToFromBytes> ()
        {
            {"char",  CharArrayFromBytes},
            {"byte",  ByteArrayFromBytes},
            {"int",   IntArrayFromBytes},
            {"short", IntArrayFromBytes},
            {"unsigned short", UIntArrayFromBytes},
            {"float", FloatArrayFromBytes},
            {"Sample", SampleArrayFromBytes},
        };
        
        internal CppCs_FromBytes (List<string> members) : base (members, CToByteRules, CArrayToByteRules)
        {
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void CharFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + " = (char) fromBytes [byteIndex++];");
            results.Add ("");
        }

        static internal void CharArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = (char) fromBytes [byteIndex++];");
            results.Add ("");
            results.Add ("                 if (byteIndex == fromBytes.Length)");
            results.Add ("                     break;");
            results.Add ("            }");
            results.Add ("");
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void ByteFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + " = fromBytes [byteIndex++];");
            results.Add ("");
        }

        static internal void ByteArrayFromBytes (string name, string max, List<string> results)
        {
            if (Char.IsLetter (max [0])) max = "Data." + max;

            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = fromBytes [byteIndex++];");
            results.Add ("            }");
            results.Add ("");
        }

        //**********************************************************************

        static internal void IntFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + " = BitConverter.ToInt16 (fromBytes, byteIndex); byteIndex += 2;");           
            results.Add ("");
        }

        static internal void IntArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToInt16 (fromBytes, byteIndex); byteIndex += 2;");
            results.Add ("            }");
            results.Add ("");
        }

        //**********************************************************************

        static internal void UIntFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + " = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");           
            results.Add ("");
        }

        static internal void UIntArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;");
            results.Add ("            }");
            results.Add ("");
        }

        //**********************************************************************

        static internal void FloatFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + " = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");            
            results.Add ("");
        }

        static internal void FloatArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            }");
            results.Add ("");
        }

        //**********************************************************************

        static internal void SampleFromBytes (string name, List<string> results)
        {
            results.Add ("            data." + name + ".enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            data." + name + ".enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("");
        }

        static internal void SampleArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                data." + name + " [i].enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("                data." + name + " [i].enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            }");
            results.Add ("");
        }
    }
}
