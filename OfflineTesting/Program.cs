// See https://aka.ms/new-console-template for more information
using DrathBot;
using DrathBot.DataStructure;
using DrathBot.MessageHandeling;
using Newtonsoft.Json;
using SagarSlayer.DataStructure; 
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using static DrathBot.DataStructure.ExtendedDiscordObjects;
using static DrathBot.DataStructure.StaticBotPaths.Sagarism;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

internal class Program
{
    private static void Main(string[] args)
    {

        Console.ReadLine();
    }

    void TestTTS()
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
        var QuoteCache = Utility.GetfromFile<Misc.DistinctList<SerializeableDiscordMessage>>(Files.SagarQuotesCacheFile);
        var OldCache = Utility.GetfromFile<Misc.DistinctList<SerializeableDiscordMessage>>(OldSagarQuotesCacheFile);
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