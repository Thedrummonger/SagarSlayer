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
            try { User = await Program._DiscordBot.GetClient().GetUserAsync(_UserID); }
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
            var TestChannel = await Program._DiscordBot.GetClient().GetChannelAsync(channelID);
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

        public static string[] GetClosestMatch(string Input, HashSet<string> valid)
        {
            Dictionary<int, List<string>> distances = new Dictionary<int, List<string>>();
            foreach(var i in valid)
            {
                int Distance = LevenshteinDistance.Compute(Input, i);
                distances.SetIfEmpty(Distance, new List<string>());
                distances[Distance].Add(i);
            }
            int Smallest = distances.Keys.Min();
            return [..distances[Smallest]];
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

        public static HashSet<string> GetAllRelevantUsers(List<SerializeableDiscordMessage> messages)
        {
            HashSet<string> Users = [];
            foreach (var message in messages )
            {
                Users.UnionWith(message.RelevantUsers);
            }
            return Users;
        }
    }
    static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
