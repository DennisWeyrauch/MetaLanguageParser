//------------------------------------------------------------------------------
// <copyright file="CSharpCodeProvider.cs" company="Microsoft">
// 
// <OWNER>gpaperin</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Collections.Generic;
using System.Runtime.Versioning;
using CSharpCodeProvider;
using IndentedTextWriter = CSharpCodeProvider.IndentedTextWriter;

namespace Bogus//Microsoft.CSharp
{
    /// <devdoc>
    ///    <para>
    ///       CSharp Code Generator -- 03 Types
    ///    </para>
    /// </devdoc>
    internal partial class CSharpCodeGenerator2 : ICodeGenerator, ICodeCompiler
    {
        
        /// <devdoc>
        ///    <para> Generates code for the specified CodeDom namespace representation and the classes it
        ///       contains.</para>
        /// </devdoc>
        private void GenerateTypes(CodeNamespace e){
            foreach (CodeTypeDeclaration c in e.Types) {
                if (options.BlankLinesBetweenMembers) {
                    Output.WriteLine();
                }
                ((ICodeGenerator)this).GenerateCodeFromType(c, output.InnerWriter, options);
            }
        }
        void ICodeGenerator.GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o){
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException("Invalid Operation");
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                GenerateType(e);
            } finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }
        private void GenerateType(CodeTypeDeclaration e){
            currentClass = e;

            if (e.StartDirectives.Count > 0) {
                GenerateDirectives(e.StartDirectives);
            }

            GenerateCommentStatements(e.Comments);

            if (e.LinePragma != null) GenerateLinePragmaStart(e.LinePragma);

            GenerateTypeStart(e);

            if (Options.VerbatimOrder) {
                foreach (CodeTypeMember member in e.Members) {
                    GenerateTypeMember(member, e);
                }
            } else {
                GenerateFields(e);
                GenerateSnippetMembers(e);
                GenerateTypeConstructors(e);
                GenerateConstructors(e);
                GenerateProperties(e);
                GenerateEvents(e);
                GenerateMethods(e);
                GenerateNestedTypes(e);
            }
            // Nested types clobber the current class, so reset it.
            currentClass = e;

            GenerateTypeEnd(e);
            if (e.LinePragma != null) GenerateLinePragmaEnd(e.LinePragma);

            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }

        }

        private void GenerateTypeStart(CodeTypeDeclaration e){
            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (IsCurrentDelegate) {
                switch (e.TypeAttributes & TypeAttributes.VisibilityMask) {
                    case TypeAttributes.Public:
                        Output.Write("public ");
                        break;
                    case TypeAttributes.NotPublic:
                    default:
                        break;
                }

                CodeTypeDelegate del = (CodeTypeDelegate)e;
                Output.Write("delegate ");
                OutputType(del.ReturnType);
                Output.Write(" ");
                OutputIdentifier(e.Name);
                Output.Write("(");
                OutputParameters(del.Parameters);
                Output.WriteLine(");");
            } else {
                OutputTypeAttributes(e);
                OutputIdentifier(e.Name);

                OutputTypeParameters(e.TypeParameters);

                bool first = true;
                foreach (CodeTypeReference typeRef in e.BaseTypes) {
                    if (first) {
                        Output.Write(" : ");
                        first = false;
                    } else {
                        Output.Write(", ");
                    }
                    OutputType(typeRef);
                }

                OutputTypeParameterConstraints(e.TypeParameters);

                OutputStartingBrace();
                Indent++;
            }
        }
        private void GenerateTypeEnd(CodeTypeDeclaration e){
            if (!IsCurrentDelegate) {
                Indent--;
                Output.WriteLine("}");
            }
        }
        private void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType){

            if (options.BlankLinesBetweenMembers) {
                Output.WriteLine();
            }

            if (member is CodeTypeDeclaration) {
                ((ICodeGenerator)this).GenerateCodeFromType((CodeTypeDeclaration)member, output.InnerWriter, options);

                // Nested types clobber the current class, so reset it.
                currentClass = declaredType;

                // For nested types, comments and line pragmas are handled separately, so return here
                return;
            }

            if (member.StartDirectives.Count > 0) {
                GenerateDirectives(member.StartDirectives);
            }

            GenerateCommentStatements(member.Comments);

            if (member.LinePragma != null) {
                GenerateLinePragmaStart(member.LinePragma);
            }

            if (member is CodeMemberField) {
                GenerateField((CodeMemberField)member);
            } else if (member is CodeMemberProperty) {
                GenerateProperty((CodeMemberProperty)member, declaredType);
            } else if (member is CodeMemberMethod) {
                if (member is CodeConstructor) {
                    GenerateConstructor((CodeConstructor)member, declaredType);
                } else if (member is CodeTypeConstructor) {
                    GenerateTypeConstructor((CodeTypeConstructor)member);
                } else if (member is CodeEntryPointMethod) {
                    GenerateEntryPointMethod((CodeEntryPointMethod)member, declaredType);
                } else {
                    GenerateMethod((CodeMemberMethod)member, declaredType);
                }
            } else if (member is CodeMemberEvent) {
                GenerateEvent((CodeMemberEvent)member, declaredType);
            } else if (member is CodeSnippetTypeMember) {

                // Don't indent snippets, in order to preserve the column
                // information from the original code.  This improves the debugging
                // experience.
                int savedIndent = Indent;
                Indent = 0;

                GenerateSnippetMember((CodeSnippetTypeMember)member);

                // Restore the indent
                Indent = savedIndent;

                // Generate an extra new line at the end of the snippet.
                // If the snippet is comment and this type only contains comments.
                // The generated code will not compile. 
                Output.WriteLine();
            }

            if (member.LinePragma != null) {
                GenerateLinePragmaEnd(member.LinePragma);
            }

            if (member.EndDirectives.Count > 0) {
                GenerateDirectives(member.EndDirectives);
            }
        }

        private void GenerateFields(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberField) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberField imp = (CodeMemberField)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateField(imp);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateField(CodeMemberField e){
            if (IsCurrentDelegate || IsCurrentInterface) return;

            if (IsCurrentEnum) {
                if (e.CustomAttributes.Count > 0) {
                    GenerateAttributes(e.CustomAttributes);
                }
                OutputIdentifier(e.Name);
                if (e.InitExpression != null) {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }
                Output.WriteLine(",");
            } else {
                if (e.CustomAttributes.Count > 0) {
                    GenerateAttributes(e.CustomAttributes);
                }

                OutputMemberAccessModifier(e.Attributes);
                OutputVTableModifier(e.Attributes);
                OutputFieldScopeModifier(e.Attributes);
                OutputTypeNamePair(e.Type, e.Name);
                if (e.InitExpression != null) {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }
                Output.WriteLine(";");
            }
        }

        private void GenerateSnippetMembers(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            bool hasSnippet = false;
            while (en.MoveNext()) {
                if (en.Current is CodeSnippetTypeMember) {
                    hasSnippet = true;
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeSnippetTypeMember imp = (CodeSnippetTypeMember)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);

                    // Don't indent snippets, in order to preserve the column
                    // information from the original code.  This improves the debugging
                    // experience.
                    int savedIndent = Indent;
                    Indent = 0;

                    GenerateSnippetMember(imp);

                    // Restore the indent
                    Indent = savedIndent;

                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }

                }
            }
            // Generate an extra new line at the end of the snippet.
            // If the snippet is comment and this type only contains comments.
            // The generated code will not compile. 
            if (hasSnippet) {
                Output.WriteLine();
            }
        }
        private void GenerateSnippetMember(CodeSnippetTypeMember e){
            Output.Write(e.Text);
        }

        private void GenerateTypeConstructors(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeTypeConstructor) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeTypeConstructor imp = (CodeTypeConstructor)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateTypeConstructor(imp);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateTypeConstructor(CodeTypeConstructor e){
            if (!(IsCurrentClass || IsCurrentStruct)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            Output.Write("static ");
            Output.Write(CurrentTypeName);
            Output.Write("()");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }

        private void GenerateConstructors(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeConstructor) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeConstructor imp = (CodeConstructor)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateConstructor(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c){
            if (!(IsCurrentClass || IsCurrentStruct)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            OutputMemberAccessModifier(e.Attributes);
            OutputIdentifier(CurrentTypeName);
            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.Write(")");

            CodeExpressionCollection baseArgs = e.BaseConstructorArgs;
            CodeExpressionCollection thisArgs = e.ChainedConstructorArgs;

            if (baseArgs.Count > 0) {
                Output.WriteLine(" : ");
                Indent++;
                Indent++;
                Output.Write("base(");
                OutputExpressionList(baseArgs);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            if (thisArgs.Count > 0) {
                Output.WriteLine(" : ");
                Indent++;
                Indent++;
                Output.Write("this(");
                OutputExpressionList(thisArgs);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }

        private void GenerateProperties(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberProperty) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberProperty imp = (CodeMemberProperty)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateProperty(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c){
            if (!(IsCurrentClass || IsCurrentStruct || IsCurrentInterface)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (!IsCurrentInterface) {
                if (e.PrivateImplementationType == null) {
                    OutputMemberAccessModifier(e.Attributes);
                    OutputVTableModifier(e.Attributes);
                    OutputMemberScopeModifier(e.Attributes);
                }
            } else {
                OutputVTableModifier(e.Attributes);
            }
            OutputType(e.Type);
            Output.Write(" ");

            if (e.PrivateImplementationType != null && !IsCurrentInterface) {
                Output.Write(GetBaseTypeOutput(e.PrivateImplementationType));
                Output.Write(".");
            }

            if (e.Parameters.Count > 0 && String.Compare(e.Name, "Item", StringComparison.OrdinalIgnoreCase) == 0) {
                Output.Write("this[");
                OutputParameters(e.Parameters);
                Output.Write("]");
            } else {
                OutputIdentifier(e.Name);
            }

            OutputStartingBrace();
            Indent++;

            if (e.HasGet) {
                if (IsCurrentInterface || (e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract) {
                    Output.WriteLine("get;");
                } else {
                    Output.Write("get");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(e.GetStatements);
                    Indent--;
                    Output.WriteLine("}");
                }
            }
            if (e.HasSet) {
                if (IsCurrentInterface || (e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract) {
                    Output.WriteLine("set;");
                } else {
                    Output.Write("set");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(e.SetStatements);
                    Indent--;
                    Output.WriteLine("}");
                }
            }

            Indent--;
            Output.WriteLine("}");
        }

        private void GenerateEvents(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberEvent) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberEvent imp = (CodeMemberEvent)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateEvent(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c){
            if (IsCurrentDelegate || IsCurrentEnum) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (e.PrivateImplementationType == null) {
                OutputMemberAccessModifier(e.Attributes);
            }
            Output.Write("event ");
            string name = e.Name;
            if (e.PrivateImplementationType != null) {
                name = GetBaseTypeOutput(e.PrivateImplementationType) + "." + name;
            }
            OutputTypeNamePair(e.Type, name);
            Output.WriteLine(";");
        }

        private void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e){
            if (e.CustomAttributes.Count > 0) {
                // Parameter attributes should be in-line for readability
                GenerateAttributes(e.CustomAttributes, null, true);
            }

            OutputDirection(e.Direction);
            OutputTypeNamePair(e.Type, e.Name);
        }

        private void GenerateMethods(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberMethod
                    && !(en.Current is CodeTypeConstructor)
                    && !(en.Current is CodeConstructor)) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberMethod imp = (CodeMemberMethod)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    if (en.Current is CodeEntryPointMethod) {
                        GenerateEntryPointMethod((CodeEntryPointMethod)en.Current, e);
                    } else {
                        GenerateMethod(imp, e);
                    }
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }
        private void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c){
            if (!(IsCurrentClass || IsCurrentStruct || IsCurrentInterface)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            if (e.ReturnTypeCustomAttributes.Count > 0) {
                GenerateAttributes(e.ReturnTypeCustomAttributes, "return: ");
            }

            if (!IsCurrentInterface) {
                if (e.PrivateImplementationType == null) {
                    OutputMemberAccessModifier(e.Attributes);
                    OutputVTableModifier(e.Attributes);
                    OutputMemberScopeModifier(e.Attributes);
                }
            } else {
                // interfaces still need "new"
                OutputVTableModifier(e.Attributes);
            }
            OutputType(e.ReturnType);
            Output.Write(" ");
            if (e.PrivateImplementationType != null) {
                Output.Write(GetBaseTypeOutput(e.PrivateImplementationType));
                Output.Write(".");
            }
            OutputIdentifier(e.Name);

            OutputTypeParameters(e.TypeParameters);

            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.Write(")");

            OutputTypeParameterConstraints(e.TypeParameters);

            if (!IsCurrentInterface
                && (e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract) {

                OutputStartingBrace();
                Indent++;

                GenerateStatements(e.Statements);

                Indent--;
                Output.WriteLine("}");
            } else {
                Output.WriteLine(";");
            }
        }
        private void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c){

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            Output.Write("public static ");
            OutputType(e.ReturnType);
            Output.Write(" Main()");
            OutputStartingBrace();
            Indent++;

            GenerateStatements(e.Statements);

            Indent--;
            Output.WriteLine("}");
        }

        private void GenerateNestedTypes(CodeTypeDeclaration e){
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeTypeDeclaration) {
                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    CodeTypeDeclaration currentClass = (CodeTypeDeclaration)en.Current;
                    ((ICodeGenerator)this).GenerateCodeFromType(currentClass, output.InnerWriter, options);
                }
            }
        }
    }
}