using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser;
using Operands;
using Common;

namespace MetaLanguageParser.MetaCode
{
    class CodeWhile : CodeBase, ICode
    {
        public string FileName => "while";
        public string Name => "While";
        public TypeCode x;


        string code => @"
while (i > 0) {
    i--;
    $$write(i)$$
}";
        string metacode => @"
while (> i 0) {
    (dec i);
    $$write(i)$$
}";


        string raw => @"
while ($$cond$$) {§inc
    $$code$$ §dec
}";

        List<string> readin = new List<string>() {
            "while (", "cond", ") {", "§inc", "§n", "code", "§dec", "§n", "}"
        };
        List<string> readin2 = new List<string>() {
            "Locals: int32 i",
            "Methods: §§",
            "while (",
            /* cond */ ">","i", "0", /* cond_End*/
            ") {", "§inc", "§n",
            /* code */
                 "(","dec", "i", ")", ";",
                 "§write", "(", "i", ")",
            /* code_End*/
            "§dec", "§n", "}"
        };


        public static Dictionary<string, string> dict;

        public string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;
            //var writer = eb.writer;
            //readin = readFile(ref eb, FileName); // eb since it contains the current LangInstance
            pos++; // Skip keyword.
            string elem = list[pos];
            list.assertC("("); // .assertC("$$cond$$")

            #region readConditional
            Func<ListWalker, string> readConditional = ((ListWalker) => {
                // Call internal Recursive Handle that returns a CondExpr, and when back, return with ToString()
                Func<ListWalker, CondExpr> readCondExpr = null;
                readCondExpr = ((ListWalker3) => {
                    //var opDict = new Dictionary<string, >
                    //elem = list[pos];
                    //if("<=>")
                    if(list.getCurrent().EqualsChar('(')) list.getNext();
                    CondExpr expr = null;// = new Operands.CondExpr();
                    Op temp = new CondExpr(null);
                    //Operands.Value val = new Value(null);
                    bool literal = false;
                    bool unary = false;
                    bool binaryRight = false;
                    var opDict = new Dictionary<string, ConditionalType>() {
                        {">", Operands.ConditionalType.GreaterThan }
                    };
                    expr = new Operands.Binary(null, null, opDict[list.getCurrent()]);
                    try
                      {
                        if(literal) return expr;

                        while(true) {
                            elem = list.getNext();
                            if(elem.Equals("(")) temp = readCondExpr(list); // ((Binary)expr).setLeft(myFunc2(list));
                            else if(elem.IsNumeric()) temp = parseNumeric(elem);
                            else {
                                string s = "";
                                if(resolveIfExist(elem, out s)) s = elem;
                                temp = new Value(s);
                            }
                            if(binaryRight) break;

                            binaryRight = true;
                            ((Binary)expr).setLeft(temp);
                            if(unary) return expr;
                        }
                    ((Binary)expr).setRight(temp);
                        return expr;
                    } finally {
#warning OR SOMETHING IN THE LIKE
                        if(list.getCurrent().EqualsChar(')')) list.getNext();
                    }
                });
                return readCondExpr(list).ToString();
            });
            #endregion
            dict.Add("cond", readConditional(list));

            list.assertC(")").assert("{");

            // COND
			
			Func<ExeBuilder, int, string> readCode = ((eb2, pos2) => {
				while (eb.list.whileNot(ref elem, '}')) ;
				return "Console.Write(\"Hello World\");";
			});
            dict.Add("code", readCode(eb, pos)); // Make them ref

            list.assert("}");
			Func<List<string>, Dictionary<string, string>, int, string> buildCode = ((list2, dict2, ebInt) => {
				/*List<string> readin2 = new List<string>() {
					"while (", "cond", ") {", "§inc", "§n", "code", "§dec", "§n", "}"
				};//*/
				using (var output = new System.IO.StringWriter())
				using (var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t")) {
					writer.Indent = ebInt;
					foreach (var item in readin) {
						switch (item) {
							case "§inc": writer.Indent++; continue;
							case "§dec": writer.Indent--; continue;
							case "§n": writer.WriteLine(); continue;
						}
						string s = "";
						if (dict.TryGetValue(item, out s)) {
							write.Write(s);
						} else writer.Write(item);
					}

					ebInt = writer.Indent;
					return output.ToString();
				}
			});
            return buildCode(readin, dict, eb.Indent); // Make the last ref
        }
    }
}
