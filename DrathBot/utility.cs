using DrathBot.DataStructure;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SagarSlayer.DataStructure;
using SagarSlayer.Lib;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TDMUtils;
using YamlDotNet.Serialization;
using static DrathBot.DataStructure.ExtendedDiscordObjects;

namespace DrathBot
{
    public static class DiscordUtility
    {
        public static async Task<DiscordUser?> GetUserByIDString(string ID)
        {
            if (!ulong.TryParse(ID, out ulong _UserID)) { return null; }
            DiscordUser? User;
            try { User = await Program._DiscordBot.Client.GetUserAsync(_UserID); }
            catch { User = null; }
            return User;
        }
        public static async Task<List<T>> AllResultsAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            if (null == asyncEnumerable)
                throw new ArgumentNullException(nameof(asyncEnumerable));

            var list = new List<T>();
            await foreach (var t in asyncEnumerable)
            {
                list.Add(t);
            }

            return list;
        }

        public static async Task<DiscordMessage[]> GetAllMessagesInChannel(ulong channelID)
        {
            var TestChannel = await Program._DiscordBot.Client.GetChannelAsync(channelID);
            var Messages = await TestChannel.GetMessagesAsync(5000).AllResultsAsync();
            //For some reason the GetMessagesAsync doesn't seem to respect the limit and will always only grab 100 messages
            //To get around this I can use GetMessagesBeforeAsync to get all of the messages before the oldest one.
            var PreviousMessages = await TestChannel.GetMessagesBeforeAsync(Messages.First().Id, 5000).AllResultsAsync();

            var allMessages = Messages.Concat(PreviousMessages);

            DiscordMessage[] Result = [.. allMessages.OrderBy(x => x.Timestamp)];

            return Result;
        }
        public static Stream ConvertAudioToPcm(string filePath)
        {
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = DrathBot.DataStructure.StaticBotPaths.Sagarism.Files.FFMPEG,
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            return ffmpeg.StandardOutput.BaseStream;
        }

        public static string GetUniqueDateID(this DateTime date)
        {
            return $"{date.Year}-{date.Month}-{date.Day}";
        }

        public static string RemoveSpecialChars(this string s)
        {
            return string.Join("", s.Where(x => char.IsAsciiLetterOrDigit(x) || char.IsWhiteSpace(x)));
        }

        public static HashSet<string> GetQuotedUsersFromQuote(this SerializeableDiscordMessage quote)
        {
            var CommonWords = languageLib.GetCommonWords();
            if (quote.Content.IsNullOrWhiteSpace()) { return []; }
            var lines = quote.Content.SplitAtNewLine();
            var QuotedUsers = new HashSet<string>();
            foreach ( var line in lines )
            {
                if (line.Contains(':')) { QuotedUsers.Add(line.SplitOnce(':').Item1); }
            }

            HashSet<string> result = new HashSet<string>();
            foreach (var QuotedUser in QuotedUsers)
            {
                string TrimmedUserline = QuotedUser.Replace('-', ' ');
                TrimmedUserline = TrimmedUserline.RemoveSpecialChars().ToLower().TrimSpaces().Trim();
                var Keywords = TrimmedUserline.Split(' ');
                foreach (var Keyword in Keywords)
                {
                    if (char.IsNumber(Keyword[0])) { continue; }
                    if (CommonWords.Contains(Keyword)) { continue; }
                    result.Add(Keyword);
                }
            }
            return result;
        }
    }
}
