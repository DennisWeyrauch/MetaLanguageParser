﻿using Common;
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


    [System.Diagnostics.DebuggerDisplay("{typeType} - {Name}: Meth({methods.Count})")]
    public class TypeData
    {
        public string debugDisplay() => $"{typeType} - {Name}: Meth({methods.Count})";

        eTypeType typeType;
        public string Name { get; protected set; }
        
        public TypeData(eTypeType entryClass, string v) : base()
        {
            this.typeType = entryClass;
            this.Name = v;
        }

        public static TypeData setMain()
        {
            return new TypeData(eTypeType.EntryClass, "Program");
        }
        string stuff;
        public void setStuff(string ret) => stuff = ret;

        List<string> modifiers = new List<string>();
        public List<string> getModifiers() => modifiers;
        internal void setMods(Stack<string> mods)
        {
            this.modifiers = mods.ToList();
        }

        /// Generics
        /// 
        /// Some kind of option for C Style Output, etc...
        /// Something holding the TypeLayout
        /// 
        /// Order of Members
        /// 
        /// List of Fields
        /// List of Methods (Ctors seperate?)
        List<MethodData> methods = new List<MethodData>();
        public List<MethodData> getMethods() => methods;

        internal void AddMethod(MethodData data)
        {
            methods.Add(data);
        }



        internal static eTypeType getModeEnum(string v)
        {
            switch (v) {
                case "class": return eTypeType.Class;
                default: throw new NotImplementedException("TypeData.getModeEnum(): Unimplemented case " + v.ToString());
            }
        }
        internal string getMode() => typeType.ToString().ToLower();


        public override string ToString()
        {
            switch (typeType) {
                //case eTypeType.Class: break;
                case eTypeType.EntryClass: return buildAsEntryType();
                //case eTypeType.Interface:  break;
                //case eTypeType.Struct: break;
                //case eTypeType.Enum:  break;
                default: throw new NotImplementedException("TypeData.ToString(): Unimplemented case " + typeType.ToString());
            }
        }

        string buildAsEntryType()
        {
            var dict = new Dictionary<string, string>();
            var meth = new StringBuilder();
            bool first = true;

            /// Methods ///
            foreach (var item in methods) {
                if (first) first = false;
                else meth.AppendLine();
                meth.AppendLine(item.ToString());
            }
            dict.Add("methods", meth.ToString());


            // Read §mainType Signature
            return new CodeBase().buildCode(new CodeBase().readFile("§mainType"), dict);
        }

    }
}
