using Common;
using MetaLanguageParser.Operands;
using MetaLanguageParser.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MetaLanguageParser.Resources.ResourceReader;

namespace MetaLanguageParser.MetaCode
{
    public partial class CodeBase
    {
        /// <summary>
        /// Contains:
        ///     readConditional
        ///     readAnyCode
        ///     readStatements
        ///     readParameterList
        /// </summary>


        #region Conditional
        /// <summary>
        /// Read an expression that resolves to a boolean value.
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected static string readConditional(ref ExeBuilder eb, ref int pos)
        {
            var expr = eb.codeBase.Peek().readCondExpr(ref eb.list);// That would be the point where the custom print would take action
            if (expr.GetType() != typeof(bool)) throw new InvalidSyntaxException("Expression didn't resolve to boolean");
            return expr.ToString();
        }

        static int condDepth = 0;

        /// <summary>
        ///  Recursive Call for <see cref="readConditional(ref ExeBuilder, ref int)"/>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private Op readCondExpr(ref ListWalker list)
        {
            var elem = list.getCurrent();
            if (elem.EqualsChar('(')) elem = list.getNext();
            Operation expr = null;
            Op temp = null;// new Operation(null);
            bool unary = false;
            bool binaryRight = false;
            // Assign the operation type
            Operands.OperatorType opType = OperatorType.Constant;
            if(condDepth == 0) {
                expr = readOperator(elem, OperatorType.Conditional);
            } else expr = readOperator(elem, OperatorType.Any);
            opType = expr._nodeType;
            condDepth++;
            // Act depending on Nonary, Unary, or Binary operand

            //if (opType == ConditionalType.Constant) return expr;
            try {
                while (true) {
                    elem = list.getNext();
                    // 1. If it's a nested expr, make recursive call
                    if (elem.Equals("("))
                        temp = readCondExpr(ref list); // ((Binary)expr).setLeft(myFunc2(list));
                    // 2. -OR- Test if a literal/primitive token
                    else if (elem.IsNumeric()) temp = parseNumeric(elem); 
                        // 3. -OR- Figure out reference token
                        else {
                        string s = elem;
                        // Lookup in method/variable/etc dict (would read methCall in case it is one)
                        if (resolveIfExist(elem, out s)) {
                            temp = new Value(s);
                            (temp as Value).setLocal();
                        }
                        // Else just default to plain text.
                        temp = new Value(elem);
                    }
                        if (binaryRight) break;
                        binaryRight = true;
                        expr.setLeft(temp);
                        if (expr is Operation.UnaryBase) return expr;
                    
                }
            expr.setRight(temp);
                return expr;
            } finally {
                list.getNext();
                condDepth--;
                //if(list.getCurrent().EqualsChar(')')) list.getNext();
            }
        }
        #endregion

        #region Read CodeBlocks
        /// <summary>
        /// A quick handle for skipping code bodies. Skips all tokens until '}' is found (pos=='}') and returns "Console.Write(\"Hello World\");"
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns>A string containing "Console.Write(\"Hello World\");"</returns>
        static internal string readAnyCode(ref ExeBuilder eb, ref int pos)
        {
#warning CUSTOM:: BlockTerminator
#warning readAnyCode: Add balancing and Skipping of BlockItems inside of strings

            int cnt = 1;
            string elem = "";
#warning CUSTOM:: Parameterlist-Seperator, Closure-Opening/Terminator, replace with skipBalanced
            while (cnt > 0) {
                elem = eb.list.getNext();
                if (elem.EqualsChar('{')) cnt++;
                else if (elem.EqualsChar('}')) cnt--;
            }

            //while (eb.list.whileNot(ref elem, '}')) ;
            return "Console.Write(\"Hello World\");";
        }

        /// <summary>
        /// Read one line of code. For now nothing more than a call to <see cref="Parser.execStatement(ref ExeBuilder, ref int)"/>
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        static internal string readStatements(ref ExeBuilder eb, ref int pos)
        {
            string elem = "";
            var output = new System.IO.StringWriter();
            var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, __INDENT);
            var par = Parser.getInstance;
            while (!eb.list.isCurrent('}')) writer.Write(par.execStatement(ref eb, ref pos));
            elem = output.ToString();
            writer.Dispose();
            output.Dispose();
            return elem;
        }

        /// <summary>
        /// Read a syntactic code unit
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        static internal string readExpression(ref ExeBuilder eb, ref int pos)
        {
            // Stuff like +, -, lambda, methodCall, etc....
            throw new NotImplementedException();
        }

        #endregion
#warning DUMMY:: Prints everything as plaintext
        /// <summary>
        /// Rudimentary print of paramList: Including the MethBracket, stops when () are balanced OR ';' is found.<para/>
        /// ENTRY: on '(' ## EXIT: On ')'
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        static private string readParameterList(ref ExeBuilder eb, ref int pos)
        {
            var sb = new StringBuilder( );
            int cnt = 1;
            string elem = "";
#warning CUSTOM:: Parameterlist-Seperator, ClosureOpening/Terminator, StatementTerminator
            while (cnt > 0) {
                elem = eb.list[pos];
                if (elem.EqualsChar('(')) cnt++;
                else if (elem.EqualsChar(')')) cnt--;
                if (cnt != 0) sb.Append(elem);
                if (elem.Equals(";")) cnt = 0;
                pos++;
            }
            pos--;
            return sb.ToString();
        }

        static Operation readOperator(string elem, OperatorType opType)
        {
#warning Change to OperatorType
            Operation expr = null;
            switch (opType) {
                case OperatorType.Invalid: throw new InvalidOperationException("Invalid Operation");
                case OperatorType.Any:
                    if(ResourceReader.opBinDict.TryGetValue(elem, out expr)) {
                        return expr.getNew();
                    } else return ResourceReader.opArithDict[elem].getNew();
                case OperatorType.Conditional:
                    return ResourceReader.opBinDict[elem].getNew();
                case OperatorType.Arithmetic:
                    return ResourceReader.opArithDict[elem].getNew();
                case OperatorType.Constant:
                    //case "true": expr = new Value(true, ConditionalType.Constant); break;
                    //case "false": expr = new Unary(false, ConditionalType.Constant); break;
                    break;
                default:
                    break; throw new NotImplementedException("CodeBase. (): Unimplemented case" + opType.ToString());
            }
            /*switch (elem[0]) {
                case '+': break;
                case '-': break;
                case '*': break;
                case '/': break;
                case '%': break;
                case '&': break;
                case '|': break;

                case '<': break;
                case '>': opType = Operands.ConditionalType.GreaterThan; break;
                //case '': break;
                default:
                    switch (elem) {
                        case "§preInc": break;
                        case "§preDec": break;
                        case "§postInc": break;
                        case "§postDec": break;
                        case "&&": break;
                        case "||": break;
                        case "==": break;
                        case "!=": break;

                        case ">=": break;
                        case "<=": break;

                        case "<<": break;
                        case ">>": break;
                    }
                    throw new NotImplementedException("CodeBase. (): Unimplemented case" + elem.ToString());
            }//*/

            return expr;
        }
    }
}
