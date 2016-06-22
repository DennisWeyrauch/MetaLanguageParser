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
            bool options = false;
            foreach (var entry in readFile(path)) {
                if (entry[0].Equals("§§Options§§")) {
                    options = true;
                    continue;
                }
                entry[0] = "__" + entry[0];
                if (options) {
                    resxDict.Add(entry[0], "true");
                } else {
                    resxDict.Add(entry[0].ToUpper(), entry[1]);
                }
            }
            Common.Serializer.SerializeFile(resxDict, resxPath);
        }
        // public static bool containsField(string name) { }


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
        internal static bool __IsClosureLikeInit => resxDict.TryGetValue(nameof(__IsClosureLikeInit), out outDummy);
        /// <summary>
        /// Describes whether or not WhiteSpace like NewLines and Indent is important and defines Syntax Elenments. <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __HasSyntacticWhitespace => resxDict.TryGetValue(nameof(__HasSyntacticWhitespace), out outDummy);
        /// <summary>
        /// Describes whether or not Newlines are part of the syntax. <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __HasSyntacticNewlines => resxDict.TryGetValue(nameof(__HasSyntacticNewlines), out outDummy);
        /// <summary>
        /// Describes whether or not If-elseIf-else can be stacked <para/>
        /// State: Not implemented
        /// </summary>
        internal static bool __NonStackingIfs => resxDict.TryGetValue(nameof(__NonStackingIfs), out outDummy);

    }
}
