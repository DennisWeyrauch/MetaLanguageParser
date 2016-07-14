## Integer ##
Int8	sbyte
UInt8	byte
Int16	short
UInt16	ushort
Int32	int
UInt32	uint
Int64	long
UInt64	ulong
## Floating Point ##
Float32	float
Float64	double
Float128	decimal
## Text ##
Char	char
#WChar	
String	string
#WString	
## Other ##
Bool	bool
Reference	object
#Time	
#Date	
DateTime	DateTime
## Of course, any Type defined additionally in /meta/_types.txt weill be mapped here as well
# List	...
# Map	...

§§Raw
## Add overrides for specific Literals.
# Default for everything is {0} (incl. Strings)
#Int8	{0}
#UInt8	{0}
#Int16	{0}S
#UInt16	{0}US
#Int32	{0}
#UInt32	{0}UI
Int64	{0}L
#UInt64	{0}UL
Float32	{0}F
#Float64	{0}R
#Float128	{0}D
Char	'{0}'
String	"{0}"
#True	true
#False	false
#DateTime	#{MM}/{D}/{YYYY} {HH}:{mm}:{SS} {AM|PM}#
## Define the constructor call (if any)
#Reference	new {0}
#Reference	new {0}()
