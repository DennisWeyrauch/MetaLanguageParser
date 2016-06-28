using Common;
using MetaLanguageParser.MetaCode;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.eTypeType;
using static MetaLanguageParser.Resources.ResourceReader;

namespace MetaLanguageParser.Parsing
{


    public enum eTypeType
    {
        /// <summary> Encapsulated DataObject</summary>
        Class,
        /// <summary> Class generated from §programm statement. </summary>
        EntryClass,
        /// <summary> Abstract Template Type</summary>
        Interface,
        /// <summary>User-defined Valuetype</summary>
        Struct,
        /// <summary>Enumeration (List of Name-value Pairs / Flags</summary>
        Enum,
    }
    public class TypeData
    {

        eTypeType typeType;
        string name;

        // Generics

        // Some kind of option for C Style Output, etc...

        // Something holding the TypeLayout
        // Order of Members

        // List of Fields
        // List of Methods (Ctors seperate?)
        List<MethodData> methods = new List<MethodData>();

        // InnerTypes?

    }
}
