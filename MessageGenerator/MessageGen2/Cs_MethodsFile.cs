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

        protected Cs_MethodsFile (string messageNameSpace, string messageName, List<string []> dataMemberTokens)
        {
            bool headerOnly = dataMemberTokens.Count == 0;

            Cs_Code.OpenNamespace (MethodsText, messageNameSpace, messageName);
            Cs_Code.OpenClass     (MethodsText, messageName);

            if (headerOnly == false)
            { 
                Cs_DefaultCtor ct = new Cs_DefaultCtor (messageName, dataMemberTokens); 
                MethodsText.AddRange (ct.MethodText);

                Cs_FromBytes fb = new Cs_FromBytes (messageName, dataMemberTokens); 
                MethodsText.AddRange (fb.MethodText);

                Cs_ToBytes tb = new Cs_ToBytes (messageName, dataMemberTokens); 
                MethodsText.AddRange (tb.MethodText);

                Cs_ToString ts = new Cs_ToString (messageName, dataMemberTokens);
                MethodsText.AddRange (ts.MethodText);
            }
            else
            {
                Cs_DefaultCtor ct = new Cs_DefaultCtor (messageName);
                MethodsText.AddRange (ct.MethodText);

                Cs_FromBytes fb = new Cs_FromBytes (messageName);
                MethodsText.AddRange (fb.MethodText);

                Cs_ToBytes tb = new Cs_ToBytes (messageName);
                MethodsText.AddRange (tb.MethodText);

                Cs_ToString ts = new Cs_ToString (messageName);
                MethodsText.AddRange (ts.MethodText);
            }

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
