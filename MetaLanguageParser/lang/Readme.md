This folder contains all Language Definition Files (further refered to as 'mdef' as in MetaDefinition) and Language Destination Files (further referred to as 'mdest' or 'dest', likewise). 

As a general rule, any line starting with a Hash ('#') will be treated as comment and therefore skipped.
Empty Lines or such containing only WS as well.

# Files #
##/meta/##
* _opArith.txt
** Define Arithmetic operators for use in the codefiles, with the key being the connector between meta and concrete language
** Form: $$key$$ + TABULATOR + $$operator$$ (in plain text, no spaces)
*** Use §§Unary to switch to OneArgument-only, §§Binary for two
* _opBool.txt: Likewise for Boolean operators (allowed in conditonals)
* _types.txt (not yet)
** Any additional type you would want to map.
## Concrete Language Directory ##
* {langFile}
** Contains configuration stuff
* ~~_newType.txt (not yet)~~
** Contains Syntax how a new Type will look like
* _operator.txt
** Define mappings for all operations via the keys defined in the two meta files files.
** Form: $$key$$ + TABULATOR + $$mapping$$ (in plain text)
*** Use {0} for first operator, {1} for second
* _vardecl.txt
** Define "Declaration", "Assignment", and optional "Definition" (both).
** If Definition is missing, it will always seperate the declaration and Assignment
* §main.{suffix}
** Used when you use "§addMethod(§main)"
* §mainType.{suffix}
** Used when you use "§addType(§main)"