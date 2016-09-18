using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Resources.ResxFiles;

namespace MetaLanguageParser.Resources
{
    public static class ResxFiles
    {

        static ResxFiles()
        {
            string cfgPath = "config.txt";
            if (File.Exists(cfgPath)) {

                foreach (var entry in File.ReadAllLines(cfgPath)) {
                    if (string.IsNullOrWhiteSpace(entry)) continue;
                    if (entry.StartsWith("#")) continue; // Comment
                    var str = entry.Split('=');
                    string elem;
                    Func<string,string> myFunc = ((s) => {
                        elem = s.Trim(' ', '"') as string;
                        if (elem.EndsWith("/")) elem.Remove(elem.Length - 2);
                        return elem;
                    });
                    switch (str[0].Trim().ToLower()) {
                        case "path":
                            basePath = myFunc(str[1]); 
                            langDir = basePath + "/lang";
                            resxPath = basePath + "/Resources";
                            break;
                        case "lang":
                            langDir = myFunc(str[1]);
                            break;
                        case "resx":
                            resxPath = myFunc(str[1]);
                            break;
                        case "printparts":
                            bool b;
                            Boolean.TryParse(str[1].Trim(), out b);
                            Program.printParts = b;
                            break;
                        case "suppressErr":
                            Boolean.TryParse(str[1].Trim(), out b);
                            Program.suppressError = b;
                            break;
                    }
                }
            } else {
                string cfgFile = @"## <AutoRegen Config-Note> ##
## This file contains where the Language files are stored
## Paths can be relative or absolute
## Lines starting with '#' and empty Lines will be ignored
## This file will be regenerated when missing.
## PathReader will trim any leading/trailing ' ' or "" from the PathStrings
## Note: Either PATH or LANG+RESX are required for proper use

# Base Dir in case only one path is enough (LANG is then PATH+'/lang' and resx PATH+'/resources')
#PATH = ""../..""
# Languages Files
LANG = ""../../lang""
# Resource Dictionaries
RESX = ""../../Resources""
# Debug Switch that prints every build-iteration
PrintParts = false

## About calling the Executable:
## 1st Arg = languageCode (the directory name in /lang), default is ""csharp""
## 2nd Arg = Path to CodeFile to parse. Default is ""./codefile.txt""
";
                File.WriteAllText(cfgPath, cfgFile);
                langDir = "../../lang";
                resxPath = "../../Resources";
            }
        }

        /// <summary>
        /// [BASE] Base path to the "lang" directory, to allow moving of stuff (like, into same folder of exe without changing too much paths)
        /// </summary>
        public readonly static string basePath;// = @"../../lang";
        /// <summary>
        /// [BASE/lang] Contains the path to the Language Directory, where all Language Definitions are stored
        /// </summary>
        public readonly static string langDir;
        /// <summary>
        /// [BASE/resx] Contains the path to the Resource Directory
        /// </summary>
        public readonly static string resxPath;
        private static string _lang;
        public static string LangCode
        {
            get {
                //if (string.IsNullOrEmpty(_lang)) throw new InvalidOperationException("ResxFiles was not initialized with LanguageCode!");
                return _lang; }
            set
            {
                string path = langDir + "/" + value;
                if (Directory.Exists(path)) {
                    path += "/" + value;
                    bool file = File.Exists(path);
                    bool fileExt = File.Exists(path+".txt");
                    if (file) _lang = value;
                    else if (fileExt) {
                        File.Move(path + ".txt", path);
                        _lang = value;
                    } else throw new FileNotFoundException($"No config file '{value}' found!");
                } else throw new DirectoryNotFoundException($"Language Directory '{value}' does not exist.");
            }
        }


        /// <summary>
        /// [BASE/lang/MYLANG] Returns the path to the current "lang" directory
        /// </summary>
        /// <returns></returns>
        public static string getLangPath() => $"{langDir}/{_lang}";//new StringBuilder(basePath).Append($"/{_lang}").ToString();
        /// <summary> [BASE/lang/MYLANG] </summary>
        private static string langPath => $"{langDir}/{_lang}";

        /// <summary>
        /// [BASE/lang/MYLANG/{<paramref name="file"/>}]Get a file from the CurLang-Directory
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static string getLangPath(string file) => $"{langPath}/{file}";// new StringBuilder(langDir).Append($"/{_lang}/{file}").ToString();

        //internal static string getLangPath3(string file) => $"{langPath}/{file}.{langSuffix}"; // Would be the shortest with 22 bytes
        /// <summary>
        /// [BASE/lang/MYLANG/mylang]Retrieves the main configfile for the language
        /// </summary>
        /// <returns></returns>
        internal static string getLangFile() => new StringBuilder(langDir).Append($"/{_lang}/{_lang}").ToString();
        /// <summary>
        /// [BASE/lang/MYLANG/{file}.{mySuffix}]Returns the path to the given file in the current LanguageDirectory. == basePath/{_lang}/file.{langSuffix}
        /// </summary>
        /// <returns></returns>
        internal static string getLangFile(string file) => new StringBuilder(langPath).Append($"/{file}.{langSuffix}").ToString();

        /// <summary>{resx}/opBinDict.xml</summary>
        internal static string getBinaryDict() => getMetaPath() + "/_opBool.txt";
        /// <summary>{resx}/opArithDict.xml</summary>
        internal static string getArithDict() => getMetaPath() + "/_opArith.txt";
        /// <summary>{resx}/opArithDict.xml</summary>
        internal static string getDestDict() => getLangPath() + "/_operator.txt";


        /// <summary>
        /// Returns the path to the MetaDirectory
        /// </summary>
        /// <returns></returns>
        internal static string getMetaPath() => $"{langDir}/meta";//new StringBuilder(basePath).Append("/meta").ToString();
        /// <summary>
        /// Returns the path to the specified file in the metaDictionary
        /// </summary>
        /// <returns></returns>
        internal static string getMetaPath(string file) => file.EndsWith(metaSuffix)
            ? new StringBuilder(getMetaPath()).Append($"/{file}").ToString()
            : new StringBuilder(getMetaPath()).Append($"/{file}").Append(metaSuffix).ToString();

        /// <summary>Returns the path to the MetaDirectory</summary>
        internal static string getResourcePath() => resxPath;// new StringBuilder(basePath).Append("/../Resources").ToString();
        /// <summary>{resx}/resxDict.<paramref name="lang"/>.xml</summary>
        internal static string getResourcePath(string lang) => getResourcePath() + "/resxDict." + lang + ".xml";//$"/resxDict.{lang}.xml";// + lang + ".xml";
        /// <summary>{resx}/opBinDict.xml</summary>
        internal static string getBinaryDict(string lang) => getResourcePath() + "/opBinDict.xml";
        /// <summary>{resx}/opArithDict.xml</summary>
        internal static string getArithDict(string lang) => getResourcePath() + "/opArithDict.xml";
        /// <summary>{resx}/opDestDict.<paramref name="lang"/>.xml</summary>
        internal static string getDestinationDict(string lang) => getResourcePath() + "/opDestDict." + lang + ".xml";

        /// <summary>Value: .txt</summary><value>.txt</value>
        public static string configSuffix => ".txt";
        /// <summary>Value: .txt</summary><value>.txt</value>
        public static string metaSuffix => ".txt";
        /// <summary><see cref="ResourceReader.__FILESUFFIX"/></summary>
        public static string langSuffix => ResourceReader.__FILESUFFIX;


    }
}
