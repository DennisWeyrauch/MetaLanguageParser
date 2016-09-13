@echo off
SETLOCAL
SET filepath="codefile.txt"
IF EXIST %~f1 SET filepath=%~fs1
MetaLanguageParser.exe vbnet %filepath%
