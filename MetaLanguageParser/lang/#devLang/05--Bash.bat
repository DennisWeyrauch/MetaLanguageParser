Bash --> bash --> ???
https://de.wikipedia.org/wiki/Bash_(Shell)

Comment --> # Linestart + Whitespace

true --> true
::String -> 
::Int32 -> 
LowerThan --> 
######
::## Import

::## Module

::## §addType { $$code$$ }

::## §addMain { $$code$$ }

## while($$cond$$) { $$code$$ }
while $$cond$$; do §inc
	$$code$$
done


::## vardecl->Decl
--> Implicit, not type-safe
--> VarRef: "$" + name

## §readLine($$args$$)
read $$args$$

::## Read-Caster

## Branches
if [ $$cond$$ ]
then §inc
	$$code$$
else
	$$code$$
fi

## §writeLine($$args$$)
echo $$args$$