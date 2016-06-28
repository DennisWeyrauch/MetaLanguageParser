using System; // AttributeTargets
using System.CodeDom; // MemberAttributes, FieldDirection
using System.Collections.Generic;
using System.Linq;
using System.Reflection; // Type/Method/Field/Event/Parameter/GenParameter --Attributes
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
{
    class AttrDisplay
    {        // Attributes of a Type
        TypeAttributes typeAttr;
        /** TypeAttributes Rollout
VisibilityMask     =      0000-0000-0000-0111 = 7
            0000 = NonPublic // Visibility == X
            0001 = Public // C --> Create H-File
            0010 = NestedPublic
            0011 = NestedPrivate
            0100 = NestedFamily
            0101 = NestedAssembly
            0110 = NestedFamANDAssem
            0111 = NestedFamORAssem
LayoutMask         =      0000-0000-0001-1000 = 24
              00 = AutoLayout // Field layout == Automatic
              01 = SequentialLayout (8) // Sequentially
              10 = ExplicitLayout (16) // Layout at the specified offsets (C-Style with Header)
ClassSemanticsMask =      0000-0000-0010-0000 = 32
               0 = Class     // Type == Class, Struct, Enum
               1 = Interface (32) // Type == Interface
Abstract           =      0000-0000-1000-0000 = 128 // Abstract
Sealed             =      0000-0001-0000-0000 = 256 // Concrete + no extension
SpecialName        =      0000-0100-0000-0000 = 1024 // Special as said by name
Import             =      0001-0000-0000-0000 = 4096 // Is imported from another module (C = H-File)
Serializable       =      0010-0000-0000-0000 = 8192  // Can be serialized
WindowsRuntime     =      0100-0000-0000-0000 = 16384 // Specifies a Windows Runtime Type
StringFormatMask   = 0011-0000-0000-0000-0000 = 196608
// Used to retrieve string information for native interoperability
              00 = AnsiClass            // LPTSTR -> ANSI
              01 = UnicodeClass (65536) // LPTSTR -> UNICODE
              10 = AutoClass (131072)   // LPTSTR -> Automatic
              11 = CustomFormatClass (196608) // LPSTR == Impl.specific means (not used by .NET)
ReservedMask       = 0100-0000-1000-0000-0000 = 196608 // Reserved by Runtime Use
          RTSpecialName    =  000-0000-1              = 2048   // RT should check Name encoding
          HasSecurity      =  100-0000-0              = 262144 // Type has security associate with it
BeforeFieldInit  = 1-0100-0000-0000-0000-0000 = 1048576 // Calling static methods does not force TypeInit
//*/

        MemberAttributes memAttr;       // Attributes of a Class member

        MethodAttributes methAttr;      // Attributes of a Method
        FieldAttributes fieldAttr;      // Attributes of a Field
        EventAttributes evattr;         // Attributes of an Event
        PropertyAttributes propAttr;    // Attributes of a Property

        ParameterAttributes paramAttr;  // Attributes of a Parameter
        FieldDirection fieldDir;        // Direction of parameter and argument declarations
        GenericParameterAttributes genParAttr; // Constraints on GenericParam of Type/Method

        void mteh(AttributeTargets attrTarget)
        {
            switch (attrTarget) {
                case AttributeTargets.Assembly: break;
                case AttributeTargets.Module: break;
                case AttributeTargets.Class: break;
                case AttributeTargets.Struct: break;
                case AttributeTargets.Enum: break;

                case AttributeTargets.Constructor: break;
                case AttributeTargets.Method: break;
                case AttributeTargets.Property: break;
                case AttributeTargets.Field: break;
                case AttributeTargets.Event: break;

                case AttributeTargets.Interface: break;
                case AttributeTargets.Parameter: break;
                case AttributeTargets.Delegate: break;

                case AttributeTargets.ReturnValue: break;
                case AttributeTargets.GenericParameter: break;
                case AttributeTargets.All: break;
            }
        }
    }

    public abstract class MetaData
    {

#warning Should the Type be stored here as well?
        protected TypeData enclosedType;

        protected MemberAttributes attr;
        /**
ScopeMask  = 0000-0000-0000-1111 = 15
	0001 = Abstract
	0010 = Final
	0011 = Static
	0100 = Override
	0101 = Const
VTableMask = 0000-0000-1111-0000 = 240
	0001 = New (16)
Overloaded = 0000-0001-0000-0000 = 256
AccessMask = 1111-0000-0000-0000 = 61440
	0001 = Assembly (4096)
	0010 = FamilyAndAssembly (8192)
	0011 = Family (12288)
	0100 = FamilyOrAssembly (16384)
	0101 = Private (20480)
	0111 = Public (24576)
        //*/
        protected Dictionary<string, MemberAttributes> attrDict = new Dictionary<string, MemberAttributes>() {
           {"assembly", MemberAttributes.Assembly },
            {"internal", MemberAttributes.FamilyAndAssembly },
            {"protected", MemberAttributes.Family },
            {"shared", MemberAttributes.FamilyOrAssembly },
            {"private", MemberAttributes.Private },
            {"public", MemberAttributes.Public },
            {"abstract", MemberAttributes.Abstract }, // 1
            {"final", MemberAttributes.Final }, // 2
            {"static", MemberAttributes.Static }, // 3
            {"readonly", MemberAttributes.Const }, // 5
            {"override", MemberAttributes.Override }, // 4
            {"virtual", MemberAttributes.Overloaded }, // 256
        };
    }
}
