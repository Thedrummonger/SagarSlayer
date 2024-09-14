namespace DrathBot.DataStructure
{
    public class StaticBotPaths
    {
        public static string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DrathBot");

        public class Sagarism
        {
            public class Files
            {
                public static string SagarismConfig = Path.Combine(Directories.SagarismData, "Data.json");
                public static string SagarQuotesCacheFile = Path.Combine(Directories.SagarismData, "SagarQuoteCache.json");
                public static string MiscQuotesCacheFile = Path.Combine(Directories.SagarismData, "MiscQuoteCache.json");
                public static string SagarResponseFile = Path.Combine(Directories.SagarismData, "SagarResponses.json");
                public static string SagarDailyQuoteFile = Path.Combine(Directories.SagarismData, "DailyQuotes.json");
                public static string DefaultResponseFile = Path.Combine(Directories.SagarismData, "DefaultResponses.json");
                public static string AIData = Path.Combine(Directories.SagarismData, "ChatGPT.json");
                public static string ImageCensors = Path.Combine(Directories.SagarismData, "ImageCensors.json");
                public static string FFMPEG = Path.Combine(Directories.SagarismData, "FFMPEG", "ffmpeg.exe");
                public static string CronDebt = Path.Combine(Directories.SagarismData, "CronDebt.json");
                public static string CommonWords = Path.Combine(Directories.SagarismData, "CommonWords.json");
            }
            public class Directories
            {
                public static string SagarismData = Path.Combine(AppDataFolder, "Sagarism");
                public static string SagarismSoundClips = Path.Combine(SagarismData, "Sound");
            }
        }
    }
}
