using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
//using static ILExtensions.ILGen_Ext;

namespace MetaLanguageParser.Operands
{
    using Resources;
    using System.Collections.Generic;    //using static ConditionalType;
    using static OperatorType;
    /// <summary>
    /// /SEE REGEX FILE
    /// </summary>
    public enum OperatorType
    {
        Invalid, Any,
        Conditional, Arithmetic, Constant,
    }

    //[System.Diagnostics.DebuggerDisplay("{this.GetType().Name} {_left?._nodeType}, {_right?._nodeType}, {_nodeType}")]

    /// <summary>
    /// ConditionalBuilder to construct and emit expressions.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{getDebug()}")]
    public abstract class Operation : Op {
        public Op _left { get; protected set; }
        public Op _right { get; protected set; }
        private Op _expr;
        protected static int _depth = 0;
        public readonly string _name;

        protected Operation() { }
        protected Operation(Op exp) { this._expr = exp; }
        protected Operation(string name) { _name = name; }
        public void setLeft(Op l) => _left = l;
        public void setRight(Op r) => _right = r;

        /// <summary>
        /// Abstract method to decomplicate the serialization of the OperatorDictionaries.
        /// </summary>
        /// <returns></returns>
        public abstract string getXmlBase();

        public static Type getOpType(string aryty, eOpDictType opType)
        {
            switch (aryty) {
                case "unary": return (opType == eOpDictType.Arithmetic) ? typeof(ArithExpr.Unary) : typeof(CondExpr.Unary);
                case "binary": return (opType == eOpDictType.Arithmetic) ? typeof(ArithExpr.Binary) : typeof(CondExpr.Binary);
                default: throw new NotImplementedException("Operation.getOpType(): Unimplemented case " + aryty);
            }
        }
        public abstract Operation getNew();

        // [_nodeType/_name =(_nodeType/_name)= _nodeType/_name]
        public string getDebug(bool unary = false)
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().Name).Append(" [ ");
            if (_left == null) sb.Append("null");
            else sb.Append((_left._nodeType == Constant) ? ((Value)_left).getConstType() : $"{_left._nodeType.ToString()}/{((Operation)_left)._name}");
            sb.Append(" =( ");
            sb.Append(_nodeType).Append("/").Append(_name);
            if (unary) {
                return sb.Append(" )]").ToString();
            }
            sb.Append(" )= ");
            if (_right == null) sb.Append("null");
            else sb.Append((_right._nodeType == Constant) ? ((Value)_right).getConstType() : $"{_right._nodeType.ToString()}/{((Operation)_right)._name}");
            return sb.Append(" ]").ToString();
        }

        // Consider skipping the above thing and directly go with String/Enum?
        public static Operation createFrom(Type t, string str)
        {
            //Console.WriteLine($"CreateFrom: {t.FullName}");
            //Console.WriteLine($"CreateFrom: {t.Name.ToCharArray()} == Unary:" +  t.Name == "Unary");
            //switch (t.Name) {
            //case nameof(UnaryBase):

            if (t.Name.Equals("Unary")){
                var un = t.GetConstructor(new[] { typeof(Op), typeof(string) }).Invoke(new object[] { null, str });// as CondExpr.Unary;
                if (t.FullName.Contains(nameof(CondExpr))) return un as CondExpr.Unary;
                else if (t.FullName.Contains(nameof(ArithExpr))) return un as ArithExpr.Unary;
                // break;
                //case nameof(BinaryBase):
            } else if (t.Name.Equals("Binary")){
                var bin = t.GetConstructor(new[] { typeof(Op), typeof(Op), typeof(string) }).Invoke(new object[] { null, null, str });// as CondExpr.Unary;
                    //if (t.Namespace.Contains("CondExpr")) return bin as CondExpr.Binary;
                    if (t.FullName.Contains(nameof(CondExpr))) return bin as CondExpr.Binary;
                    else if (t.FullName.Contains(nameof(ArithExpr))) return bin as ArithExpr.Binary;
                    //break;
            }
            throw new NotImplementedException("Operation.createFrom(): Unimplemented case " + t.Name);
        }
        public static Operation createFrom2(Type t, string str)
        {
            Console.WriteLine($"{t.Name} == {nameof(CondExpr.Unary)}");
            var to_Op = typeof(Op);
            var to_str = typeof(string);
            switch (t.Name) {
                case "Unary":
                    var un = t.GetConstructor(new[] { to_Op, to_str }).Invoke(new object[] { null, str });// as CondExpr.Unary;
                    if (t.IsSubclassOf(typeof(CondExpr))) return un as CondExpr.Unary;
                    else if (t.IsSubclassOf(typeof(ArithExpr))) return un as ArithExpr.Unary;
                    break;
                case "Binary":
                    var bin = t.GetConstructor(new[] { to_Op, to_Op, to_str }).Invoke(new object[] { null, null, str });// as CondExpr.Unary;
                    //if (t.Namespace.Contains("CondExpr")) return bin as CondExpr.Binary;
                    if (t.IsSubclassOf(typeof(CondExpr))) return bin as CondExpr.Binary;
                    else if (t.IsSubclassOf(typeof(ArithExpr))) return bin as ArithExpr.Binary;
                    break;
            }
            throw new NotImplementedException("Operation.createFrom(): Unimplemented case " + t.Name);
        }

        [System.Diagnostics.DebuggerDisplay("{getDebug(true)}")]
        public abstract class UnaryBase : Operation
        {
            public UnaryBase(Op l, string name) : base(name)
            {
                this._left = l;
            }
            public override string getXmlBase() => "unary";

            /**
            public override string ToString()
            {
                string s = ""; // get the thing from somewhere
                _depth++;
                string result = string.Format(s, _left.ToString());
                _depth--;
                return (_depth == 0) ? result : $"({result})";
            }//*/
#if DEBUG
            //public new void setRight(Op r) { throw new InvalidOperationException("Can't set the right side of an Unary operation"); }
#endif
        }
        public abstract class BinaryBase : Operation
        {
            public BinaryBase(Op l, Op r, string name) : base(name)
            {
                this._left = l;
                this._right = r;
            }
            public override string getXmlBase() => "binary";
        }
        public override string ToString()
        {
            string s = ResourceReader.opDestDict[_name];
            _depth++;
            string result = string.Format(s, _left.ToString(), _right?.ToString());
            _depth--;
            return (_depth == 0) ? result : $"({result})";
        }
    }


    abstract class CondExpr : Operation
    {
        public interface ICondExpr { }
        //public override Type GetType() => typeof(bool);
        public class Unary : UnaryBase, ICondExpr //
        {
            public Unary(Op l, string name)/**/ : base(l, name) { _nodeType = Conditional; }
            public override Type GetType() => typeof(bool);
            public override Operation getNew() => new Unary(null, _name);
        }
        public class Binary : BinaryBase, ICondExpr // : ArithExpr
        {
            public Binary(Op l, Op r, string name)/**/ : base(l, r, name) { _nodeType = Conditional; }
            public override Type GetType() => typeof(bool);
            public override Operation getNew() => new Binary(null, null, _name);
        }
    }

    abstract class ArithExpr : Operation
    {
        public interface IArithExpr { }
        //public override Type GetType() => typeof(object);
        public class Unary : UnaryBase, IArithExpr //
        {
            public Unary(Op l, string name)/**/ : base(l, name) { _nodeType = Arithmetic; }
            public override Type GetType() => typeof(object);
            public override Operation getNew() => new Unary(null, _name);
        }
        public class Binary : BinaryBase, IArithExpr // : ArithExpr
        {
            public Binary(Op l, Op r, string name)/**/ : base(l, r, name) { _nodeType = Arithmetic; }
            public override Type GetType() => typeof(object);
            public override Operation getNew() => new Binary(null, null, _name);
        }
    }
#if false
    // If there will be no more then one of this type, then redirect to Binary
    public abstract class Unary2 : Operation
    {
        public Unary(Op l, ConditionalType node)
        {
            this._left = l;
            this._nodeType = node;
        }
        /*
        public override string ToString()
        {
            switch (_nodeType.As<eConditionalType>()) {
                case Negate: return $"!({_left.ToString()})";
                case Constant: return _left.ToString();
                default: throw new InvalidOperationException();
            }
        }//*/
    }

    public class Binary2 : CondExpr
    {
        public Binary2(Op l, Op r, ConditionalType node)
        {
            this._left = l;
            this._right = r;
            this._nodeType = node;
        }
        public void setLeft(Op l) => _left = l;
        public void setRight(Op r) => _right = r;

        public override string ToString()
        {
#warning CUSTOM:: Operator Symbols; Operation
            // Most have the usual balanced print, but some (Functional+My Meta) have it left-branch
            string s = "";
            _depth++;
            switch (_nodeType.As<eConditionalType>()) {
                case _And:               s = " & "; break;
                case _Or:                s = " | "; break;
                case Equal:              s = " == "; break;
                case NotEqual:           s = " != "; break;
                case GreaterThan:        s = " > "; break;
                case LessThan:           s = " < "; break;
                //case ExclusiveOr: // bitwise XOR         [f1  f2]
                case GreaterThanOrEqual: s = " >= "; break;
                case LessThanOrEqual:    s = " <= "; break;
                //case Negate: return $"!({_left.ToString()})";
                default: throw new NotImplementedException();
            }
            s = _left.ToString() + s + _right.ToString();
            _depth--;
            return (_depth == 0) ? s : $"({s})";
        }
    }
endif
    public class ShortCircuit : CondExpr
    {
        public new Op[] _left { get; protected set; }

        public static ShortCircuit ShortCircuit_AND(Op[] op)
            => new ShortCircuit(op[], AndAlso);
        public static ShortCircuit ShortCircuit_OR(Op[] op)
            => new ShortCircuit(op, OrElse);
        protected ShortCircuit(Op[] op, ConditionalType node)
        {
            this._left = op;
            this._nodeType = node;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(");
            string s = "";
            switch (_nodeType.As<eConditionalType>()) {
                case AndAlso: s = " && "; break;
                case OrElse:  s = " || "; break;
                default: throw new InvalidOperationException("Trying to use ShortCircuit with "+_nodeType);
            }
            sb.Append(_left[0].ToString()).Append(s);
            for (int i = 1; i < _left.Length; i++) {
                sb.Append(_left[i].ToString()).Append(s);
            }
            return sb.Append(")").ToString();
        }
    }
#endif
}


