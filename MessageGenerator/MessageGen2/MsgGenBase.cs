using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    internal class MsgGenBase
    {
        public List<string> results = new List<string> ();

        protected delegate void BuiltInTypeToFromBytes      (string inp, List<string> reults);
        protected delegate void BuiltInTypeArrayToFromBytes (string inp, string count, List<string> reults);

        protected List<string []> linesAsTokens = new List<string []> ();

        //******************************************************************************
        //
        // Base class ctor - passed a list of all the members of one structure
        //                     - splits into tokens and does some error checking
        //                 - also receives lists of type handlers
        //
        protected MsgGenBase (List<string> memberVariables, 
                              Dictionary <string, BuiltInTypeToFromBytes> typeHandlers, 
                              Dictionary<string, BuiltInTypeArrayToFromBytes> typeArrayHandlers)
        {
            foreach (string str in memberVariables)
            {
                string [] tokens = str.Split (new char [] { ' ', '\t', '[', ']', ';' }, StringSplitOptions.RemoveEmptyEntries);

                //
                // if first token is "unsigned", combine it with second
                //
                if (tokens [0] == "unsigned")
                {
                    string [] tokens2 = new string [tokens.Length - 1];
                    tokens2 [0] = tokens [0] + " " + tokens [1];

                    for (int i = 2; i<tokens.Length; i++)
                        tokens2 [i-1] = tokens [i];

                    tokens = tokens2;
                }

                if (tokens.Length == 2 || tokens.Length == 3)
                    linesAsTokens.Add (tokens);
                else
                    throw new Exception ("Syntax error in line: " + str);
            }

            foreach (string[] tokens in linesAsTokens)
            {
                if (tokens.Length == 2)
                {
                    if (typeHandlers.ContainsKey (tokens [0]))
                    {
                        BuiltInTypeToFromBytes tp = typeHandlers [tokens [0]];
                        tp (tokens [1], results);
                    }

                    else
                        throw new Exception ("Type " + tokens [0] + " not found");
                }

                else if (tokens.Length == 3)
                {
                    if (typeArrayHandlers.ContainsKey (tokens [0]))
                    {
                        BuiltInTypeArrayToFromBytes tp = typeArrayHandlers [tokens [0]];
                        tp (tokens [1], tokens [2], results);
                    }

                    else
                        throw new Exception ("Array type " + tokens [0] + " not found");
                }
            }
        }
    }
}
