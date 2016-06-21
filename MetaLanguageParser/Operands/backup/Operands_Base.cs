using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Operands
{
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
        public virtual ConditionalType _nodeType { get; protected set; }
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
    /* Unsigned versions:
	Add_Ovf, Sub_Ovf, Mul_Ovf, Div
	Bge, Bgt, Ble, Blt, Bne(Only unsigned)
	Cgt, Clt
	Conv_Ovf_I/U/R/(IU+1248)
	Rem, Shr
     //Used: And, Or, Ceq,Cgt,Clt; Neq->!Ceq, GE->!LT, LE->!GT
        //*/

    /// <summary>
    /// Operator type that represents a constant value. Contains predefinitions for
    /// <see cref="bool"/>, <see cref="Int32"/> (Int32), <see cref="LocalBuilder"/>, and argLess <see cref="OpCodes"/>.
    /// Everything else (incl. casting) has to be taken care of with a custom function.
    /// </summary>

    [System.Diagnostics.DebuggerDisplay("{_constType}: {_value.ToString()}")]
    public class Value : Op
    {

        enum ConstantType
        {
            Invalid, DBNull, IsTrue, IsFalse, String,
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

        
        public override ConditionalType _nodeType => ConditionalType.Constant;

        public override Type GetType()
        {
            switch (_constType) {
                case ConstantType.Invalid: throw new InvalidOperationException("Invalid Value");
                case ConstantType.DBNull: return typeof(object);
                case ConstantType.IsTrue:
                case ConstantType.IsFalse: return typeof(bool);
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


        public override string ToString()
        {
            switch (_constType) {
                case ConstantType.Local: //return $"ldloc '{_lb.ToString()}'";
                case ConstantType.SByte:
                case ConstantType.Float:
                case ConstantType.Double:
                case ConstantType.Long:
                case ConstantType.Integer:
                case ConstantType.String:
                    return _value.ToString();
                //case ConstantType.Func: return _emitFunc.ToString();
                case ConstantType.IsFalse: return "false";
                case ConstantType.IsTrue: return "true";
#warning IMPLEMENT MISSING _constType Cases
                default: throw new NotImplementedException($"Value.ToString(): Unimplemented case '{_constType.ToString()}'");
            }
            //return base.ToString();
        }
    }
}
