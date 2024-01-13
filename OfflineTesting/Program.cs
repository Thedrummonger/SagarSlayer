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

string OldSagarQuotesCacheFile = Path.Combine(Directories.SagarismData, "SagarQuoteCacheOLD.json");
var QuoteCache = Utility.GetfromFile<Misc.DistinctList<SerializeableDiscordMessage>>(StaticBotPaths.Sagarism.Files.SagarQuotesCacheFile);
var OldCache = Utility.GetfromFile<Misc.DistinctList<SerializeableDiscordMessage>>(OldSagarQuotesCacheFile);
QuoteCache.ResetAll();

foreach(var oldUsedQuote in OldCache.Used)
{
    var UsedQUote = QuoteCache.Unused.First(x => x.Content == oldUsedQuote.Content);
    QuoteCache.GetUnused(QuoteCache.Unused.IndexOf(UsedQUote));
    Debug.WriteLine($"Using Old Quote {UsedQUote.Content}");
}

File.WriteAllText(StaticBotPaths.Sagarism.Files.SagarQuotesCacheFile, QuoteCache.ToFormattedJson());

Console.ReadLine();

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