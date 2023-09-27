using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    //
    // delegates for (i.e. function pointers to) methods that are passed a variable's name
    // and return source code that act on that variable
    //
    //  

    // example: table for "ToBytes ()"
    //   Table =
    //   {
    //      "char", charToBytesMethod,
    //      "int",  intToBytesMethod,
    //      etc.
    //   }

    // charToBytes (string name, List<string> sourceCode)
    // {
    // }


    public delegate void VariableTypeToCode      (string name, List<string> codeOutput);
    public delegate void VariableTypeArrayToCode (string name, string arrayCount, List<string> results);
}
