using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Resources.ResxFiles;

namespace MetaLanguageParser.Resources
{
    /// <summary>
    /// Central location for reading/writing Resources
    /// </summary>
    public static class ResourceReader
    {
        public static void readConfiguration(string lang)
        {
            checkResx(lang);
            string path = getBinaryDict();
            if (!File.Exists(path)) throw new FileNotFoundException($"No config file '{lang}' found!");
            opBinDict = __readOpDict(path, getBinaryDict(lang), eOpDictType.Boolean);
            path = getArithDict();
            if (!File.Exists(path)) throw new FileNotFoundException($"No config file '{lang}' found!");
            opArithDict = __readOpDict(path, getArithDict(lang), eOpDictType.Arithmetic);
            path = getDestDict();
            if (!File.Exists(path)) throw new FileNotFoundException($"No config file '{lang}' found!");
            opDestDict = __readOpDict(path, getDestinationDict(lang));

        }

        #region ResourceReader
        public static ConfigurationDictionary resxDict = new ConfigurationDictionary();
        public static void checkResx(string lang)
        {
            string path = ResxFiles.getLangFile();
            string resxPath = ResxFiles.getResourcePath(lang);
            if (!File.Exists(path)) throw new FileNotFoundException($"No config file '{lang}' found!");
            if (File.Exists(resxPath)) {
                var src = File.GetLastWriteTime(path);
                var dst = File.GetLastWriteTime(resxPath);
                if (dst >= src) {
                    resxDict = Common.Serializer.DeserializeFile<ConfigurationDictionary>(resxPath);
                    return;
                }
            }
            foreach (var entry in readFile(path)) {
                entry[0] = "__" + entry[0].ToUpper();
                resxDict.Add(entry[0], entry[1]);
            }
            Common.Serializer.SerializeFile(resxDict, resxPath);
        }


        public static OperatorDictionary opBinDict = new OperatorDictionary(eOpDictType.Boolean);
        public static OperatorDictionary opArithDict = new OperatorDictionary(eOpDictType.Arithmetic);
        private static OperatorDictionary __readOpDict(string path, string resxPath, eOpDictType opType)
        {
            var dict = new OperatorDictionary(opType);
            if (File.Exists(resxPath)) {
                var src = File.GetLastWriteTime(path);
                var dst = File.GetLastWriteTime(resxPath);
                if (dst >= src) {
                    dict = Common.Serializer.DeserializeFile<OperatorDictionary>(resxPath);
                    dict.setType(opType);
                    return dict;
                }
            }

            Type type = null;
            foreach (var entry in readFile(path)) {
                if (entry[0].StartsWith("§§")) {
                    type = Operands.Operation.getOpType(entry[0].Substring(2).ToLower(), opType);
                    continue;
                }
                dict.Add(entry[1], Operands.Operation.createFrom(type, entry[0]));
            }
            Common.Serializer.SerializeFile(dict, resxPath);
            return dict;
        }


        public static ConfigurationDictionary opDestDict = new ConfigurationDictionary();
        private static ConfigurationDictionary __readOpDict(string path, string resxPath)
        {
            var dict = new ConfigurationDictionary();
            if (File.Exists(resxPath)) {
                var src = File.GetLastWriteTime(path);
                var dst = File.GetLastWriteTime(resxPath);
                if (dst >= src) {
                    return Common.Serializer.DeserializeFile<ConfigurationDictionary>(resxPath);
                }
            }

            foreach (var entry in readFile(path)) {
                if (entry[0].StartsWith("§§")) continue;
                dict.Add(entry[0], entry[1]);
            }
            Common.Serializer.SerializeFile(dict, resxPath);
            return dict;
        }
#endregion
        /// <summary>
        /// Yielder for reading MetaDefinition Files.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string[]> readFile(string path)
        {
            foreach (var entry in File.ReadAllLines(path)) {
                if (string.IsNullOrWhiteSpace(entry)) continue;
                if (entry.StartsWith("#")) continue; // Comment
                yield return entry.Split('\t');
            }
        }

        internal static string __BLOCK_INIT => resxDict[nameof(__BLOCK_INIT)];
        internal static string __BLOCK_CLOSE => resxDict[nameof(__BLOCK_CLOSE)];
        internal static string __FILESUFFIX => resxDict[nameof(__FILESUFFIX)];
        internal static string __STATEMENT_CLOSE => resxDict[nameof(__STATEMENT_CLOSE)];
        internal static string __INDENT => System.Text.RegularExpressions.Regex.Unescape(resxDict[nameof(__INDENT)]);
        
        private static string outDummy;
        /// <summary>
        /// Describes if the BlockClosure is consists of <see cref="__BLOCK_CLOSE"/> and an Individual Keyword introducing the Structure. <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __IS_CLOSURE_LIKE_INIT => resxDict.TryGetValue(nameof(__IS_CLOSURE_LIKE_INIT), out outDummy);
        /// <summary>
        /// Describes whether or not WhiteSpace like NewLines and Indent is important and defines Syntax Elenments. <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __HAS_SYNTATCTIC_WHITESPACE => resxDict.TryGetValue(nameof(__HAS_SYNTATCTIC_WHITESPACE), out outDummy);
        /// <summary>
        /// Describes whether or not Newlines are part of the syntax. <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __HAS_SYNTATCTIC_NEWLINES => resxDict.TryGetValue(nameof(__HAS_SYNTATCTIC_NEWLINES), out outDummy);
        /// <summary>
        /// Describes whether or not If-elseIf-else can be stacked <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __NON_STACKING_IFS => resxDict.TryGetValue(nameof(__NON_STACKING_IFS), out outDummy);

    }
    public static class rootExtensions
    {

#if false
        public static void checkResx(string lang)
        {
            string path = ExeBuilder.getLangPath() + "/" + lang;
            string resxPath = ExeBuilder.getResourcePath() + "/" + lang + ".resx";
            if(!File.Exists(path)) throw new FileNotFoundException($"No config file '{lang}' found!");
            if (File.Exists(resxPath)) {
                var src = File.GetLastWriteTime(path);
                var dst = File.GetLastWriteTime(resxPath);
                if (dst >= src) return;
            }
            //*/
            var root = new Resources.root();
            root.AddDefaults();
            //*/
            using (ResourceWriter resx = new ResourceWriter(resxPath)) {
                foreach (var entry in File.ReadAllLines(path)) {
                    var kv = entry.Split('\t');
                    resx.AddResource(kv[0], kv[1]);
                    root.Items.Add(new rootData() { name = kv[0], value = kv[1] });
                }
            }
            Common.Serializer.SerializeFile(root, resxPath);
            //*/
        }

        public static void readResx(string lang)
        {
            string resxPath = ExeBuilder.getResourcePath() + "/" + lang + ".resx";
            Stream stream = File.OpenRead(resxPath);
            try {
                using (ResourceReader resx = new ResourceReader(stream)) {
                    foreach (DictionaryEntry entry in resx) {
                        resxDict.Add((string)entry.Key, (string)entry.Value);
                    }
                }
            } catch (ArgumentException) {
                var x = Common.Serializer.DeserializeFromStream<root>(ref stream);
                foreach (var item in x.Items) {
                    if(item is rootData) {
                        var i = (rootData) item;
                        resxDict.Add(i.name, i.value);
                    }
                }
            }
        }
#endif
        public static void AddDefaults(this root r)
        {
            var mime = new rootResheader() {name = "resmimetype", value = "text/microsoft-resx" };
            var version = new rootResheader() {name = "version", value = "2.0" };
            var reader = new rootResheader() {name = "reader", value = "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" };
            var writer = new rootResheader() {name = "writer", value = "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" };

            r.Items = new List<object>() { mime, version, reader, writer };
        }
    }
}
