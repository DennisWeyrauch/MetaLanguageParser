Responsible for storing all ResourceFiles (called Resx or ResxFiles elsewhere) in one central location.

The programm will always load it's data from here, except the corresponding mdef file has a more recent Timestamp, in which case it will be replaced.
* ResourceReader.cs
** CodeFile responsible for checking Timestamps and parsing of resxDefinitions.
* ResxFiles.cs
** Static class for Pathbuilding of all kinds.
* opArithDict.xml and opBinDict.xml
** Contains MetaDefinitions for operators
* opDestDict./lang/.sml
** Contains the InsertStrings for operators of the given language
* resxDict./lang/.xml
** Contains the OptionsDictionary for the given language
