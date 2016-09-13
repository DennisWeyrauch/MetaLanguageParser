@echo off
SETLOCAL
SET langCode=""
SET /P langCode= "Enter Langcode: "
(echo @echo off
echo SETLOCAL
echo SET filepath="codefile.txt"
echo IF EXIST %%~f1 SET filepath=%%~fs1
echo MetaLanguageParser.exe %langCode% %%filepath%%
) >#%langCode%.bat
::echo 
