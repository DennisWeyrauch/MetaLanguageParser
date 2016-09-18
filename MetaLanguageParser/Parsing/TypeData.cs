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
using static MetaLanguageParser.Parsing.Adapter;

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

        string typeType;
        protected internal bool isEntryType = false;
        public string Name { get; protected set; }
        
        public TypeData(string mode, string v) : base()
        {
            this.typeType = mode;
            this.Name = v;
        }

        public static TypeData makeMain()
        {
            var td = new TypeData("<<<MAIN>>>", "Program");
            td.isEntryType = true;
            return td;
        }

        string stuff;
        public void setStuff(string ret) => stuff = ret;

        List<string> modifiers = new List<string>();
        public List<string> getModifiers() => modifiers;
        public void setMods(Stack<string> mods)
        {
            string temp;
            foreach (var item in mods) {
                if (!KEYWORD.modDict.TryGetValue(item, out temp)) temp = item;
                this.modifiers.Add(temp);
            }
        }

        // // KILL this enum
        /*internal static eTypeType getModeEnum(string v)
        {
            switch (v) {
                case "class": return eTypeType.Class;
                default: throw new NotImplementedException("TypeData.getModeEnum(): Unimplemented case " + v.ToString());
            }
        }//*/
        internal string getMode() => typeType.ToString().ToLower();


        #region Extension and Interfaces
        List<MetaType> listExtends = new List<MetaType>();
        public bool hasExt() => listExtends.Count > 0;
        public MetaType getExt(int idx = 0) => listExtends[idx];
        public List<MetaType> getExtList() => listExtends;
        public void addExt(MetaType mt) => listExtends.Add(mt);

        List<MetaType> listImpl = new List<MetaType>();
        public bool hasImpl() => listImpl.Count > 0;
        public MetaType getInterface(int idx = 0) => listImpl[idx];
        public List<MetaType> getInterfaces() => listImpl;
        public void addInterface(MetaType mt) => listImpl.Add(mt);
        #endregion

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



        public override string ToString()
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

            /*
            If EntryType not set, add Comment with all Moddifiers, Extends, Interfaces
            // Modifiers: XX, XX, 
            // Extends: XX
            // Interfaces: XX
            //###########
            
            // Build comments
            return strCmts + new CodeBase().buildCode(new CodeBase().readFile("§mainType"), dict);
            */
        }
    }
}
