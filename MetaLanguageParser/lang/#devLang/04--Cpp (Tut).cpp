C++ -> CPlusPlus -> CPP

true --> 1
String -> string
Int32 -> int
LowerThan --> "{0} < {1}"
######
## Import
#include <iostream>

## Module
using namespace std;

## §addType
struct $$name$$ {
	$$fields$$
	$$ctors$$
	$$$methods$$
}

## §addMain
int main()§n{§inc
	$$code$$§dec
}
/// DefineOption: InlineMain (useful for JS as well (there is main.js just "$$code$$"))
/// Main will be outputted into Namespace Layer
/// Add Field to ExeBuilder containing that MethodData

##while
while ($$cond$$) {§inc
	$$code$$§dec
}

##vardecl->Decl
{type} {name};

## §readLine(args)
cin >> $$args$$; cin.ignore();
## §readMultiple
cin >> $$arg1$$ >> $$arg2$$ >> $$arg1$$ ...; cin.ignore();
//Read-Caster --> --snip--

## Branches
if($$cond$$) {§inc
	$$code$$§dec
} else {§inc
	$$code$$§dec
}

## §writeLine
cout << $$arg$$ << endl;