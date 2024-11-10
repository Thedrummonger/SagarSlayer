using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using System.ComponentModel;
using System.Text;
using TDMUtils;

namespace DrathBot.Commands
{
    internal class SagarConfigSlashCommands
    {

        [Command("SetReplyChance")]
        [Description("Sets the chance the bot will reply to Jordan")]
        public async Task SetReplyChance(CommandContext ctx, [Parameter("chance"), Description("Percent Chance")] double Chance)
        {
            var Result = Program._SagarismClient.Commands.SetChance((int)Chance);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));

        }
        [Command("SetReplyWeight")]
        [Description("Sets the Weight of the given reply")]
        public async Task SetReplyChance(CommandContext ctx, 
            [Parameter("ReplyID"), Description("Reply ID (found with SeeActiveReplies)")] string ReplyID, 
            [Parameter("weight"), Description("Reply Weight")] double Chance)
        {
            if (!Guid.TryParse(ReplyID, out Guid ReplyGUID))
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                    .WithContent($"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's").AsEphemeral());
                return;
            }
            var Result = Program._SagarismClient.Commands.SetReplyWeight(ReplyGUID, (int)Chance);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));

        }
        [Command("SeeReplyChance")]
        [Description("Returns the current Reply Chance")]
        public async Task SeeReplyChance(CommandContext ctx)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .WithContent($"Current Reply Chance is {Program._SagarismClient.Commands.GetReplyChance() * 100}%"));
        }
        [Command("SeeActiveReplies")]
        [Description("List all active replies and their ID")]
        public async Task SeeActiveReplies(CommandContext ctx)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .WithContent($"Active replies are ```{Program._SagarismClient.Commands.GetReplies().ToFormattedJson()}```"));
        }
        [Command("AddNewReply")]
        [Description("Adds a new reply")]
        public async Task AddNewReply(CommandContext ctx, [Parameter("Reply"), Description("New Reply to add")] string Reply)
        {
            DataStructure.Sagarism.SagarResponse NewResponse = new DataStructure.Sagarism.SagarResponse(Reply);
            var Result = Program._SagarismClient.Commands.AddReply(NewResponse);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [Command("DeleteReply")]
        [Description("Deletes a reply")]
        public async Task DeleteReply(CommandContext ctx, [Parameter("ReplyID"), Description("Reply ID (found with SeeActiveReplies)")] string ReplyID)
        {
            if (!Guid.TryParse(ReplyID, out Guid ReplyGUID))
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                    .WithContent($"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's").AsEphemeral());
                return;
            }
            var Result = Program._SagarismClient.Commands.DelReply(ReplyGUID);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [Command("SeeReplyTargets")]
        [Description("List all active replies and their ID")]
        public async Task SeeReplyTargets(CommandContext ctx)
        {
            var Users = await Program._SagarismClient.Commands.GetReplyTargets();
            if (Users.Length == 0)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .WithContent("Not replying to any users"));
            }
            StringBuilder Response = new StringBuilder();
            Response.AppendLine("Replying to Users");
            Response.Append("```");
            foreach (var User in Users)
            {
                Response.AppendLine($"{User.Username} ({User.GlobalName}  {User.Id})");
            }
            Response.Append("```");

            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .WithContent(Response.ToString()));
        }
        [Command("AddNewReplyTarget")]
        [Description("Adds a New Reply Target")]
        public async Task AddNewReplyTarget(CommandContext ctx, [Parameter("User"), Description("Discord User To Add")] DiscordUser User)
        {
            var Result = Program._SagarismClient.Commands.AddReplyTarget(User);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [Command("AddNewReplyTargetByID")]
        [Description("Adds a New Reply Target By User ID")]
        public async Task AddNewReplyTargetID(CommandContext ctx, [Parameter("UserID"), Description("Discord User ID To Add")] string UserID)
        {
            var User = await DiscordUtility.GetUserByIDString(UserID);
            if (User is null)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Invalid UserID").AsEphemeral());
                return;
            }
            await AddNewReplyTarget(ctx, User);
        }
        [Command("DeleteReplyTarget")]
        [Description("Deletes a Reply Target")]
        public async Task DeleteReplyTarget(CommandContext ctx, [Parameter("User"), Description("Discord User To Add")] DiscordUser User)
        {
            var Result = Program._SagarismClient.Commands.DelReplyTarget(User);
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [Command("DeleteReplyTargetByID")]
        [Description("Deletes a Reply Target By User ID")]
        public async Task DeleteReplyTargetID(CommandContext ctx, [Parameter("UserID"), Description("Discord User ID To Add")] string UserID)
        {
            var User = await DiscordUtility.GetUserByIDString(UserID);
            if (User is null)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Invalid UserID").AsEphemeral());
                return;
            }
            await DeleteReplyTarget(ctx, User);
        }

        [Command("UpdateQuoteCash")]
        [Description("Manually updates the Cache of Quotes from the Quotes channel")]
        public async Task UpdateQuoteCache(CommandContext ctx, [Parameter("type"), Description("Quote Type")] DataStructure.Sagarism.QuoteType type)
        {
            await ctx.DeferResponseAsync();

            Console.WriteLine($"Getting all messages from Quote Channel");

            DiscordMessage[] AllMessages = type switch
            {
                DataStructure.Sagarism.QuoteType.SagarQuote => await DiscordUtility.GetAllMessagesInChannel(Program._SagarismClient.SagarConfig.DiscordData.GetSagarQuotesChannel()),
                DataStructure.Sagarism.QuoteType.MiscQuote => await DiscordUtility.GetAllMessagesInChannel(Program._SagarismClient.SagarConfig.DiscordData.GetMiscQuotesChannel()),
                _ => throw new Exception($"{type} was not a valid quote type"),
            };
            Console.WriteLine($"Got {AllMessages.Length} Total Messages From Channel");

            List<DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage> DeserializedQuotes =
                [.. AllMessages.Select(DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage.FromDiscordMessage).OrderBy(x => x.TimeStamp)];

            Console.WriteLine($"Deserialized {DeserializedQuotes.Count} Messages to SerializeableDiscordMessage");

            RandomCycleList<DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage> CurrentQuoteCache = type switch
            {
                DataStructure.Sagarism.QuoteType.SagarQuote => Program._SagarismClient.SagarQuotes,
                DataStructure.Sagarism.QuoteType.MiscQuote => Program._SagarismClient.MiscQuotes,
                _ => throw new Exception($"{type} was not a valid quote type"),
            };

            Console.WriteLine($"Current Quote Cache had {CurrentQuoteCache.Source.Count} Messages");

            var NewQuotes = new RandomCycleList<DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage>(DeserializedQuotes, CurrentQuoteCache.refreshDec);

            Console.WriteLine($"Created New Quote Cache {NewQuotes.Source.Count} Messages");

            List<int> IndexsToSetUsed = [];
            foreach (var i in CurrentQuoteCache.Used)
            {
                var NewEntry = NewQuotes.Unused.FirstOrDefault(x => x.MessageID == i.MessageID);
                if (NewEntry is null) { continue; }
                IndexsToSetUsed.Add(NewQuotes.Unused.IndexOf(NewEntry));
            }
            Console.WriteLine($"Setting {IndexsToSetUsed.Count} Messages used");
            NewQuotes.SetMessagesUsed(IndexsToSetUsed);

            switch (type)
            {
                case DataStructure.Sagarism.QuoteType.SagarQuote:
                    Program._SagarismClient.SagarQuotes.Override(NewQuotes);
                    break;
                case DataStructure.Sagarism.QuoteType.MiscQuote:
                    Program._SagarismClient.MiscQuotes.Override(NewQuotes);
                    break;
                default:
                    throw new Exception($"{type} was not a valid quote type");
            }
            Console.WriteLine($"Overrode Program Quotes");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Got {NewQuotes.Source.Count} Messages"));
        }

        [Command("ToggleReduceDupeResponses")]
        [Description("The program will attempt to use unique responses")]
        public async Task ToggleReduceDuplicateResponses(CommandContext ctx)
        {
            var Result = Program._SagarismClient.Commands.ToggleReduceDuplicateResponses();
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [Command("ToggleReduceDuplicateQuotes")]
        [Description("The program will attempt to use unique quotes")]
        public async Task ToggleReduceDuplicateQuotes(CommandContext ctx)
        {
            var Result = Program._SagarismClient.Commands.ToggleReduceDuplicateQuotes();
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [Command("SetActivity")]
        [Description("Sets the bots activity")]
        private async Task setactivity(CommandContext ctx, [Parameter("type"), Description("Activity Type")] DiscordActivityType type, [Parameter("name"), Description("Activity Name")] string name)
        {
            await Program._SagarismClient.SetStatus(new DiscordActivity(name, type));
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("Status updated"));
        }
        [Command("SetActivityCronDebt")]
        [Description("Shows the current debt as the activity")]
        private async Task setactivityCronDebt(CommandContext ctx)
        {
            await Program._SagarismClient.SetDebtAsStatus();
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("Status updated"));
        }

        [Command("QuoteOfTheDay")]
        [Description("Forces the bot to post a Sagar QOTD")]
        private async Task ForceQuote(CommandContext ctx)
        {
            var Quote = Program._SagarismClient.GetRandomSagarQuote();
            //var Quote = Program._SagarismClient.SagarQuotes.Source.First(x => x.Attachments is not null && x.Attachments.Count > 1);

            Program._SagarismClient.Commands.UpdateDailyQuoteCacheFile();

            var Channel = await Program._DiscordBot.GetClient().GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("Sending QOTD"));
            await Program._DiscordBot.GetClient().SendMessageAsync(Channel, Program._SagarismClient.BuildSagarQuoteOfTheDay(Quote));
        }

        [Command("devTest")]
        [Description("Dev Testing")]
        private async Task DevTest(CommandContext ctx)
        {
            var Quote = Program._SagarismClient.SagarQuotes.Source.First(x => x.MessageID == 765343459049209906);

            await ctx.RespondAsync(Program._SagarismClient.BuildSagarQuoteReply(Quote));
        }

    }
}
