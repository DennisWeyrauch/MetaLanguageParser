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
        #region 2-Invoke Method (7651/7670 in 123)
        /// <summary> </summary>
        /// <typeparam name="T">The return type of the to be invoked method</typeparam>
        /// <param name="type">The type containing <paramref name="method"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <returns></returns>
        public T invokeMethod<T>(Type type, string method) => invokeMethod<T>(type, method, false, null);
        /// <summary> </summary>
        /// <typeparam name="T">The return type of the to be invoked method</typeparam>
        /// <param name="type">The type containing <paramref name="method"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <param name="isStatic">Whether or not the method is static</param>
        /// <returns></returns>
        public T invokeMethod<T>(Type type, string method, bool isStatic) => invokeMethod<T>(type, method, isStatic, null);
        /// <summary> </summary>
        /// <typeparam name="T">The return type of the to be invoked method</typeparam>
        /// <param name="type">The type containing <paramref name="method"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <param name="args">Parameter array to supply the method with. Default null (no parameters)</param>
        /// <returns></returns>
        public T invokeMethod<T>(Type type, string method, object[] args) => invokeMethod<T>(type, method, false, args);
        /// <summary>
        /// Invoke the given method in type.<para/>
        /// Use this to change the state of static members. Won't work with abstract or open-generic members.
        /// </summary>
        /// <typeparam name="T">The return type of the to be invoked method. Use Reflection.TVoid for void</typeparam>
        /// <param name="type">The type containing <paramref name="method"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <param name="isStatic">Whether or not the method itself is static</param>
        /// <param name="args">Parameter array to supply the method with. Default null (no parameters)</param>
        /// <exception cref="TargetException"><paramref name="isStatic"/> is true but the invocated method isn't static</exception>
        /// <exception cref="ArgumentException">Parameter types do not match</exception>
        /// <exception cref="TargetParameterCountException">Wrong parameter signature</exception>
        /// <exception cref="TargetInvocationException">Invoked member threw an exception</exception>
        /// <exception cref="MethodAccessException">Instance or caller has no permission to access method</exception>
        /// <exception cref="InvalidOperationException">Declaring type is an open generic type</exception>
        /// <returns>The output of type <typeparamref name="T"/> of the invoked method</returns>
        public T invokeMethod<T>(Type type, string method, bool isStatic, object[] args)
        {
            if (typeof(T) == typeof(TVoid)) {
                if (isStatic) type.GetMethod(method, LookupStatic).Invoke(null, args);
                return default(T);
            } else if (isStatic) {
                return (T)type.GetMethod(method, LookupStatic).Invoke(null, args);
            } else {
                var obj = Activator.CreateInstance(type, true);
                return (T)type.GetMethod(method, LookupAll).Invoke(obj, args);
            }
        }
		
        /// <summary>
        /// Invoke the given method in type.<para/>
        /// Use this to change the state of static members. Won't work with abstract or open-generic members.
        /// </summary>
        /// <typeparam name="T">The return type of the to be invoked method. Use Reflection.TVoid for void</typeparam>
        /// <param name="type">The type containing <paramref name="method"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <param name="isStatic">Whether or not the method itself is static</param>
        /// <param name="args">Parameter array to supply the method with. Default null (no parameters)</param>
        /// <param name="refArgs">Reference parameters. Will overwrite changed elements. </param>
        /// <param name="outArgs">Out parameters. Will overwrite changed elements. </param>
        /// <exception cref="TargetException"><paramref name="isStatic"/> is true but the invocated method isn't static</exception>
        /// <exception cref="ArgumentException">Parameter types do not match</exception>
        /// <exception cref="TargetParameterCountException">Wrong parameter signature</exception>
        /// <exception cref="TargetInvocationException">Invoked member threw an exception</exception>
        /// <exception cref="MethodAccessException">Instance or caller has no permission to access method</exception>
        /// <exception cref="InvalidOperationException">Declaring type is an open generic type</exception>
        /// <returns>The output of type <typeparamref name="T"/> of the invoked method</returns>
        //public T invokeMethod<T>(Type type, string method, bool isStatic, ref object[] args, bool[] refArgs. bool[] outArgs)
		public T invokeMethod2<T>(Type type, string method, bool isStatic, ref object[] args)
        {
			/*if((refArgs == null || refArgs.Length == 0 ) && (outArgs == null || outArgs.Length == 0))
				return invokeMethod<T>(type, method, isStatic, args);
			if(rerArgs.Length != args.Length || outArgs.Length != args.Length) throw new TargetParameterCountException("Wrong number of ref/out values");//*/
			T retVal;
			object[] locArg = new object[args.Length];
			for(int i=0;i<args.Length;i++){
				locArg[i] = args[i];
			}
			/*for(int i=0;i<args.Length;i++){ // Fuck it. Don't care about refNulls
				if(i<refArgs.Length && )
			}//*/
			// if ref==true and args == null -> ERROR
			// Ref parameter just use the initial value AND replace it eventually 
			// out parameter will store their value in the array again.
			
            if (typeof(T) == typeof(TVoid)) {
                if (isStatic) type.GetMethod(method, LookupStatic).Invoke(null, locArg);
                retVal = default(T);
            } else if (isStatic) {
                retVal = (T)type.GetMethod(method, LookupStatic).Invoke(null, locArg);
            } else {
                var obj = Activator.CreateInstance(type, true);
                retVal = (T)type.GetMethod(method, LookupAll).Invoke(obj, locArg);
            }
			for(int i=0;i<args.Length;i++){
				args[i] = locArg[i];
			}
			return retVal;
        }
		public class InvokeArg {
			public object arg;
			public bool isRef;
			public bool isOut;
		}

        /// <summary>
        /// Trigger an Invocation of the given method onto the given instance.<para/>
        /// Using the instance ensures that abstracts and generics are properly closed.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance</typeparam>
        /// <typeparam name="TReturn">The return type of the to be invoked method. Use <see cref="TVoid"/> for Void.</typeparam>
        /// <param name="obj">An instance of type <typeparamref name="TInstance"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <returns></returns>
        public TReturn invokeMethod<TReturn, TInstance>(TInstance obj, string method)
            => invokeMethod<TReturn, TInstance>(obj, method, null);
        /// <summary>
        /// Trigger an Invocation of the given method onto the given instance.<para/>
        /// This is to use for "return void", derivates of abstract and generic types, 
        /// as they have unresolveable constructors on their own.
        /// </summary>
        /// <typeparam name="TInstance">The Type of the instance</typeparam>
        /// <typeparam name="TReturn">The return type of the to be invoked method. Use TVoid for void</typeparam>
        /// <param name="obj">An instance of type <typeparamref name="TInstance"/>.</param>
        /// <param name="method">The name of the method to invoke</param>
        /// <param name="args">Parameter array to supply the method with. Default null (no parameters)</param>
        /// <exception cref="TargetException"><paramref name="obj"/> is null</exception>
        /// <exception cref="ArgumentException">Parameter types do not match</exception>
        /// <exception cref="TargetParameterCountException">Wrong parameter signature</exception>
        /// <exception cref="TargetInvocationException">Invoked member threw an exception</exception>
        /// <exception cref="MethodAccessException">Instance or caller has no permission to access method</exception>
        /// <exception cref="InvalidOperationException"><paramref name="obj"/> is an open generic type</exception>
        /// <returns>The output of type <typeparamref name="TReturn"/> of the invoked method</returns>
        public TReturn invokeMethod<TReturn, TInstance>(TInstance obj, string method, params object[] args)
        {
            if (obj == null) throw new TargetException();
            var t = obj.GetType();
            MethodInfo mi = FindMethod(obj.GetType(), method, args);
            if (typeof(TReturn) == typeof(TVoid)) {
                mi.Invoke(obj, args);
                return default(TReturn);
            }
            return (TReturn)mi.Invoke(obj, args);
        }
        /*
#warning Tested: No overload  // works
#warning Tested: overload, args.Length = 0  // 2015-10-20
#warning Tested: Overload, args.Length > 0  // 2015-10-20
        //*/
        static string lastMatch = "";
        /// <summary>
        /// Find a given member "<paramref name="method"/>(<paramref name="args"/>)" in the type <paramref name="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        MethodInfo FindMethod(Type t, string method, object[] args)
        {
            try {
                return t.GetMethod(method, LookupAll);
            } catch (AmbiguousMatchException) {
                //Console.WriteLine("Overlo(a|r)d found!");
                if (!lastMatch.Equals(method)) {
                    Console.WriteLine($"Ambiguous match found for {method}!");
                    lastMatch = method;
                }
            }

            // Get all ambiguous matches.
            MemberInfo[] mem = t.GetMember(method, LookupAll);
            /// Ensures that NoPara == NoPara & prevents inner for from starting
            Type[] argt = null;
            int len = (args != null && (argt = Type.GetTypeArray(args)) != null) ? args.Length : 0;
            MethodInfo mi = null;
            bool match = false;
            for (int i = 0; i < mem.Length; i++) {
                var x = invokeMethod<Type[], MethodBase>(mem[i] as MethodBase, "GetParameterTypes");
                if (x?.Length == len) {
                    match = true;
                    for (int p = 0; match && p < len; p++) {
                        if (x[p] != argt[p]) match = false;
                    }
                }
                if (match) {
                    mi = mem[i] as MethodInfo;
                    break;
                }
            }
            return mi;
        }
        /// <summary>
        /// Find a given member "<paramref name="method"/>(<paramref name="args"/>)" in the type <paramref name="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public MethodInfo FindMethod(Type t, string method, params Type[] args)
        {
            try {
                return t.GetMethod(method, LookupAll);
            } catch (AmbiguousMatchException ame) {
                //Console.WriteLine("Overlo(a|r)d found!");
                Console.WriteLine(ame.Message + $" for {method}");
            }

            // Get all ambiguous matches.
            MemberInfo[] mem = t.GetMember(method, LookupAll);
            /// Ensures that NoPara == NoPara & prevents inner for from starting
            int len = (args != null) ? args.Length : 0;
            MethodInfo mi = null;
            bool match = false;
            for (int i = 0; i < mem.Length; i++) {
                var x = invokeMethod<Type[], MethodBase>(mem[i] as MethodBase, "GetParameterTypes");
                if (x?.Length == len) {
                    match = true;
                    for (int p = 0; match && p < len; p++) {
                        if (x[p] != args[p]) match = false;
                    }
                }
                if (match) {
                    mi = mem[i] as MethodInfo;
                    break;
                }
            }
            return mi;
        }

        // Get internal types
        //var sigType = Assembly.Load("mscorlib.dll").GetType("System.Signature");

        /// <summary>
        /// Helper class to invoke void-returning methods
        /// </summary>
        public sealed class TVoid { }
        #endregion
    }
}
