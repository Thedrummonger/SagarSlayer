// See https://aka.ms/new-console-template for more information
using DrathBot;
using DrathBot.DataStructure;
using DrathBot.MessageHandling;
using Newtonsoft.Json;
using SagarSlayer.DataStructure; 
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using static DrathBot.DataStructure.ExtendedDiscordObjects;
using static DrathBot.DataStructure.StaticBotPaths.Sagarism;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using TDMUtils;
using DSharpPlus;
using SagarSlayer.Lib;

internal class Program
{
    private static void Main(string[] args)
    {
        GetNameData();
    }

    static void GetNameData()
    {
        var MiscQuotes = DataFileUtilities.LoadObjectFromFileOrDefault(StaticBotPaths.Sagarism.Files.MiscQuotesCacheFile, new Misc.DistinctList<SerializeableDiscordMessage>(), false);
        
        var CommonWords = SagarSlayer.Lib.languageLib.GetCommonWords();
        foreach (var discordMessage in MiscQuotes.Source)
        {
            discordMessage.RelevantUsers = discordMessage.GetQuotedUsersFromQuote();
        }
        foreach (var discordMessage in MiscQuotes.Used)
        {
            discordMessage.RelevantUsers = discordMessage.GetQuotedUsersFromQuote();
        }
        foreach (var discordMessage in MiscQuotes.Unused)
        {
            discordMessage.RelevantUsers = discordMessage.GetQuotedUsersFromQuote();
        }
        File.WriteAllText(StaticBotPaths.Sagarism.Files.MiscQuotesCacheFile, MiscQuotes.ToFormattedJson());
    }

    static void TestTTS()
    {
        using (SpeechSynthesizer synth = new SpeechSynthesizer())
        {
            synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Child);

            synth.Rate = -2;

            Stream ms = new MemoryStream();
            //reader.SetOutputToWaveStream(ms);

            Console.WriteLine("Speaking");
            synth.Speak("Hello this is an example expression from the computers TTS engine in C-Sharp");
            Console.WriteLine("Done Speaking");
        }
    }

    void SyncUsedWithOldQuoteCache()
    {
        string OldSagarQuotesCacheFile = Path.Combine(Directories.SagarismData, "SagarQuoteCacheOLD.json");
        var QuoteCache = DataFileUtilities.LoadObjectFromFileOrDefault<Misc.DistinctList<SerializeableDiscordMessage>>(Files.SagarQuotesCacheFile);
        var OldCache = DataFileUtilities.LoadObjectFromFileOrDefault<Misc.DistinctList<SerializeableDiscordMessage>>(OldSagarQuotesCacheFile);
        QuoteCache.ResetAll();

        foreach (var oldUsedQuote in OldCache.Used)
        {
            var UsedQUote = QuoteCache.Unused.First(x => x.Content == oldUsedQuote.Content);
            QuoteCache.GetUnused(QuoteCache.Unused.IndexOf(UsedQUote));
            Debug.WriteLine($"Using Old Quote {UsedQUote.Content}");
        }

        File.WriteAllText(Files.SagarQuotesCacheFile, QuoteCache.ToFormattedJson());
    }

    byte[] CompressByte(byte[] bytes)
    {
        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
        {
            gzipStream.Write(bytes, 0, bytes.Length);
        }
        return memoryStream.ToArray();
    }

    byte[] Compress(string String)
    {
        byte[] dataToCompress = Encoding.UTF8.GetBytes(String);
        byte[] compressedData = CompressByte(dataToCompress);
        return compressedData;
    }
}