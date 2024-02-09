using DrathBot.DataStructure;
using DSharpPlus.Entities;
using SagarSlayer.DataStructure;
using System.Diagnostics;
using static DrathBot.DataStructure.ExtendedDiscordObjects;
using static DrathBot.DataStructure.Sagarism;

namespace DrathBot.MessageHandeling
{

    public class Sagarism
    {
        public Random rnd = new();

        public SagarConfig SagarConfig;

        public Misc.DistinctList<SerializeableDiscordMessage> SagarQuotes;

        public Misc.DistinctList<SagarResponse> SagarReplies;

        public SagarConfigCommands Commands;

        public Timer DailyQuoteTimer;

        public Dictionary<ulong, string> ImageCensors;

        public Dictionary<string, SerializeableDiscordMessage> DailyQuoteTracking;

        public CronDebt Debt;

        public Sagarism()
        {
            Console.WriteLine("Initializing Sagarism");
            Commands = new SagarConfigCommands(this);
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }

            SagarConfig = Utility.GetfromFile<SagarConfig>(StaticBotPaths.Sagarism.Files.SagarismConfig);
            if (SagarConfig is null) { throw new Exception("Sagar Config Was missing or corrupted"); }
            SagarQuotes = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.SagarQuotesCacheFile, new Misc.DistinctList<SerializeableDiscordMessage>(), true);
            SagarReplies = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.SagarResponseFile, GetSagarRepliesTemplate(), true);
            DailyQuoteTracking = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.SagarDailyQuoteFile, new Dictionary<string, SerializeableDiscordMessage>(), true);
            ImageCensors = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.ImageCensors, new Dictionary<ulong, string>(), true);
            DailyQuoteTimer = new(e => { SendDailyQuote(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            Debt = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.CronDebt, new CronDebt(), true);

            SagarQuotes.ListUpdated += () => { Commands.UpdateQuoteCacheFile(); };
            SagarReplies.ListUpdated += () => { Commands.UpdateResponseCacheFile(); };
        }

        public async void PrintSagarInitializeData()
        {

            Console.WriteLine($"{SagarQuotes.Source.Count} Sagar Quotes ========");
            Console.WriteLine($"{SagarQuotes.Used.Count} History");
            Console.WriteLine($"{SagarQuotes.Unused.Count} Available");
            Console.WriteLine($"{SagarQuotes.MaxUsed} Max History");
            var QuotesChannel = await Program._DiscordBot.Client.GetChannelAsync(SagarConfig.DiscordData.GetQuotesChannel());
            Console.WriteLine($"Listening for new Sagar Quotes in Channel [{QuotesChannel.Guild.Name}({QuotesChannel.Guild.Id})]{QuotesChannel.Name}({QuotesChannel.Id})");
            Console.WriteLine($"{SagarReplies.Source.Count} Sagar Replies ========");
            Console.WriteLine($"{SagarReplies.Used.Count} History");
            Console.WriteLine($"{SagarReplies.Unused.Count} Available");
            Console.WriteLine($"{SagarReplies.MaxUsed} Max History");
            Console.WriteLine($"Sagar Config ========");
            var ConfigCopy = Utility.SerializeConvert<SagarConfig>(SagarConfig);
            ConfigCopy.DiscordData = null;
            Console.WriteLine($"{ConfigCopy.ToFormattedJson()}");
            var Now = DateTime.Now;
            Console.WriteLine($"Daily Sagar Quote ========");
            var GeneralChannel = await Program._DiscordBot.Client.GetChannelAsync(SagarConfig.DiscordData.GetGeneralChannel());
            Console.WriteLine($"Sending Daily Quotes to [{GeneralChannel.Guild.Name}({GeneralChannel.Guild.Id})]{GeneralChannel.Name}({GeneralChannel.Id})");
            Console.WriteLine($"Has sent Quote Today?: {HasSentDailyQuote(Now)}");
            Console.WriteLine($"Past Time Frame For Quote?: {Now.Hour > 8}");
            Console.WriteLine($"Sagarism Initialized ========");
        }

        public static Misc.DistinctList<SagarResponse> GetSagarRepliesTemplate()
        {
            List<SagarResponse> Responses = new List<SagarResponse>();
            foreach (var i in Utility.GetfromFile<string[]>(StaticBotPaths.Sagarism.Files.DefaultResponseFile, Array.Empty<string>(), false))
            {
                Responses.Add(new SagarResponse(i));
            }
            return new Misc.DistinctList<SagarResponse>(Responses, 0.7);
        }

        public bool HasSentDailyQuote(DateTime Now)
        {
            return DailyQuoteTracking.ContainsKey(Now.GetUniqueDateID());
        }

        bool FirstAttempt = true;
        private async void SendDailyQuote()
        {
            Debug.WriteLine("Attempting to send Daily Quote");
            if (FirstAttempt)
            {
                Debug.WriteLine("Waiting For Bot to Initialize");
                FirstAttempt = false;
                return;
            }
            var InternetTimeTest = DateTime.Now;

            if (HasSentDailyQuote(InternetTimeTest))
            {
                Debug.WriteLine("Already sent Todays Quote");
                return;
            }
            if (InternetTimeTest.Hour < 8)
            {
                Debug.WriteLine("Waiting until 8 am");
                return;
            }
            if (InternetTimeTest.Hour > 8)
            {
                Debug.WriteLine("Time Frame Has passed");
                return;
            }
            Debug.WriteLine("Sending Daily Quote");

            var Quote = GetRandomQuote();

            DailyQuoteTracking[InternetTimeTest.GetUniqueDateID()] = Quote;
            Commands.UpdateDailyQuoteCacheFile();

            var Channel = await Program._DiscordBot.Client.GetChannelAsync(SagarConfig.DiscordData.GetGeneralChannel());
            await Program._DiscordBot.Client.SendMessageAsync(Channel, BuildSagarQuoteOfTheDay(Quote));
        }

        public void ReplyToSagar(RecievedMessage message)
        {
            if (SagarConfig.ReplyTargets.Contains(message.Author.Id) && RollResponseChance())
            {
                var response = GetRandomResponse();
                Console.WriteLine($"Replying to Sagar with {response.ToFormattedJson()}");
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.AddMention(new UserMention(message.Author));
                messageBuilder.WithContent(response.Message);
                messageBuilder.WithReply(message.Message.Id);

                messageBuilder.SendAsync(message.Channel);
            }
        }

        public bool RollResponseChance()
        {
            int Roll = rnd.Next(0, 100);
            double Chance = SagarConfig.ReplyChance * 100;
            return (Roll < Chance);
        }

        public SagarResponse GetRandomResponse()
        {
            return SagarConfig.ReduceDuplicateResponses ? GetDistinctResponse() : GetStandardResponse();
        }

        private SagarResponse GetDistinctResponse()
        {
            List<int> ValidResponseIndices = [];
            for (var i = 0; i < SagarReplies.Unused.Count; i++)
            {
                ValidResponseIndices.AddRange(Enumerable.Repeat(i, SagarReplies.Unused[i].Weight));
            }
            int Candidate = ValidResponseIndices.PickRandom();

            var Reponse = SagarReplies.GetUnused(Candidate);
            return Reponse;
        }
        private SagarResponse GetStandardResponse()
        {
            List<int> ValidResponseIndices = [];
            for (var i = 0; i < SagarReplies.Source.Count; i++)
            {
                ValidResponseIndices.AddRange(Enumerable.Repeat(i, SagarReplies.Source[i].Weight));
            }
            int Candidate = ValidResponseIndices.PickRandom();
            return SagarReplies.Source[Candidate];
        }

        public SerializeableDiscordMessage GetRandomQuote()
        {
            if (SagarConfig.ReduceDuplicateQuotes)
            {
                var Quote = SagarQuotes.GetRandomUnused();
                return Quote;
            }
            return SagarQuotes.Source.PickRandom();
        }

        public void AddSagarQuote(RecievedMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.MessageText)) { return; }
            if (message.MessageText.StartsWith("<")) { return; }
            Console.WriteLine("Adding New Sagar Quote");
            Console.WriteLine($"{message.Author.GlobalName}: {message.MessageText}");
            SagarQuotes.AddNew(SerializeableDiscordMessage.FromDiscordMessage(message.Message));
        }

        public DiscordInteractionResponseBuilder BuildSagarQuoteReply(SerializeableDiscordMessage Quote)
        {
            var ResponseBuilder = new DiscordInteractionResponseBuilder();
            ResponseBuilder.WithContent(Quote.Link.ToString());
            var Embeds = Program._SagarismClient.GetSagarQuoteEmbeds(Quote);
            ResponseBuilder.AddEmbeds(Embeds.Select(x => x.Build()));
            return ResponseBuilder;
        }

        public DiscordMessageBuilder BuildSagarQuoteOfTheDay(SerializeableDiscordMessage Quote)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder();
            builder.WithContent(Quote.Link.ToString());
            var Embeds = Program._SagarismClient.GetSagarQuoteEmbeds(Quote, "Sagarism Quote of the Day");
            builder.AddEmbeds(Embeds.Select(x => x.Build()));
            return builder;
        }

        public List<DiscordEmbedBuilder> GetSagarQuoteEmbeds(SerializeableDiscordMessage Quote, string? Title = null)
        {
            List<DiscordEmbedBuilder> Embeds = [];
            List<DiscordAttachment> attachments = Quote.Attachments is null ? [] : Quote.Attachments;

            if (attachments.Count > 0)
            {
                foreach (var Attachment in attachments)
                {
                    var ImageEmbed = new DiscordEmbedBuilder();
                    FilterImageThroughSensor(ImageEmbed, Attachment);
                    Embeds.Add(ImageEmbed);
                }
            }
            else
            {
                Embeds.Add(new DiscordEmbedBuilder());
            }
            if (!string.IsNullOrWhiteSpace(Quote.Content))
            {
                Embeds.First().WithDescription(Quote.Content);
            }
            if (Title is not null)
            {
                Embeds.First().WithTitle(Title).WithColor(DiscordColor.Green);
            }
            Embeds.Last().WithTimestamp(Quote.TimeStamp);
            return Embeds;
        }

        public DiscordEmbedBuilder FilterImageThroughSensor(DiscordEmbedBuilder Builder, DiscordAttachment Image)
        {
            var Censors = Program._SagarismClient.ImageCensors;
            string Censor = Censors.TryGetValue(Image.Id, out string? value) ? value : string.Empty;
            if (!string.IsNullOrWhiteSpace(Censor)) { Builder.WithDescription(Censor); }
            else { Builder.WithImageUrl(Image.Url); }
            return Builder;
        }

    }

    public class SagarConfigCommands
    {
        Sagarism _Parent;
        public SagarConfigCommands(Sagarism Parent)
        {
            _Parent = Parent;
        }

        public void UpdateConfigFile()
        {
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }
            try { File.WriteAllText(StaticBotPaths.Sagarism.Files.SagarismConfig, _Parent.SagarConfig.ToFormattedJson()); }
            catch (Exception ex) { Console.WriteLine($"Failed to write Sagar config data\n{ex}"); }
        }
        public void UpdateResponseCacheFile()
        {
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }
            try { File.WriteAllText(StaticBotPaths.Sagarism.Files.SagarResponseFile, _Parent.SagarReplies.ToFormattedJson()); }
            catch (Exception ex) { Console.WriteLine($"Failed to write Sagar Response Cache\n{ex}"); }
        }
        public void UpdateQuoteCacheFile()
        {
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }
            try { File.WriteAllText(StaticBotPaths.Sagarism.Files.SagarQuotesCacheFile, _Parent.SagarQuotes.ToFormattedJson()); }
            catch (Exception ex) { Console.WriteLine($"Failed to write Sagar Quote data\n{ex}"); }
        }
        public void UpdateDailyQuoteCacheFile()
        {
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }
            try { File.WriteAllText(StaticBotPaths.Sagarism.Files.SagarDailyQuoteFile, _Parent.DailyQuoteTracking.ToFormattedJson()); }
            catch (Exception ex) { Console.WriteLine($"Failed to write Sagar Daily Quote data\n{ex}"); }
        }
        public void UpdateCronData()
        {
            if (!Directory.Exists(StaticBotPaths.Sagarism.Directories.SagarismData)) { Directory.CreateDirectory(StaticBotPaths.Sagarism.Directories.SagarismData); }
            try { File.WriteAllText(StaticBotPaths.Sagarism.Files.CronDebt, _Parent.Debt.ToFormattedJson()); }
            catch (Exception ex) { Console.WriteLine($"Failed to write Cron Debt data\n{ex}"); }
        }

        public DiscordUser[] GetReplyTargets()
        {
            List<DiscordUser> Targets = new List<DiscordUser>();
            foreach (var T in _Parent.SagarConfig.ReplyTargets)
            {
                DiscordUser? User = DiscordUtility.GetUserByIDString(T.ToString());
                if (User is null) { continue; }
                Targets.Add(User);
            }
            return [.. Targets];
        }
        public SagarOptionEditStatus AddReplyTarget(DiscordUser User)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            if (_Parent.SagarConfig.ReplyTargets.Contains(User.Id))
            {
                Result.WasError = true;
                Result.Status = $"Already sending replies to user ```{User.Username} ({User.GlobalName}  {User.Id})```";
                return Result;
            }
            _Parent.SagarConfig.ReplyTargets.Add(User.Id);
            UpdateConfigFile();
            Result.Status = $"Now replying to user ```{User.Username} ({User.GlobalName}  {User.Id})```";
            return Result;
        }
        public SagarOptionEditStatus DelReplyTarget(DiscordUser User)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            if (!_Parent.SagarConfig.ReplyTargets.Contains(User.Id))
            {
                Result.WasError = true;
                Result.Status = $"Replies were not being sent to user ```{User.Username} ({User.GlobalName}  {User.Id})```";
                return Result;
            }
            _Parent.SagarConfig.ReplyTargets.Remove(User.Id);
            UpdateConfigFile();
            Result.Status = $"No longer replying to user ```{User.Username} ({User.GlobalName}  {User.Id})```";
            return Result;
        }
        public DataStructure.Sagarism.SagarResponse[] GetReplies()
        {
            return [.. _Parent.SagarReplies.Source];
        }
        public double GetReplyChance()
        {
            return _Parent.SagarConfig.ReplyChance;
        }
        public SagarOptionEditStatus AddReply(DataStructure.Sagarism.SagarResponse Reply)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            _Parent.SagarReplies.AddNew(Reply);
            Result.Status = $"Added Reply ```{Reply.ToFormattedJson()}```";
            return Result;
        }
        public SagarOptionEditStatus DelReply(Guid ReplyID)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();

            var ToDelete = _Parent.SagarReplies.Source.FirstOrDefault(x => x.ID == ReplyID);

            if (ToDelete is null)
            {
                Result.WasError = true;
                Result.Status = $"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's";
                return Result;
            }
            _Parent.SagarReplies.Remove(ToDelete);
            Result.Status = $"Removed Reply {ToDelete.ToFormattedJson()}";
            return Result;
        }
        public SagarOptionEditStatus SetChance(int PercentChance)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            if (PercentChance < 0 || PercentChance > 100)
            {
                Result.WasError = true;
                Result.Status = "Could not set reply chance. Chance must be between 0 and 100";
                return Result;
            }
            _Parent.SagarConfig.ReplyChance = (double)PercentChance / (double)100;
            Result.Status = $"Reply chance was set to {_Parent.SagarConfig.ReplyChance * 100}%";
            UpdateConfigFile();
            return Result;
        }
        public SagarOptionEditStatus ToggleReduceDuplicateResponses()
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            _Parent.SagarConfig.ReduceDuplicateResponses = !_Parent.SagarConfig.ReduceDuplicateResponses;
            Result.Status = _Parent.SagarConfig.ReduceDuplicateResponses ? $"Reducing Duplicate Sagar Responses" : $"No Longer Reducing Duplicate Sagar Responses";
            UpdateConfigFile();
            return Result;
        }
        public SagarOptionEditStatus ToggleReduceDuplicateQuotes()
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            _Parent.SagarConfig.ReduceDuplicateQuotes = !_Parent.SagarConfig.ReduceDuplicateQuotes;
            Result.Status = _Parent.SagarConfig.ReduceDuplicateResponses ? $"Reducing Duplicate Sagar Quotes" : $"No Longer Reducing Duplicate Sagar Quotes";
            UpdateConfigFile();
            return Result;
        }

        public SagarOptionEditStatus SetReplyWeight(Guid ReplyID, int Weight)
        {
            SagarOptionEditStatus Result = new SagarOptionEditStatus();
            var ToEdit = _Parent.SagarReplies.Source.FirstOrDefault(x => x.ID == ReplyID);
            if (ToEdit is null)
            {
                Result.WasError = true;
                Result.Status = $"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's";
                return Result;
            }
            string Before = ToEdit.ToFormattedJson();
            ToEdit.Weight = Weight;
            string After = ToEdit.ToFormattedJson();
            UpdateResponseCacheFile();
            Result.Status = $"Reply ```{Before}``` Became ```{After}```";
            return Result;
        }
    }
}
