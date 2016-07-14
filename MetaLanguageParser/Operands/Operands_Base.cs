using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Operands
{
    using Common;
    using static Convert;
    /**
    class Value
    {
        void m()
        {
            // Creating a parameter expression.
            ParameterExpression value = Expression.Parameter(typeof(int), "value");

            // Creating an expression to hold a local variable. 
            ParameterExpression result = Expression.Parameter(typeof(int), "result");

            // Creating a label to jump to from a loop.
            LabelTarget label = Expression.Label(typeof(int));

            // Creating a method body.
            BlockExpression block = Expression.Block(
                // Adding a local variable.
                new[] { result },
                // Assigning a constant to a local variable: result = 1
                Expression.Assign(result, Expression.Constant(1)),
                // Adding a loop.
                Expression.Loop(
                // Adding a conditional block into the loop.
                    Expression.IfThenElse(
                // Condition: value > 1
                        Expression.GreaterThan(value, Expression.Constant(1)),
                // If true: result *= value --
                        Expression.MultiplyAssign(result, Expression.PostDecrementAssign(value)),
                // If false, exit the loop and go to the label.
                        Expression.Break(label, result)
                    ),
                // Label to jump to.
                   label
                )
            );

            // Compile and execute an expression tree.
            int factorial = Expression.Lambda<Func<int, int>>(block, value).Compile()(5);

            Console.WriteLine(factorial);
            // Prints 120.
        }
    }
    //*/
    /// <summary>
    /// 
    /// </summary>
    public abstract class Op
    {
        public virtual OperatorType _nodeType { get; protected set; }
        // Unary: + - ! ~ ++ -- true false
        // Binary: + - * / % & | ^ << >>
        // Pairwise: == !=  < >  <= >=
        public abstract new Type GetType();
        /*
        public static Op operator !(Op l) => new Binary(l, new Value(0), ConditionalType.Equal);
        //public static Op operator true(Op b) => new Value(true);
        //public static bool operator false(Op b) => new Value(true);

        // Binary operators
        public static Op operator &(Op l, Op r) => new Binary(l, r, ConditionalType._And);
        public static Op operator |(Op l, Op r) => new Binary(l, r, ConditionalType._Or);

        // Comparison operators, only in pairs
        public static Op operator ==(Op l, Op r) => new Binary(l, r, ConditionalType.Equal);
        public static Op operator !=(Op l, Op r) => new Binary(l, r, ConditionalType.NotEqual);
        public static Op operator <(Op l, Op r) => new Binary(l, r, ConditionalType.LessThan);
        public static Op operator >(Op l, Op r) => new Binary(l, r, ConditionalType.GreaterThan);
        public static Op operator <=(Op l, Op r) => new Binary(l, r, ConditionalType.LessThanOrEqual);
        public static Op operator >=(Op l, Op r) => new Binary(l, r, ConditionalType.GreaterThanOrEqual);
        //*/

        /// <summary>Trigger with True/False</summary>
        public static implicit operator Op(bool value) => new Value(value);
        public static implicit operator Op(sbyte value) => new Value(value);
        public static implicit operator Op(int value) => new Value(value);
        public static implicit operator Op(long value) => new Value(value);
        public static implicit operator Op(float value) => new Value(value);
        public static implicit operator Op(double value) => new Value(value);
        public static implicit operator Op(string value) => new Value(value);
        public static implicit operator Op(LocalBuilder value) => new Value(value, false);
    }

    /// <summary>
    /// Operator type that represents a constant value.
    /// </summary>

    [System.Diagnostics.DebuggerDisplay("{_constType}: {_value.ToString()}")]
    public class Value : Op
    {

        enum ConstantType
        {
            Invalid, DBNull, IsTrue, IsFalse, Char, String,
            SByte, Integer, Long, Float, Double,
            Local, Parameter, Field, Method, Func
        }
        private readonly object _value;
        private readonly LocalBuilder _lb;
        ConstantType _constType;
        public string getConstType() => _constType.ToString();
        /// <summary>
        /// Static field for the null-Value
        /// </summary>
        public static Value nullValue = new Value() {_constType = ConstantType.DBNull };

        public delegate object EmitFunc(ref ILGenerator ilg);
        //emitFunc func = ((ref ILGenerator ilg) => null);
        private Value() { }
        private Value(ConstantType ct) { _constType = ct; }

        #region ValueTypes
        /// <summary>Create a boolean constant.</summary>
        /// <param name="b">The value to assign</param>
        public Value(bool b)
        {
            _constType = (b) ? ConstantType.IsTrue : ConstantType.IsFalse;
            _value = b;
        }
        public Value(sbyte i) {
            _constType = ConstantType.SByte;
            _value = i;
        }
        //public Value(byte i) { } // Auto convert
        //public Value(short i) { } // Auto convert
        /// <summary>Create a integer constant. (Applies to byte, short and int)</summary>
        /// <param name="i"></param>
        public Value(int i)
        {
                if (sbyte.MinValue <= i && i <= sbyte.MaxValue) {
                    _constType = ConstantType.SByte;
                } else {
                    _constType = ConstantType.Integer;
                }
                _value = i;
        }
        public Value(long l) {
            _constType = ConstantType.Long;
            _value = l;
        }
        public Value(float f)
        {
            _value = f;
            _constType = ConstantType.Float;
        }
        public Value(double d)
        {
            _value = d;
            _constType = ConstantType.Double;
        }
        public Value(char c)
        {
            _value = c;
            _constType = ConstantType.Char;
        }
        public Value(string s)
        {
            _value = s;
            _constType = ConstantType.String;
        }
        public void setLocal() => _constType = ConstantType.Local;
        #endregion
        #region MemberInfos
        //bool myFlag = false;
        /// <summary>Create a constant that represents a local variable.</summary>
        /// <param name="lb"></param>
        /// <param name="byRef">True to load the address instead.</param>
        public Value(LocalBuilder lb, bool byRef)
        {
            _lb = lb;
            _constType = ConstantType.Local;
        }
        /// <summary>Create a constant that represents an argument.</summary>
        /// <param name="pi"></param>
        /// <param name="byRef">True to load by reference.</param>
        public Value(ParameterInfo pi, bool byRef)
        {
            _value = pi;
            _constType = ConstantType.Parameter;
        }
        /// <summary>Create a constant that represents a field or event.</summary>
        /// <param name="fi"></param>
        /// <param name="isStatic">True to load a static field</param>
        /// <param name="byRef">True to load a the address of the fieldf</param>
        public Value(FieldInfo fi, bool isStatic) { }
        /// <summary>Create a constant that represents a method, constructor or accessor.</summary>
        /// <param name="fi"></param>
        /// <param name="isStatic">True to load a static method</param>
        public Value(MethodInfo mi, bool isStatic) { }
        #endregion


        #region Parse and TryParse
        /// <Summary>Converts the string representation of a number to its 32-bit signed integer equivalent.
        ///      A return value indicates whether the conversion succeeded.</Summary>
        /// <param name="elem">A string containing a number to convert.</param>
        /// <param name="val">When this method returns, contains the <see cref="Value"/> type equivalent
        ///      of the ValueType contained in <paramref name="elem"/>, if the conversion succeeded, or null if the conversion
        ///      failed. The conversion fails if the <paramref name="elem"/> parameter is null or <see cref="System.String.Empty"/>,
        ///      is not of the correct format. This parameter is passed uninitialized;
        ///      any value originally supplied in result will be overwritten.</param>
        /// <Returns>true if <paramref name="elem"/> was converted successfully; otherwise, false.</Returns>
        public static bool TryParse(string elem, out Op val, bool printError = true)
        {
            val = null;
            if (elem.IndexOfAny("+-*.=!§$%&/()[]{}<>?:".ToCharArray()) != -1) return false;
            //if (elem.StartsWith("'") || elem.StartsWith("\"") || elem.IsNumeric())
                try {
                    val = Parse(elem);
                } catch (Exception e) { if(printError)Logger.LogNonFatalException(e); }
            return val != null;
        }

        public static bool IsValue(string elem) => elem.StartsWith("'") || elem.StartsWith("\"") || elem.Equals("null") || elem.IsNumeric();


        /// <Summary>Converts the value of the specified string to its equivalent Unicode character.</Summary>
        /// <param name="elem">A string that contains a primitive Value, or null.</param>
        /// <Returns>A <see cref="Value"/> type object containing the ValueType from <paramref name="elem"/>.</Returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="elem"/> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="elem"/> contains no valid ValueType, Unicode character, or String.</exception>
        public static Value Parse(string elem)
        {
            if (elem == null) throw new ArgumentNullException(elem);
            if (string.IsNullOrWhiteSpace(elem)) throw new ArgumentException("Is Empty", elem);
            if (elem.StartsWith("'")) return new Value(Char.Parse(elem.Substring(1, elem.Length-2)));
            if (elem.StartsWith("\"")) return new Value(elem.Substring(1, elem.Length - 2));
            if (elem.Equals("null")) return nullValue;
            // DateTime
            int i; if (Int32.TryParse(elem, out i)) return new Value(i);
            long l; if (Int64.TryParse(elem, out l)) return new Value(l);
            double d; if (Double.TryParse(elem, out d)) return new Value(d);
            float f; if (Single.TryParse(elem, out f)) return new Value(f);
            bool b; if (Boolean.TryParse(elem, out b)) return new Value(b);
            throw new ArgumentException("Not a valid ValueType!", elem);
        }

        /// <Summary>Converts the string representation of a number to its <see cref="Value"/> equivalent.
        ///      A return value indicates whether the conversion succeeded.</Summary>
        /// <param name="elem">A string containing a number to convert.</param>
        /// <param name="val">When this method returns, contains the numeric <see cref="Value"/> type equivalent
        ///      of the ValueType contained in <paramref name="elem"/>, if the conversion succeeded, or null if the conversion
        ///      failed. The conversion fails if the <paramref name="elem"/> parameter is null or <see cref="System.String.Empty"/>,
        ///      is not of the correct format, or represents a number less than <see cref="System.Int64.MinValue"/>
        ///      or greater than <see cref="System.Int64.MaxValue"/>. This parameter is passed uninitialized;
        ///      any value originally supplied in result will be overwritten.</param>
        /// <Returns>true if <paramref name="elem"/> was converted successfully; otherwise, false.</Returns>
        public static bool TryParseNumeric(string elem, out Op val)
        {
            val = null;
            if (elem.IsNumeric()) {
                val = parseNumeric(elem);
            }
            return val != null;
        }

        /// <see cref="Common.Extensions.IsNumeric(object)"/>
        public static Value parseNumeric(string elem)
        {
            int i; if (Int32.TryParse(elem, out i)) return new Value(i);
            long l; if (Int64.TryParse(elem, out l)) return new Value(l);
            double d; if (Double.TryParse(elem, out d)) return new Value(d);
            float f; if (Single.TryParse(elem, out f)) return new Value(f);
            bool b; if (Boolean.TryParse(elem, out b)) return new Value(b);
            //decimal dec; if (Decimal.TryParse(elem, out dec)) return new Value(dec);

            throw new ArgumentException("Not a valid numeric ValueType!", elem);
        }

        #endregion

        public override OperatorType _nodeType => OperatorType.Constant;

        public override Type GetType()
        {
            switch (_constType) {
                case ConstantType.Invalid: throw new InvalidOperationException("Invalid Value");
                case ConstantType.DBNull: return typeof(object);
                case ConstantType.IsTrue:
                case ConstantType.IsFalse: return typeof(bool);
                case ConstantType.Char:
                case ConstantType.String:
                case ConstantType.SByte:
                case ConstantType.Integer:
                case ConstantType.Long:
                case ConstantType.Float:
                case ConstantType.Double: return _value.GetType();
                case ConstantType.Local: return _lb.LocalType;
                case ConstantType.Parameter: 
                case ConstantType.Field:  
                case ConstantType.Method:
                default: throw new NotImplementedException($"Value.GetType(): Unimplemented case '{_constType.ToString()}'");
           }
        }



        public static void setTypeDict(Dictionary<string, string> dict) => typeDict = dict;
        static Dictionary<string,string> typeDict;// = new Dictionary<string,string>();
        public override string ToString()
        {
            string str = _value.ToString();
            switch (_constType) {
                /**/case ConstantType.Local: //return $"ldloc '{_lb.ToString()}'";
                    //return _value.ToString();
                case ConstantType.SByte:
                case ConstantType.Float:
                case ConstantType.Double:
                case ConstantType.Long:
                case ConstantType.Integer:
                    return _value.ToString();//*/
                case ConstantType.Char:
                case ConstantType.String:
                    var ct = _constType.ToString().ToLower();
                    return (typeDict.ContainsKey(ct)) ? string.Format(typeDict[ct], str) : str;
                case ConstantType.IsFalse: return "false";
                case ConstantType.IsTrue: return "true";
#warning IMPLEMENT MISSING _constType Cases
                default:
                    //return _value.ToString();//
                    throw new NotImplementedException($"Value.ToString(): Unimplemented case '{_constType.ToString()}'");
            }
            return str;
            //return base.ToString();
        }


        //public static implicit operator Op(Value v)  => v as Op;
    }
}
