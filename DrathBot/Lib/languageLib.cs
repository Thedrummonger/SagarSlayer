using DrathBot.DataStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace SagarSlayer.Lib
{
    public static class languageLib
    {
        private static HashSet<string>? CommonWords = null;
        public static HashSet<string> GetCommonWords()
        {
            CommonWords ??= GetCommonWordsFromMainFile();
            return CommonWords;
        }
        public static void InitializeCommonWords() => CommonWords = GetCommonWordsFromMainFile();
        class VerbConjuctions
        {
            public List<string> infinitive = [];
            public List<string> participle = [];
            public List<string> gerund = [];
            public indicativeTense indicative = new indicativeTense();
            public indicativeTense subjuntive = new indicativeTense();
            public List<string> imperative = [];
        }
        class indicativeTense
        {
            public List<string> present = [];
            public List<string> perfect = [];
            public List<string> imperfect = [];
            public List<string> plusperfect = [];
            public List<string> future = [];
        }
        public static HashSet<string> GetCommonWordsFromLangFiles()
        {
            HashSet<string> words = new HashSet<string>();
            var adverbs = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(Path.Combine("lib", "adverbs.json")));
            var nouns = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(Path.Combine("lib", "nouns.json")));
            var prepositions = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(Path.Combine("lib", "prepositions.json")));
            var verbs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>[]>>(File.ReadAllText(Path.Combine("lib", "verbs.json")));
            var adjectives = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Path.Combine("lib", "adjectives.json")));
            var verbConjunctions = JsonConvert.DeserializeObject<VerbConjuctions[]>(File.ReadAllText(Path.Combine("lib", "verbs_with_conjugations.json")));

            foreach (var i in adverbs["adverbs"]) { words.Add(i); }
            foreach (var i in nouns["nouns"]) { words.Add(i); }
            foreach (var i in prepositions["prepositions"]) { words.Add(i); }
            foreach (var i in verbs["verbs"]) { words.Add(i["present"]); words.Add(i["past"]); }
            foreach (var i in adjectives) { words.Add(i); }
            foreach (var vc in verbConjunctions)
            {
                foreach (var i in vc.infinitive) { words.Add(i); }
                foreach (var i in vc.participle) { words.Add(i); }
                foreach (var i in vc.gerund) { words.Add(i); }
                foreach (var i in vc.indicative.present) { words.Add(i); }
                foreach (var i in vc.indicative.perfect) { words.Add(i); }
                foreach (var i in vc.indicative.imperfect) { words.Add(i); }
                foreach (var i in vc.indicative.plusperfect) { words.Add(i); }
                foreach (var i in vc.indicative.future) { words.Add(i); }
                foreach (var i in vc.subjuntive.present) { words.Add(i); }
                foreach (var i in vc.subjuntive.perfect) { words.Add(i); }
                foreach (var i in vc.imperative) { words.Add(i); }
            }
            return words;
        }

        public static HashSet<string> GetCommonWordsFromMainFile()
        {
            if (File.Exists(StaticBotPaths.Sagarism.Files.CommonWords))
            {
                return JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(StaticBotPaths.Sagarism.Files.CommonWords));
            }
            return [];
        }

        public static void WriteCommonWordsFile()
        {
            HashSet<string> CommonWords = [.. GetCommonWords(), .. GetCommonWordsFromLangFiles()];
            File.WriteAllText(StaticBotPaths.Sagarism.Files.CommonWords, CommonWords.ToFormattedJson());
        }
    }
}
