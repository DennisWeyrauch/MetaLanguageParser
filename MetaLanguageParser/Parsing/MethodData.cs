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
using static MetaLanguageParser.Parsing.eMethodType;
using static MetaLanguageParser.Resources.ResourceReader;


namespace MetaLanguageParser.Parsing
{
    public enum eMethodType
    {
        Method, EntryMethod, Constructor, TypeConstructor, Destructor
    }
    public class MethodData
    {
        eMethodType methodType;
        internal void setMain()
        {
            methodType = eMethodType.EntryMethod;
        }


        //MethodAttributes attr;
        MemberAttributes attr;

        internal void addLocal(LocalData data)
        {
            throw new NotImplementedException();
        }

        bool hasReturn;
        MetaType retType;

        public string Name { get; private set; }
        bool hasGeneric;
        MetaType generic;

        Dictionary<string, MetaType> args = new Dictionary<string, MetaType>();
        Dictionary<string, LocalData> locals = new Dictionary<string, LocalData>();

        string code;

        public bool isSigOnly()
        {
            return (attr & MemberAttributes.Abstract) == MemberAttributes.Abstract;
        }

        internal void setCode(string code) {
            if (code.IsNOE()) {
                this.code = code;
            } else throw new InvalidOperationException("Already added MethodCode of " + Name);
        }


        Dictionary<string, MemberAttributes> attrDict = new Dictionary<string, MemberAttributes>() {
           {"assembly", MemberAttributes.Assembly },
            {"internal", MemberAttributes.FamilyAndAssembly },
            {"protected", MemberAttributes.Family },
            {"shared", MemberAttributes.FamilyOrAssembly },
            {"private", MemberAttributes.Private },
            {"public", MemberAttributes.Public },
            {"abstract", MemberAttributes.Abstract },
            {"final", MemberAttributes.Final },
            {"static", MemberAttributes.Static },
            {"override", MemberAttributes.Override },
            //{"virtual", MemberAttributes. },
        };
        Dictionary<string, MethodAttributes> attrDict2 = new Dictionary<string, MethodAttributes>() {
           {"assembly", MethodAttributes.Assembly },
            {"internal", MethodAttributes.FamANDAssem },
            {"protected", MethodAttributes.Family },
            {"shared", MethodAttributes.FamORAssem },
            {"private", MethodAttributes.Private },
            {"public", MethodAttributes.Public },
            {"abstract", MethodAttributes.Abstract },
            {"final", MethodAttributes.Final },
            {"static", MethodAttributes.Static },
            //{"override", MemberAttributes.Override },
            {"virtual", MethodAttributes.Virtual },
        };
        public void readSignature(ref Common.ListWalker list, ref int pos)
        {
            string elem = list.getCurrent();

            //MethodAttributes attr;
            MemberAttributes attr;
            while(attrDict.TryGetValue(elem, out attr)){
                this.attr |= attr;
                elem = list.getNext();
            }
            if(methodType != eMethodType.EntryMethod) {
            // if elem.equals(declaringType.Name) then 
            // if isStatic then cctor else ctor // or dtor? 
            // else 
                methodType = eMethodType.Method;
            }
            retType = readType(elem);//new LocalData(readType(elem), "<return>");
            //retType.dir = MetaType.enumLocalType.Return;
            Name = list.getNext();
            list.assertPreInc("("); // On (
            bool first = true;
            while(list.whileNot(ref elem, ')')) { //1. On ( -> TYPE1 != ), go into loop
                // 2nd: Name++ -> , != )
                // 3rd: Name2++ -> ) == )
                if (first) first = false;
                else elem = list.assert(","); // 2. is , --> TYPE2
                //FieldDirection parDir = ;//FieldDirection. // Readin Direction
                // elem = list.getNext();
                MetaType parType = readType(elem); // Read Type
                // Assign that FieldDirection
                string parName = list.getNext(); // list++, read Name
                args.Add(parName, parType);
            }
            // List is on )
            //* if InInterface OR abstract THEN 
            if((attr & MemberAttributes.Abstract) == MemberAttributes.Abstract) {
                list.assert(";");
            } else {
                //list.assert("{");
            }
        }
        
        private MetaType readType(string elem)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
			// Also something about an OOP Option
            //var output = new StringWriter();
            //var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, __INDENT);
        
            switch (methodType) {
                case eMethodType.Method:
                    break;
                case eMethodType.EntryMethod:
                    break;
                case eMethodType.Constructor:
                    break;
                case eMethodType.TypeConstructor:
                    break;
                case eMethodType.Destructor:
                    break;
                default:
                    throw new NotImplementedException("MethodData.ToString(): Unimplemented case " + methodType.ToString());
            }

            return base.ToString();
        }

        private string GenerateAsMethod()
        {
            /**
            if not Class | Struct | Interface then return // or throw exception?
            if list_customAttr.size > 0 THEN GenerateAttributes(custAttr)
            if list_retTypeCustAttr.size > 0 THEN GenerateAttributes(retTypeCustAttr)
            if (!IsCurrentInterface) {
                if(PrivateImplementationType == null){
                    OutputMemberAccess, OutputVTable, OutputMemberScope    
                }
            } else {
                OutputVTableModifier(attr)
            }
            OutputType(retType) --> Prints TypeName with Generics
            Write(" ")
            if(PrivateImplementationType != null){
                Write(GetBaseTypeOutput(PrivateImplementationType))
                Write(".")
            }
            OutputIdentifier(name)
            OutputTypeParameters
            Write("(")
            OutputParameters
            Write(")")
            OutputTypeParameterConstraints
            if(!IsInterface && attrNot(Attributes.Abstract)) {
                OutputStartingBrace
                Indent++
                PrintCode
                Indent--
                OutputClosingBrace --> newline + '}' + newline
            } else { 
                WriteLine(';')
            }
            //*/
            /**
            SHould work in form of returning a dictionary and then calling buildCode in the toString() method

            //*/

            return "";
        }

        private string GenerateAsEntryMethod()
        {
            return "";
        }
        private string GenerateAsConstructor()
        {
            return "";
        }
        private string GenerateAsTypeConstructor()
        {
            return "";
        }
        private string GenerateAsDestructor()
        {
            return "";
        }

    }
    public static class OutputHolder
    {
        private const int ParameterMultilineThreshold = 15;
        public static System.CodeDom.Compiler.IndentedTextWriter Output { get; private set; }

        // Just combine them all
        public static void OutputVTableModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.VTableMask) {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }
        }

        /// <summary>Generates code for the specified member access modifier.</summary>
        public static void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.AccessMask) {
                case MemberAttributes.Assembly:
                    Output.Write("internal ");
                    break;
                case MemberAttributes.FamilyAndAssembly:
                    Output.Write("internal ");  /*FamANDAssem*/
                    break;
                case MemberAttributes.Family:
                    Output.Write("protected ");
                    break;
                case MemberAttributes.FamilyOrAssembly:
                    Output.Write("protected internal ");
                    break;
                case MemberAttributes.Private:
                    Output.Write("private ");
                    break;
                case MemberAttributes.Public:
                    Output.Write("public ");
                    break;
            }

            switch (attributes & MemberAttributes.ScopeMask) {
                case MemberAttributes.Abstract:
                    Output.Write("abstract ");
                    break;
                case MemberAttributes.Final:
                    Output.Write("");
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Override:
                    Output.Write("override ");
                    break;
                default:
                    switch (attributes & MemberAttributes.AccessMask) {
                        case MemberAttributes.Family:
                        case MemberAttributes.Public:
                        case MemberAttributes.Assembly:
                            Output.Write("virtual ");
                            break;
                        default:
                            // nothing;
                            break;
                    }
                    break;
            }
        }

        /**public static void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.ScopeMask) {
                case MemberAttributes.Abstract:
                    Output.Write("abstract ");
                    break;
                case MemberAttributes.Final:
                    Output.Write("");
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Override:
                    Output.Write("override ");
                    break;
                default:
                    switch (attributes & MemberAttributes.AccessMask) {
                        case MemberAttributes.Family:
                        case MemberAttributes.Public:
                        case MemberAttributes.Assembly:
                            Output.Write("virtual ");
                            break;
                        default:
                            // nothing;
                            break;
                    }
                    break;
            }
        }//*/

        public static void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.ScopeMask) {
                case MemberAttributes.Final:
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Const:
                    Output.Write("const ");
                    break;
                default:
                    break;
            }
        }
        
        /// <summary>Generates code for the specified System.CodeDom.FieldDirection.</summary>
        public static void OutputDirection(FieldDirection dir)
        {
            switch (dir) {
                case FieldDirection.In:
                    break;
                case FieldDirection.Out:
                    Output.Write("out ");
                    break;
                case FieldDirection.Ref:
                    Output.Write("ref ");
                    break;
            }
        }

        /// <summary>Generates code for the specified parameters.</summary>
        public static void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            bool first = true;
            bool multiline = parameters.Count > ParameterMultilineThreshold;
            if (multiline) {
                Output.Indent += 3;
            }
            System.Collections.IEnumerator en = parameters.GetEnumerator();
            while (en.MoveNext()) {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression)en.Current;
                if (first) {
                    first = false;
                } else {
                    Output.Write(", ");
                }
                if (multiline) {
                    //ContinueOnNewLine("");
                }
                //GenerateExpression(current); // --> Typename, name, Direction, evtl. default or varArgs
            }
            if (multiline) {
                Output.Indent -= 3;
            }
        }
    }
}
