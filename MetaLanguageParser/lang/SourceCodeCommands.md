This file should documentate ParserHooks useable in the codeFile or in the mDest Files.


# CodeFile #
-- none yet --

# DefFiles #
$$varName::parserHook$$

# DestFiles #
* NewLine -- Just make a linebreak (Multiple linebreaks without code in between will be truncated)
** Use §n to force linebreaks in Result (will then use the System's newline culture)
** Best Result when used NOT on own lines, but on the end of codelines 
* §inc -- Increment Indent (' ' as visual seperation from code is allowed and will be removed subsequently)
* §dec -- Decrement Indent (Likewise)
* §retract -- Retract SourcePos (Will be done right before printing Code to File)
  * §retract(6) --> Delete previous 6 characters. 
  * Example: VB.net --> "End If§retract(6)Else" --> "Else"
  * Obviously should only be used at the start of files to be useful.
