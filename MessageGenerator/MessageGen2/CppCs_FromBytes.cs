
using System;
using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    partial class MessageCodeGenerator //: MsgGenBase
    {
        //
        // Dictionary of custom methods to handle built-in types
        //
        static Dictionary<string,VariableTypeToFromBytes> CsFromByteRules = new Dictionary<string, VariableTypeToFromBytes> ()
        {
            {"char",  CharFromBytes},
            {"byte",  ByteFromBytes},
            {"int",   IntFromBytes},
            {"short", IntFromBytes},
            {"unsigned short", UIntFromBytes},
            {"float", FloatFromBytes},
            {"Sample", SampleFromBytes},
        };

        static Dictionary<string, VariableTypeArrayToFromBytes> CsArrayFromByteRules = new Dictionary<string, VariableTypeArrayToFromBytes> ()
        {
            {"char",  CharArrayFromBytes},
            {"byte",  ByteArrayFromBytes},
            {"int",   IntArrayFromBytes},
            {"short", IntArrayFromBytes},
            {"unsigned short", UIntArrayFromBytes},
            {"float", FloatArrayFromBytes},
            {"Sample", SampleArrayFromBytes},
        };

        static void CppCs_FromBytes (StreamWriter sw, string msgName, List<string []> memberTokens) //: base (members, CToByteRules, CArrayToByteRules)
        {
            List<string> code = CodeGenerator_Variables (memberTokens, CsFromByteRules, CsArrayFromByteRules);

            foreach (string str in code)
                sw.WriteLine (str);
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
            results.Add ("");
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = (char) fromBytes [byteIndex++];");
            results.Add ("");
            results.Add ("                 if (byteIndex == fromBytes.Length)");
            results.Add ("                     break;");
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

        static internal void ByteArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            if (Char.IsLetter (max [0])) max = "Data." + max;

            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
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

        static internal void IntArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
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

        static internal void UIntArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<Data." + max + "; i++)");
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

        static internal void FloatArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                 data." + name + " [i] = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            }");
        }

        //**********************************************************************

        // Custom code for Sample structure

        static internal void SampleFromBytes (string name, List<string> results)
        {
            results.Add ("");
            results.Add ("            data." + name + ".enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            data." + name + ".enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
        }

        static internal void SampleArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("");
            results.Add ("            for (int i=0; i<" + max + "; i++)");
            results.Add ("            {");
            results.Add ("                data." + name + " [i].enc1 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("                data." + name + " [i].enc2 = BitConverter.ToSingle (fromBytes, byteIndex); byteIndex += 4;");
            results.Add ("            }");
        }
    }
}
