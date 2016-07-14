The following types are predefined in the programm: (Invariant Case)
* Signed Integer: Int8, Int16, Int32, Int64
  * Alternative: sbyte, short, int, long
* Unsigned Integer: UInt8, UInt16, UInt32, UInt64
  * Alternative: byte, ushort, uint, ulong
* Decimals: Float32, Float64, Float128
  * Alternative: float/single, double, decimal
* Text: Char, String; WChar, WString
* Boolean: Bool
* Reference: Any/Object
* CompileTime Dynamic: var 
* RunTime Dynamic: dynamic (also called duckTyping)

Additional types can be declared in "/meta/_types.txt" in the same way as operators.

Excluding the Alternatives, you can use any type for the Mapping to Destination Type in "/myLang/_types.{lang}"