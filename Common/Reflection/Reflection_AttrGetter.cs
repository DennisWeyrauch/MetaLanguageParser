using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reflection
{
    public partial class Reflection
    {
        #region GetAttributes

        static string __getAttributes(MemberInfo mem, string i)
        {
            string prefix = $"\r\n{i}{i}{i}";
            string sumH = $"{prefix}<summary>", sumT = "</summary>"; //"";// 
            string attrH = $"{prefix}<attr>", attrT = "</attr>"; //"";// 
            var memI = new List<string>();
            string a;
            sneakMember(ref memI, mem, out a);
            var joiner = string.Join(" | ", memI.ToArray());
            var sb = new StringBuilder();
            if (joiner.IsNotNOE()) {
                sb.Append(sumH).Append(joiner).Append(sumT);
            }
            sb.Append(attrH).Append(a).Append(attrT);
            //sb.Append(prefix).Append(STUFF(mem.GetType()));
            return sb.ToString();
        }
        /// <summary>
        /// Get full MemberInformation (Access, Types, Attributes)
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string getAttributes(MemberInfo mem, string prefix)
        {
            string sumH = $"<summary>", sumT = "</summary>"; //"";// 
            string attrH = $"<attr>", attrT = "</attr>"; //"";// 
            var memI = new List<string>();
            string a;
            sneakMember(ref memI, mem, out a);
            var joiner = string.Join(" | ", memI.ToArray());
            var sb = new StringBuilder();
            if (joiner.IsNotNOE()) {
                sb.Append(sumH).Append(joiner).Append(sumT);
            }
            sb.Append(attrH).Append(a).Append(attrT);
            //sb.Append(prefix).Append(STUFF(mem.GetType()));
            return sb.ToString();
        }

        /// <summary>
        /// Print the type with all attributes.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string sneakType(Type t, string prefix)
        {
            var list = new List<string>();
            string generics = "";
            #region Type
#if false // Everything covered by Attributes
            // // Visibility Mask (0x07) // 0x00 -- 0x07 //
            if (t.IsNotPublic) list.Add("Not Public"); // Visibility Mask
            else if (t.IsPublic) list.Add("Public");   // Visibility Mask
            // (RuntimeType.IsVisible | IsGenericParameter | GetElementType.IsVisible | (if Nested get Base).IsPublic
            if (t.IsVisible) list.Add("Visible outside ASM");
            if (t.IsNested) {  // DeclaringType != null
                var sb = new System.Text.StringBuilder("Nested Type");
                if (t.IsNestedPublic) sb.Append(" (Public)");
                if (t.IsNestedPrivate) sb.Append(" (Private)");
                if (t.IsNestedFamily) sb.Append(" (Protected)");
                if (t.IsNestedAssembly) sb.Append(" (Internal)");
                if (t.IsNestedFamANDAssem) sb.Append(" (Protected AND Internal)");
                if (t.IsNestedFamORAssem) sb.Append(" (Protected OR Internal)");
                list.Add(sb.ToString());
            }

            // // TypeAttributes.LayoutMask (0x18) // 0x00, 0x08, 0x10 //
            if (t.IsAutoLayout) list.Add("Layout = Automatic");
            if (t.IsExplicitLayout) list.Add("Layout = Explicit specific offsets");
            if (t.IsLayoutSequential) list.Add("Layout = Sequential");

            // // ClassSemanticsMask (0x20) // 0x00, 0x20 //
            if (t.IsClass) list.Add("Class"); // ClassSemanticsMask
            else if (t.IsInterface) list.Add("Interface"); // (RuntimeType.IsInterface() | ClassSemanticsMask)
            
            if (t.IsEnum) list.Add("Enum"); // IsSubClassOf(RuntimeType.EnumType)
            if (t.IsValueType) list.Add("Value type"); // IsValueImpl() --> IsSubClassOf(RumTimeType.ValueType)

            // // Special semantics // 0x80, 0x100, 0x400 //
            if (t.IsAbstract && t.IsSealed) list.Add("Static"); // Implied with Abstract+Sealed (Before changing everything, why not using an otherwise invalid state?)
            else if (t.IsAbstract) list.Add("Abstract");  // TypeAttributes.Abstract
            else if (t.IsSealed) list.Add("Sealed");      // TypeAttributes.Sealed
            if (t.IsSpecialName) list.Add("SpecialName"); // TypeAttributes.SpecialName

            if (t.IsImport) list.Add("Import"); // TypeAttributes.Import
            if (t.IsSerializable) list.Add("Serializeable"); // TypeAttributes.Serializeable | RuntimeType.IsSpecial...()

            // StringFormatMask
            if (t.IsAutoClass) list.Add("StrFormat = Auto"); 
            else if (t.IsAnsiClass) list.Add("StrFormat = Ansi");
            else if (t.IsUnicodeClass) list.Add("StrFormat = Unicode");

#else
            // (RuntimeType.IsVisible | IsGenericParameter | GetElementType.IsVisible | (if Nested get Base).IsPublic
            if (t.IsVisible) list.Add("Visible outside ASM");
            if (t.IsEnum) list.Add("Enum");
            if (t.IsAbstract && t.IsSealed) list.Add("Static"); // Implied with Abstract+Sealed (Before changing everything, why not using an otherwise invalid state?)
            if (t.IsValueType) list.Add("Value type");
#endif
            #region Indexing and Adresses
            // (IsArray | IsPointer | IsByRef)
            if (t.HasElementType) {
                list.Add("Reference type");
                if (t.IsArray) { // IsArrayImpl
                    int dim = t.GetArrayRank(); // Get number of dimensions
                    list.Add($"Array ({dim})");
                }
                if (t.IsByRef) list.Add("ByRef"); // IsByRefImpl
                if (t.IsPointer) list.Add("Pointer"); // IsPointerImpl
            }

            if (t.IsContextful) list.Add("Context"); // Can be hosted in a context
            if (t.IsMarshalByRef) list.Add("Marshalled by Reference");
            if (t.IsPrimitive) list.Add("Primitive type"); // IsPrimitiveImpl
            if (t.IsCOMObject) list.Add("COM object"); // IsCOMObjectImpl
            #endregion

            // GetRootElemType.Con... | IsGenericParameter | (IsGenericType | GetGenArgs(a -> a.ContainsGeneric)
            if (t.ContainsGenericParameters) {
                // $"{prefix}<typeparam>{generics}</typeparam>"
                var sb = new System.Text.StringBuilder("Generic Type");
                if (t.IsGenericTypeDefinition) { // --> Open Generic
                    sb.Append(" (Definition)");
                    var sb2 = new System.Text.StringBuilder();
                    dynamic gen = t;
                    foreach (var arg in gen.GenericTypeParameters) { // Whereever it is, atm it exists
                        sb2.Append($"{prefix}<typeparam>");
                        sb2.Append($"[{arg.GenericParameterPosition}] {arg.Name}");
                        var x = arg.GenericParameterAttributes.ToString();
                        if (!x.Equals("None")) { sb2.Append($" ({x})"); }
                        sb2.Append("</typeparam>");
                    }
                    generics = sb2.ToString();
                }
                if (t.IsConstructedGenericType) {
                    sb.Append(" (Constructed)");
                    generics = $"{prefix}<typeparam>{printGeneric(t)}</typeparam>";
                }
                if (t.IsGenericParameter) sb.Append(" (Parameter)");
                list.Add(sb.ToString());
            }

            //if (x.IsAbstract) list.Add("");
            #endregion

            var joiner = string.Join(" | ", list.ToArray());
            var esb = new System.Text.StringBuilder( );
            if (joiner.IsNotNOE()) { esb.Append($"{prefix}<type>{joiner}</type>"); }
            if (generics.IsNotNOE()) { esb.Append(generics); }
            esb.Append($"{prefix}<attr>{t.Attributes.ToString()}</attr>");

            return esb.ToString(); //"";// 
        }
        static void sneakMember(ref List<string> list, MemberInfo mi, out string attrInfo)
        {
            //var attrInfo = "";
            attrInfo = "";
            //var retInfo = "";

            #region MemberInfo
            // MemberType
            // String Name
            // Type DeclaringType
            // Object[] GetCustomAttributes
            #endregion
            #region Methods  (MemberInfo <- MethodBase <- MethodInfo, ConstructorInfo)
            MethodBase m;
            if ((m = mi as MethodBase) != null) {
                MethodAttributes myAttr = m.Attributes;
                // list.Add("Attributes = " + myAttr.ToString());
                attrInfo = myAttr.ToString();
                // IsPublic/Pri/Fam/Asm/Fam&Asm/FamOrAsm/Static
                // Static/Final/Virtual/HideBySig/Abstract/SpecialName/Constructor
                if (m.IsConstructor) list.Add("Constructor");
                if (m.IsGenericMethod) list.Add("GenericMethod");
                if (m.IsGenericMethodDefinition) list.Add(" (Definition)");
                // MethodInfo
                if (m as MethodInfo != null) {
                    var meth = m as MethodInfo;
                    Type retType = meth.ReturnType;
                    //list.Add("ReturnType = " + retType.Name);
                    ParameterInfo retPar = meth.ReturnParameter;
                }
            }
            #endregion
            #region Field    (MemberInfo <- FieldInfo)
            FieldInfo f;
            if ((f = mi as FieldInfo) != null) {
                Type myType = f.FieldType;
                FieldAttributes myAttr = f.Attributes;
                //list.Add("Type = " + myType.Name); // Covered by f.FullName
                //list.Add("Attributes = " + myAttr.ToString());
                attrInfo = myAttr.ToString();
                // IsPublic/Pri/Fam/Asm/Fam&Asm/FamOrAsm/Static
                // Static/InitOnly/Literal/NotSerialized/SpecialName / PinvokeImpl
                if (f.IsInitOnly) list.Add("Readonly"); // Can only be set in ctor body
                if (f.IsLiteral) list.Add("Const"); // Written at compile time, cannot be changed
                if (f.IsNotSerialized) list.Add("Not Serialized");
            }
            #endregion
            #region Property (MemberInfo <- PropertyInfo) [Ref--> MethodInfo]
            PropertyInfo p;
            if ((p = mi as PropertyInfo) != null) {
                Type myType = p.PropertyType;
                PropertyAttributes myAttr = p.Attributes; // SpecialName
                list.Add("Type = " + myType.Name);
                // list.Add("Attributes = " + myAttr.ToString());
                attrInfo = myAttr.ToString();
                // MethodInfo[] GetAccessors(bool nonPublic = false)
                // Alt.: GetGetMethod / GetSetMethod
                ParameterInfo[] idxPara = p?.GetIndexParameters();
                if (p.CanRead) list.Add("Read-Access");
                if (p.CanWrite) list.Add("Write-Access");
                if (p.IsSpecialName) list.Add("Special Name");
            }
            #endregion
            #region Event    (MemberInfo <- EventInfo) [Ref--> MethodInfo]
            EventInfo e;
            if ((e = mi as EventInfo) != null) {
                MethodInfo m_add = e.GetAddMethod();
                MethodInfo m_rem = e.GetRemoveMethod();
                MethodInfo m_raise = e.GetRaiseMethod();
                Type myType = e.EventHandlerType;
                EventAttributes myAttr = e.Attributes;
                list.Add("Type = " + myType.Name);
                // list.Add("Attributes = " + myAttr.ToString());
                attrInfo = myAttr.ToString();
                if (e.IsSpecialName) list.Add("Special Name");
                if (e.IsMulticast) list.Add("Multicast");
            }
            #endregion
            #region Parameter

            Func<ParameterInfo, object> myFunc = ((ParameterInfo para) => {
                ParameterAttributes paraAttr = para.Attributes;
                // IN, OUT, RetVal(= ref, out), Optional
                int pos = para.Position;
                MemberInfo myMem = para.Member;
                Type myType = para.ParameterType;
                return null;
            });
            #endregion
        }
        #endregion
    }
}
