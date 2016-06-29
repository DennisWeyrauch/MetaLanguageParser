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
    // Debug: methodType + name + "( args:" + args.Count + ", local: " + local.Count +")"
    public class MethodData : MetaData
    {
//#warning Should the Type be stored here as well?
        //TypeData enclosedType;

        eMethodType methodType;
        internal void setMain()
        {
            methodType = eMethodType.EntryMethod;
            this.Name = "Main";
        }


        //MethodAttributes attr;


        bool hasReturn;
        MetaType retType;

        public string Name { get; protected set; }
        bool hasGeneric;
        MetaType generic;

        Dictionary<string, MetaType> args = new Dictionary<string, MetaType>();
        Dictionary<string, LocalData> locals = new Dictionary<string, LocalData>();
        internal void addLocal(LocalData data) => locals.Add(data.Name, data);

        string code;

        public bool isSigOnly()
        {
            return (attr & MemberAttributes.Abstract) == MemberAttributes.Abstract;
        }

        internal void setCode(string code) {
            if (code.IsNotNOE()) {
                this.code = code;
                if (code.EndsWith("\r\n\r\n")) code = code.Remove(code.Length - 2);
            } else throw new InvalidOperationException("Already added MethodCode of " + Name);
        }

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

            if (locals.Count > 0) {

                var sb = new StringBuilder();
                foreach (var item in locals) sb.AppendLine(item.Value.ToString());
#warning If option "Seperate them" is true....
                if (false) {
                    foreach (var item in locals) sb.AppendLine(item.Value.getAssign());
                }
                code = sb.Append(code).ToString();
                if (Program.printParts) {
                    //Program.printer("method_"+Name, new StringBuilder(sb.ToString()).AppendLine().AppendLine("########").Append(code).ToString());
                    Program.printer("method_" + Name, code);
                }
            }
            switch (methodType) {
                case eMethodType.Method:
                    break;
                case eMethodType.EntryMethod: return GenerateAsEntryMethod();
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
        #region Generator Methods
        // MemberAccess = public, private, protected, internal, public
        // MemberScope = abstract, static, virtual, override
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
            // Read §main Signature
            return new CodeBase().buildCode(new CodeBase().readFile("§main"), new Dictionary<string, string>() { { "code", code }});
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
        #endregion
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

        /*/
        CodeParameterDeclarationExpression = FieldDirection dir, string Name, CodeTypeReference Type
            //*/

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
