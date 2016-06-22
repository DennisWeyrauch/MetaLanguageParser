This file should documentate ParserHooks useable in the codeFile or in the mDest Files.

# CodeFile
-- none yet --

# DestFiles
* NewLine -- Just make a linebreak
* §inc -- Increment Indent
* §dec -- Decrement Indent
* §retract -- Retract SourcePos (Will be done right before printing Code to File)
  * §retract(6) --> Delete previous 6 characters. 
  * Example: VB.net --> "End If§retract(6)Else" --> "Else"
  * Obviously should only be used at the start of files to be useful.
