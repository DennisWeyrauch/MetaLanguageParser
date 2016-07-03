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
                            langPath = basePath + "/lang";
                            resxPath = basePath + "/Resources";
                            break;
                        case "lang":
                            langPath = myFunc(str[1]);
                            break;
                        case "resx":
                            resxPath = myFunc(str[1]);
                            break;
                        case "printparts":
                            bool b;
                            Boolean.TryParse(str[1].Trim(), out b);
                            Program.printParts = b;
                            break;
                    }
                }
            } else {
                string cfgFile = @"## <AutoRegen Config-Note> ##
## This file contains where the Language files are stored
## Paths can be relative or absolute
## Lines starting with '#' and empty Lines will be ignored
## This file will be regenerated when missing.
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
                langPath = "../../lang";
                resxPath = "../../Resources";
            }
        }

        /// <summary>
        /// Base path to the "lang" directory, to allow moving of stuff (like, into same folder of exe without changing too much paths)
        /// </summary>
        public readonly static string basePath;// = @"../../lang";
        /// <summary>
        /// Contains the path to the Language Directory, where all Language Definitions are stored
        /// </summary>
        public readonly static string langPath;
        /// <summary>
        /// Contains the path to the Resource Directory
        /// </summary>
        public readonly static string resxPath;
        private static string _lang;
        public static string LangCode
        {
            get { return _lang; }
            set
            {
                string path = langPath + "/" + value;
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
        /// Returns the path to the current "lang" directory
        /// </summary>
        /// <returns></returns>
        public static string getLangPath() => $"{langPath}/{_lang}";//new StringBuilder(basePath).Append($"/{_lang}").ToString();

        /// <summary>
        /// Returns the path to the given file in the current LanguageDirectory. == basePath/{_lang}/file.{langSuffix}
        /// </summary>
        /// <returns></returns>
        internal static string getLangPath(string file) => new StringBuilder(langPath).Append($"/{_lang}/{file}.{langSuffix}").ToString();
        internal static string getLangPath2(string file) => $"{langPath}/{_lang}/{file}.{langSuffix}";
        internal static string getLangPath(string file, bool hasSuffix) => new StringBuilder(langPath).Append($"/{_lang}/{file}").ToString();
        /// <summary>
        /// Retrieves the main configfile for the language
        /// </summary>
        /// <returns></returns>
        internal static string getLangFile() => new StringBuilder(langPath).Append($"/{_lang}/{_lang}").ToString();

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
        internal static string getMetaPath() => $"{langPath}/meta";//new StringBuilder(basePath).Append("/meta").ToString();
        /// <summary>
        /// Returns the path to the specified file in the metaDictionary
        /// </summary>
        /// <returns></returns>
        internal static string getMetaPath(string file)
            => new StringBuilder(getMetaPath()).Append($"/{file}").Append(metaSuffix).ToString();

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
