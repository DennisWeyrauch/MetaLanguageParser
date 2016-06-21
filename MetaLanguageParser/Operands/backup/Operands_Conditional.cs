using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
//using static ILExtensions.ILGen_Ext;

namespace MetaLanguageParser.Operands
{
    //using static ConditionalType;
    using static eConditionalType;
    //using static OpCodes;

    //[System.Diagnostics.DebuggerDisplay("{this.GetType().Name} {_left?._nodeType}, {_right?._nodeType}, {_nodeType}")]

    /// <summary>
    /// ConditionalBuilder to construct and emit expressions.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{getDebug()}")]
    public class CondExpr : Op {
        public Op _left { get; protected set; }
        public Op _right { get; protected set; }
        private Op _expr;
        protected static int _depth = 0;

        protected CondExpr() { }
        public CondExpr(Op exp) { this._expr = exp; }

        public override Type GetType() => typeof(bool);
        public string getDebug()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().Name).Append(" [ ");
            if (_left == null) sb.Append("null");
            else sb.Append((_left._nodeType == Constant) ? ((Value)_left).getConstType() : _left._nodeType.ToString());
            sb.Append(", ");
            if (_right == null) sb.Append("null");
            else sb.Append((_right._nodeType == Constant) ? ((Value)_right).getConstType() : _right._nodeType.ToString());
            sb.Append(", ").Append(_nodeType).Append(" ]");
            return sb.ToString();
        }
    }
    // If there will be no more then one of this type, then redirect to Binary
    public class Unary : CondExpr
    {
        public Unary(Op l, ConditionalType node)
        {
            this._left = l;
            this._nodeType = node;
        }

        public override string ToString()
        {
            switch (_nodeType.As<eConditionalType>()) {
                case Negate: return $"!({_left.ToString()})";
                case Constant: return _left.ToString();
                default: throw new InvalidOperationException();
            }
        }
    }

    public class Binary : CondExpr
    {
        public Binary(Op l, Op r, ConditionalType node)
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
    
    public class ShortCircuit : CondExpr
    {
        public new Op[] _left { get; protected set; }

        public static ShortCircuit ShortCircuit_AND(Op[] l, Op r)
            => new ShortCircuit(l, r, AndAlso);
        public static ShortCircuit ShortCircuit_OR(Op[] l, Op r)
            => new ShortCircuit(l, r, OrElse);
        protected ShortCircuit(Op[] l, Op r, ConditionalType node)
        {
            this._left = l;
            this._right = r;
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
            for (int i = 0; i < _left.Length; i++) {
                sb.Append(_left[i].ToString()).Append(s);
            }
            return sb.Append(_right.ToString()+")").ToString();
        }
    }
}
