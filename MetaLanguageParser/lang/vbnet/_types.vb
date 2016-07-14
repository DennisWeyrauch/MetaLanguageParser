## Integer ##
Int8	SByte
UInt8	Byte
Int16	Short
UInt16	UShort
Int32	Integer
UInt32	UInteger
Int64	Long
UInt64	ULong
## Floating Point ##
Float32	Single
Float64	Double
Float128	Decimal
## System.Char is a 16bit datatype ##
Char	Char
WChar	Char
String	String
WString	String
## Boolean is 4 bytes ##
Bool	Boolean
Reference	Object
#Time	???
#Date	???
DateTime	Date
## LiteralDefault:
# 42, 1000 --> Integer
# "Hello World", "A" --> String
# #7/4/1776 12:01:50 PM#

§§Raw
## Add overrides for specific Literals.
# Default for everything is {0} (incl. Strings)
#Int8	{0}
#UInt8	{0}
Int16	{0}S
UInt16	{0}US
#Int32	{0}
UInt32	{0}UI
Int64	{0}L
UInt64	{0}UL
Float32	{0}F
Float64	{0}R
Float128	{0}D
Char	"{0}"
String	"{0}"
#True	True
#False	False
DateTime	#{MM}/{D}/{YYYY} {HH}:{mm}:{SS} {AM|PM}#
Reference	New {0}
