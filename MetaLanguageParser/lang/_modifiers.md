# Access/Scope #
How the Type/Member can be accessed
## Public ##
By everyone
## FamORAsm ##
Within its class, derived classes in any assembly, and any class in the same assembly
## Assembly ##
Within its class, and any class in the same assembly
## Family ##
Within the family of its class and derived classes
## FamANDAsm ##
Within its class, and derived classes in the same assembly
## Private ##
Only inside the class

# Modifier #
## Abstract ##
[Types] State that this type contains abstract methods and therefore can't create an instance of it.
[Methods] State that this method contains no implementation, which will instead be supplied by derived classes.
* Usually, interface members are implicit abstract.
## Const ##
[Fields] Static + Readonly.
* Enumeration members are per definition implicit Constant.
## Final / Readonly ##
[Fields] Allows fields to be assigned only once, and only in Constructors.
## Sealed ##
Opposite of Abstract
[Methods] Cannot be overriden in a derived class
[Types] Prevent extension of this type
## Static ##
Static TypeMember, in contrast to instance
[Types] Some languages allow static types, which explicit requires all members to be "public static" as well.
* C# for example can use Static imports to add all members of said Import to the private scope of the destination.
## Override ##
[Methods] Overrides implementation of a base class member. Required if base is virtual
## Virtual ##
[Methods] For languages where overrideable methods have to be explicit stated as such.
* C# allows the use of virtual to explicit allow overriding, otherwise it (the override) would only have effect in privateScope.
* VB.Net disallows overriding except if decorated with this.
## New ##
[Members] Explicit state that this hides a base member of the same name.

# Keywords to use
* Access/Scope
	* Public	public
	* FamORAsm	shared, nonpublic
	* Assembly	assembly
	* Family	protected, family
	* FamANDAsm	internal
	* Private	private
* Modifier
	* Abstract	abstract
	* Constant	const
	* Final 	final, readonly
	* Sealed	sealed
	* Static	static
	* Override	override
	* Virtual	virtual
	* New   	new

Empty List to copy (make it a code section)

## Access/Scope
Public	
FamORAsm	
Assembly	
Family	
FamANDAsm	
Private	
## Modifier
Abstract	
Constant	
Final	
Sealed	
Static	
Override	
Virtual	
New	
