using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace DrathBot
{
    public static class DiscordUtility
    {
        public static DiscordUser? GetUserByIDString(string ID)
        {
            if (!ulong.TryParse(ID, out ulong _UserID)) { return null; }
            DiscordUser User;
            try { User = Program._DiscordBot.Client.GetUserAsync(_UserID)?.Result; }
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

        public static DiscordMessage[] GetAllMessagesInChannel(ulong channelID)
        {
            var TestChannel = Program._DiscordBot.Client.GetChannelAsync(channelID).Result;
            var Messages = TestChannel.GetMessagesAsync(5000).AllResultsAsync().Result;
            //For some reason the GetMessagesAsync doesn't seem to respect the limit and will always only grab 100 messages
            //To get around this I can use GetMessagesBeforeAsync to get all of the messages before the oldest one.
            var PreviousMessages = TestChannel.GetMessagesBeforeAsync(Messages.First().Id, 5000).AllResultsAsync().Result;

            var allMessages = Messages.Concat(PreviousMessages);

            return [.. allMessages.OrderBy(x => x.Timestamp)];
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
    }

    public static class Utility
    {
        public static string Replace(this string s, int index, int length, string replacement)
        {
            var builder = new StringBuilder();
            builder.Append(s[..index]);
            builder.Append(replacement);
            builder.Append(s[(index + length)..]);
            return builder.ToString();
        }
        public static bool IndexInBounds<T>(this IEnumerable<T> List, int Index)
        {
            return Index >= 0 && List.Count() > Index;
        }

        public static string GetUniqueDateID(this DateTime date)
        {
            return $"{date.Year}-{date.Month}-{date.Day}";
        }

        public static string[] TrimSplit(this string s, string Val)
        {
            return s.Split(Val).Select(x => x.Trim()).ToArray();
        }
        public static T GetValueAs<Y, T>(this Dictionary<Y, object> source, Y Key)
        {
            if (!source.ContainsKey(Key)) { return default; }
            return source[Key].SerializeConvert<T>();
        }
        public static T SerializeConvert<T>(this object source)
        {
            string Serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(Serialized);
        }
        public static Tuple<string, string> SplitOnce(this string input, char Split, bool LastOccurence = false)
        {
            int idx = LastOccurence ? input.LastIndexOf(Split) : input.IndexOf(Split);
            Tuple<string, string> Output;
            if (idx != -1) { Output = new(input[..idx], input[(idx + 1)..]); }
            else { Output = new(input, string.Empty); }
            return Output;
        }
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
        public static List<T> GetRange<T>(this List<T> list, Range range)
        {
            var (start, length) = range.GetOffsetAndLength(list.Count);
            return list.GetRange(start, length);
        }

        public static string TrimSpaces(this string myString)
        {
            return Regex.Replace(myString, @"\s+", " ");
        }

        public static Version AsVersion(this string version)
        {
            if (!version.Any(x => char.IsDigit(x))) { version = "0"; }
            if (!version.Contains('.')) { version += ".0"; }
            return new Version(string.Join("", version.Where(x => char.IsDigit(x) || x == '.')));
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static void SetIfEmpty<T, V>(this Dictionary<T, V> Dict, T Value, V Default)
        {
            if (!Dict.ContainsKey(Value)) { Dict[Value] = Default; }
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        public static void PrintObjectToConsole(object o)
        {
            Debug.WriteLine(o.ToFormattedJson());
        }
        public static string ToFormattedJson(this object o)
        {
            return JsonConvert.SerializeObject(o, _NewtonsoftJsonSerializerOptions);
        }

        public readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter(), new IPConverter() }
        };
        public class IPConverter : JsonConverter<IPAddress>
        {
            public override void WriteJson(JsonWriter writer, IPAddress value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override IPAddress ReadJson(JsonReader reader, Type objectType, IPAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var s = (string)reader.Value;
                return IPAddress.Parse(s);
            }
        }

        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID.Trim();
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID[1..^1];
            }
            return Literal;
        }

        public static bool isJsonTypeOf<T>(string Json)
        {
            try
            {
                T test = JsonConvert.DeserializeObject<T>(Json, _NewtonsoftJsonSerializerOptions);
                return test != null;
            }
            catch
            {
                return false;
            }
        }

        public static void TimeCodeExecution(Stopwatch stopwatch, string CodeTimed = "", int Action = 0)
        {
            if (Action == 0)
            {
                stopwatch.Start();
            }
            else
            {
                Debug.WriteLine($"{CodeTimed} took {stopwatch.ElapsedMilliseconds} m/s");
                stopwatch.Stop();
                stopwatch.Reset();
                if (Action == 1) { stopwatch.Start(); }
            }
        }

        public static bool DynamicPropertyExist(dynamic Object, string name)
        {
            if (Object is null) { return false; }
            if (Object is ExpandoObject)
                return ((IDictionary<string, object>)Object).ContainsKey(name);

            var type = Object.GetType();
            return type.GetProperty(name) != null;
        }
        public static bool DynamicMethodExists(dynamic Object, string methodName)
        {
            if (Object is null) { return false; }
            var type = Object.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static bool OBJIsThreadSafe(Thread thread, dynamic Obj)
        {
            bool IsWinformSafe = Obj is not null && (!Utility.DynamicPropertyExist(Obj, "IsHandleCreated") || Obj.IsHandleCreated);
            return thread is not null && thread.IsAlive && Obj is not null && IsWinformSafe;
        }

        public static string ConvertYamlStringToJsonString(string YAML, bool Format = false)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            object yamlIsDumb = deserializer.Deserialize<object>(YAML);
            if (Format) { return JsonConvert.SerializeObject(yamlIsDumb, _NewtonsoftJsonSerializerOptions); }
            return JsonConvert.SerializeObject(yamlIsDumb);
        }
        public static string ConvertObjectToYamlString(object OBJ)
        {
            var serializer = new SerializerBuilder().Build();
            var stringResult = serializer.Serialize(OBJ);
            return stringResult;
        }
        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) { continue; }
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, _NewtonsoftJsonSerializerOptions);
        }

        public static string ConvertToCamelCase(string Input)
        {
            string NiceName = Input.ToLower();
            TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
            NiceName = cultInfo.ToTitleCase(NiceName);
            return NiceName;
        }

        public static T GetfromFile<T>(string FilePath)
        {
            var Result = GetfromFile<T>(FilePath, default, false);
            return Result;
        }

        public static T GetfromFile<T>(string FilePath, T Default, bool WriteFileIfError)
        {
            T result = Default;
            bool FileError = false;
            if (File.Exists(FilePath))
            {
                try { result = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath)); }
                catch
                {
                    Debug.WriteLine($"Failed to Deserialize {FilePath} to {typeof(T)}");
                    FileError = true;
                    result = Default;
                }
            }
            else
            {
                Debug.WriteLine($"File did not exit {FilePath}");
                FileError = true;
            }
            if (FileError && WriteFileIfError)
            {
                File.WriteAllText(FilePath, result.ToFormattedJson());
            }
            return result;
        }

    }
}
