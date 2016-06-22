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
        /// <summary>
        /// Base path to the "lang" directory, to allow moving of stuff (like, into same folder of exe without changing too much paths)
        /// </summary>
        public static string basePath = @"../../lang";
        private static string _lang;
        public static string LangCode
        {
            get { return _lang; }
            set
            {
                string path = basePath + "/" + value;
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
        /// Returns the path to the "lang" directory, where all LangDefinition folders are located.
        /// </summary>
        /// <returns></returns>
        public static string getLangPath() => $"{basePath}/{_lang}";//new StringBuilder(basePath).Append($"/{_lang}").ToString();

        /// <summary>
        /// Returns the path to the given file in the current LanguageDirectory.
        /// </summary>
        /// <returns></returns>
        internal static string getLangPath(string file) => new StringBuilder(basePath).Append($"/{_lang}/{file}.{langSuffix}").ToString();
        /// <summary>
        /// Retrieves the main configfile for the language
        /// </summary>
        /// <returns></returns>
        internal static string getLangFile() => new StringBuilder(basePath).Append($"/{_lang}/{_lang}").ToString();

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
        internal static string getMetaPath() => $"{basePath}/meta";//new StringBuilder(basePath).Append("/meta").ToString();
        /// <summary>
        /// Returns the path to the specified file in the metaDictionary
        /// </summary>
        /// <returns></returns>
        internal static string getMetaPath(string file)
            => new StringBuilder(getMetaPath()).Append($"/{file}").Append(metaSuffix).ToString();

        /// <summary>Returns the path to the MetaDirectory</summary>
        internal static string getResourcePath() => new StringBuilder(basePath).Append("/../Resources").ToString();
        /// <summary>{resx}/resxDict.<paramref name="lang"/>.xml</summary>
        internal static string getResourcePath(string lang) => getResourcePath() + "/resxDict." + lang + ".xml";
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
