using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class Cs_MethodsFile
    {
        public List<string> MethodsText { get; protected set; } = new List<string> ();

        public Cs_MethodsFile (string messageNameSpace, string messageName, List<string []> dataMemberTokens)
        {
            Cs_Code.OpenNamespace (MethodsText, messageNameSpace, messageName);
            Cs_Code.OpenClass     (MethodsText, messageName);

            Cs_DefaultCtor ct = new Cs_DefaultCtor (messageName, dataMemberTokens);
            MethodsText.AddRange (ct.MethodText);

            Cs_FromBytes fb = new Cs_FromBytes (messageName, dataMemberTokens);
            MethodsText.AddRange (fb.MethodText);

            Cs_ToBytes tb = new Cs_ToBytes (messageName, dataMemberTokens);
            MethodsText.AddRange (tb.MethodText);

            Cs_ToString ts = new Cs_ToString (messageName, dataMemberTokens);
            MethodsText.AddRange (ts.MethodText);

            MethodsText.Add ("    }"); // close class
            MethodsText.Add ("}");     // close namespace
        }

        public Cs_MethodsFile (StreamWriter sw, string messageNameSpace, string messageName, List<string []> dataMemberTokens)
            : this (messageNameSpace, messageName, dataMemberTokens)
        {
            foreach (string str in MethodsText)
                sw.WriteLine (str);
        }

        //******************************************************************************

    }
}
