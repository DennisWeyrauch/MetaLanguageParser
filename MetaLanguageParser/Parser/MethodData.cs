using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parser.eMethodType;
using static MetaLanguageParser.Resources.ResourceReader;


namespace MetaLanguageParser.Parser
{
    public enum eMethodType
    {
        Method, EntryMethod, Constructor, TypeConstructor, Destructor
    }
    class MethodData
    {
        eMethodType methodType;
        MethodAttributes attr;

        bool hasReturn;
        MetaType retType;

        string name;
        bool hasGeneric;
        MetaType generic;

        Dictionary<string, MetaType> args = new Dictionary<string, MetaType>();
        Dictionary<string, MetaType> locals = new Dictionary<string, MetaType>();

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
            if not Class | Struct | Interface then return
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
        }

        public static void OutputMemberScopeModifier(MemberAttributes attributes)
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
        }

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
