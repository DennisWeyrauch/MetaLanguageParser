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
        ///     <see cref="readConditional"/> + <see cref="readArithmetic"/>
        ///     <see cref="readOperator"/> + <see cref="parseNumeric(string)"/>
        ///     <see cref="readAnyCode"/>
        ///     <see cref="readStatements"/>
        ///     <see cref="readParameterList"/>
        /// </summary>


        #region Conditional & Arithmetic Expressions
        /// <summary>
        /// Read an expression that resolves to a boolean value.
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected static string readConditional(ref ExeBuilder eb, ref int pos)
        {
            var expr = eb.codeBase.Peek().readCondExpr(ref eb.list, OperatorType.Conditional);// That would be the point where the custom print would take action
            if (expr.GetType() != typeof(bool)) throw new InvalidSyntaxException("Expression didn't resolve to boolean");
            return expr.ToString();
        }
        /// <summary>
        /// Read an expression that resolves to an value (in contrast to a reference).
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected static string readArithmetic(ref ExeBuilder eb, ref int pos)
        {
            Op expr = null;
            if (!TryParseNumeric(eb.list.getCurrent(), out expr)) {
                expr = eb.codeBase.Peek().readCondExpr(ref eb.list, OperatorType.Any);
                //if (expr.GetType() != typeof(bool)) throw new InvalidSyntaxException("Expression didn't resolve to boolean");
            } else pos++;
            return expr.ToString();
        }

        static int condDepth = 0;

        /// <summary>
        ///  Recursive Call for <see cref="readConditional(ref ExeBuilder, ref int)"/>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private Op readCondExpr(ref ListWalker list, OperatorType lookup)
        {
            var elem = list.getCurrent();
            // In case of nested / simple closure, skip the Brace
            if (elem.EqualsChar('(')) elem = list.getNext();
            Operation expr = null;
            Op temp = null;
            bool unary = false;
            bool binaryRight = false;

            // Assign the operation type
            if (elem.IsNumeric()) {
                return parseNumeric(elem);
            }
            expr = readOperator(elem, lookup);
            
            condDepth++;
            // Act depending on Nonary, Unary, or Binary operand
            
            try {
                // 0. Confirmed that Operator is not a literal
                while (true) {
                    elem = list.getNext(); // Get the Operand
                    // 1. If it's a nested expr, make recursive call
                    if (elem.Equals("("))
                        temp = readCondExpr(ref list, OperatorType.Any); // ((Binary)expr).setLeft(myFunc2(list));
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

        #region CodeElements
        static Operation readOperator(string elem, OperatorType opType)
        {
#warning Change to OperatorType
            Operation expr = null;
            switch (opType) {
                case OperatorType.Invalid: throw new InvalidOperationException("Invalid Operation");
                case OperatorType.Any:
                    if (ResourceReader.opBinDict.TryGetValue(elem, out expr)) {
                        return expr.getNew();
                    } else return ResourceReader.opArithDict[elem].getNew();
                case OperatorType.Conditional:
                    return ResourceReader.opBinDict[elem].getNew();
                case OperatorType.Arithmetic:
                    return ResourceReader.opArithDict[elem].getNew();
                case OperatorType.Constant:
                    //case "true": expr = new Value(true, ConditionalType.Constant); break;
                    //case "false": expr = new Unary(false, ConditionalType.Constant); break;
                    //break;
                default:
                    break; throw new NotImplementedException("CodeBase.readOperator(): Unimplemented case " + opType.ToString());
            }
            return expr;
        }
        

        static internal bool TryParseNumeric(string elem, out Op val)
        {
            val = null;
            if (elem.IsNumeric()) {
                val = parseNumeric(elem);
            }
            return val != null;
        }

        /// <see cref="Common.Extensions.IsNumeric(object)"/>
        static internal Value parseNumeric(string elem)
        {
            int i;
            long l;
            //decimal dec;
            float f;
            double d;
            bool b;
            if (Int32.TryParse(elem, out i)) return new Value(i);
            if (Int64.TryParse(elem, out l)) return new Value(l);
            //if (Decimal.TryParse(elem, out dec)) return new Value(dec);
            if (Double.TryParse(elem, out d)) return new Value(d);
            if (Single.TryParse(elem, out f)) return new Value(f);
            if (Boolean.TryParse(elem, out b)) return new Value(b);
            throw new NotImplementedException("Invalid Numeric Type in parseNumeric");
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
            while (!eb.list.isCurrent('}')) writer.Write(Parser.execStatement(ref eb, ref pos));
            elem = output.ToString();
            writer.Dispose();
            output.Dispose();
            return elem;
        }
        

        /// <summary>
        /// Read a syntactic code unit. For now, only Values/Operations supported
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <remarks>
        /// Expression is one of the following things
        /// - A Value, which can be
        /// -- A literal (0, 2.0, -4, 'c', "text", true)
        /// -- A Conditional expression resolving to a boolean value
        /// -- An Arithmetic expression
        /// -- A Ternary
        /// -- A lambda expression
        /// - A Reference
        /// -- this, null, new(), or new[] {}
        /// -- Method calls
        /// -- Any Local/Field/Property in scope (or via Proxy)
        /// - An Assignment with Closure and again an Expression of its own: (i = ....)
        /// </remarks>
        static internal string readExpression(ref ExeBuilder eb, ref int pos)
        {
            string result = "";

            // Blablbalblablabla
            result = readArithmetic(ref eb, ref pos);
            //pos++;
            return result;

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

    }
}
