using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;

namespace DrathBot.Commands
{
    internal class SagarConfigSlashCommands : ApplicationCommandModule
    {

        [SlashCommand("Help", "Shows Available Commands")]
        public async Task ShowHelp(InteractionContext ctx)
        {
            StringBuilder sb = new();
            sb.AppendLine("Available Commands:");
            sb.AppendLine();
            sb.AppendLine("/SeeActiveReplies").Append("```");
            sb.Append("Shows a list of replies the bot will use along with the reply ID").AppendLine("```");

            sb.AppendLine("/SeeReplyTargets").Append("```");
            sb.Append("Shows a list of the users the bot will reply to").AppendLine("```");

            sb.AppendLine("/SeeReplyChance").Append("```");
            sb.Append("Returns the current chance the will reply to a a user in the Reply Targets List").AppendLine("```");

            sb.AppendLine("/SetReplyChance").Append("```");
            sb.Append("Sets the chance the bot will reply to users in the Reply Targets List").AppendLine("```");

            sb.AppendLine("/SetReplyWeight").Append("```");
            sb.Append("Sets the frequency a reply will be used. A reply with a higher weight will be used more often than replies with lower weights").AppendLine("```");

            sb.AppendLine("/AddNewReply").Append("```");
            sb.Append("Adds a the given reply to the reply list").AppendLine("```");

            sb.AppendLine("/DeleteReply").Append("```");
            sb.Append("Deletes the given reply. Use SeeActiveReplies to get the replies ID").AppendLine("```");

            sb.AppendLine("/AddNewReplyTarget").Append("```");
            sb.Append("Adds a user that the bot will reply to").AppendLine("```");

            sb.AppendLine("/AddNewReplyTargetByID").Append("```");
            sb.Append("Same as AddNewReplyTarget but can accept the users ID. This allow you to add users that are not in the current server").AppendLine("```");

            sb.AppendLine("/DeleteReplyTarget").Append("```");
            sb.Append("Removes a user from the bots reply list").AppendLine("```");

            sb.AppendLine("/DeleteReplyTargetByID").Append("```");
            sb.Append("Same as DeleteReplyTarget but can accept the users ID. This allow you to remove users that are not in the current server").AppendLine("```");

            sb.AppendLine("/ToggleReduceDuplicateResponses").Append("```");
            sb.Append("If enabled, after a response is sent, the program will go trhough at least half of the available responses before reusing that response").AppendLine("```");

            sb.AppendLine("/ToggleReduceDuplicateQuotes").Append("```");
            sb.Append("Same as above but for quotes").AppendLine("```");

            sb.AppendLine("/setactivity").Append("```");
            sb.Append("Sets the bots activity").AppendLine("```");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(sb.ToString()));
        }

        [SlashCommand("SetReplyChance", "Sets the chance the bot will reply to Jordan")]
        public async Task SetReplyChance(InteractionContext ctx, [Option("chance", "Percent Chance")] double Chance)
        {
            var Result = Program._SagarismClient.Commands.SetChance((int)Chance);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));

        }
        [SlashCommand("SetReplyWeight", "Sets the Weight of the given reply")]
        public async Task SetReplyChance(InteractionContext ctx, [Option("ReplyID", "Reply ID (found with SeeActiveReplies)")] string ReplyID, [Option("weight", "Reply Weight")] double Chance)
        {
            if (!Guid.TryParse(ReplyID, out Guid ReplyGUID))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's").AsEphemeral());
                return;
            }
            var Result = Program._SagarismClient.Commands.SetReplyWeight(ReplyGUID, (int)Chance);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));

        }
        [SlashCommand("SeeReplyChance", "Returns the current Reply Chance")]
        public async Task SeeReplyChance(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Current Reply Chance is {Program._SagarismClient.Commands.GetReplyChance() * 100}%"));
        }
        [SlashCommand("SeeActiveReplies", "List all active replies and their ID")]
        public async Task SeeActiveReplies(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Active replies are ```{Program._SagarismClient.Commands.GetReplies().ToFormattedJson()}```"));
        }
        [SlashCommand("AddNewReply", "Adds a new reply")]
        public async Task AddNewReply(InteractionContext ctx, [Option("Reply", "New Reply to add")] string Reply)
        {
            DataStructure.Sagarism.SagarResponse NewResponse = new DataStructure.Sagarism.SagarResponse(Reply);
            var Result = Program._SagarismClient.Commands.AddReply(NewResponse);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [SlashCommand("DeleteReply", "Deletes a reply")]
        public async Task DeleteReply(InteractionContext ctx, [Option("ReplyID", "Reply ID (found with SeeActiveReplies)")] string ReplyID)
        {
            if (!Guid.TryParse(ReplyID, out Guid ReplyGUID))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"ID {ReplyID} was not a valid reply ID. Use /See Active Replies to see all reply ID's").AsEphemeral());
                return;
            }
            var Result = Program._SagarismClient.Commands.DelReply(ReplyGUID);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [SlashCommand("SeeReplyTargets", "List all active replies and their ID")]
        public async Task SeeReplyTargets(InteractionContext ctx)
        {
            var Users = Program._SagarismClient.Commands.GetReplyTargets();
            if (!Users.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
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

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent(Response.ToString()));
        }
        [SlashCommand("AddNewReplyTarget", "Adds a New Reply Target")]
        public async Task AddNewReplyTarget(InteractionContext ctx, [Option("User", "Discord User To Add")] DiscordUser User)
        {
            var Result = Program._SagarismClient.Commands.AddReplyTarget(User);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [SlashCommand("AddNewReplyTargetByID", "Adds a New Reply Target By User ID")]
        public async Task AddNewReplyTargetID(InteractionContext ctx, [Option("UserID", "Discord User ID To Add")] string UserID)
        {
            var User = DiscordUtility.GetUserByIDString(UserID);
            if (User is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Invalid UserID").AsEphemeral());
                return;
            }
            await AddNewReplyTarget(ctx, User);
        }
        [SlashCommand("DeleteReplyTarget", "Deletes a Reply Target")]
        public async Task DeleteReplyTarget(InteractionContext ctx, [Option("User", "Discord User To Add")] DiscordUser User)
        {
            var Result = Program._SagarismClient.Commands.DelReplyTarget(User);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }
        [SlashCommand("DeleteReplyTargetByID", "Deletes a Reply Target By User ID")]
        public async Task DeleteReplyTargetID(InteractionContext ctx, [Option("UserID", "Discord User ID To Add")] string UserID)
        {
            var User = DiscordUtility.GetUserByIDString(UserID);
            if (User is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Invalid UserID").AsEphemeral());
                return;
            }
            await DeleteReplyTarget(ctx, User);
        }
        [SlashCommand("UpdateSagarQuotes", "Manually updates the Cache of Sagar Quotes from the Sagar Quotes channel")]
        public async Task GetQuotes(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var allMessages = DiscordUtility.GetAllMessagesInChannel(Program._SagarismClient.SagarConfig.DiscordData.GetQuotesChannel());

            Console.WriteLine($"Got {allMessages.Length} Total Messages");

            List<DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage> SagarQuotes = [];
            foreach (var message in allMessages)
            {
                var NewMessage = DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage.FromDiscordMessage(message);
                SagarQuotes.Add(NewMessage);
            }

            SagarQuotes = [.. SagarQuotes.OrderBy(x => x.TimeStamp)];

            Console.WriteLine($"Got {allMessages.Length} Valid Messages");

            DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage[] CurrentUsed = [.. Program._SagarismClient.SagarQuotes.Used];

            Console.WriteLine($"Got {allMessages.Length} Valid Messages");

            var NewQuotes = new SagarSlayer.DataStructure.Misc.DistinctList<DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage>(SagarQuotes, Program._SagarismClient.SagarQuotes.refreshDec);
            Program._SagarismClient.SagarQuotes = NewQuotes;

            List<int> IndexsToSetUsed = [];
            foreach(var i in CurrentUsed)
            {
                var NewEntry = Program._SagarismClient.SagarQuotes.Unused.FirstOrDefault(x => x.MessageID == i.MessageID);
                if (NewEntry is null) { continue; }
                IndexsToSetUsed.Add(Program._SagarismClient.SagarQuotes.Unused.IndexOf(NewEntry));
            }
            Program._SagarismClient.SagarQuotes.SetMessagesUsed(IndexsToSetUsed);
            Program._SagarismClient.Commands.UpdateQuoteCacheFile();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Got {SagarQuotes.Count} Messages"));
        }

        [SlashCommand("ToggleReduceDuplicateResponses", "The program will attempt to use unique responses")]
        public async Task ToggleReduceDuplicateResponses(InteractionContext ctx)
        {
            var Result = Program._SagarismClient.Commands.ToggleReduceDuplicateResponses();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [SlashCommand("ToggleReduceDuplicateQuotes", "The program will attempt to use unique quotes")]
        public async Task ToggleReduceDuplicateQuotes(InteractionContext ctx)
        {
            var Result = Program._SagarismClient.Commands.ToggleReduceDuplicateQuotes();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(Result.Status).AsEphemeral(Result.WasError));
        }

        [SlashCommand("SetActivity", "Sets the bots activity")]
        private async Task setactivity(InteractionContext ctx, [Option("type", "Activity Type")] ActivityType type, [Option("name", "Activity Name")] string name)
        {
            await Program._SagarismClient.SetStatus(new DiscordActivity(name, type));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Status updated"));
        }
        [SlashCommand("SetActivityCronDebt", "Shows the current debt as the activity")]
        private async Task setactivityCronDebt(InteractionContext ctx)
        {
            await Program._SagarismClient.SetDebtAsStatus();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Status updated"));
        }

        [SlashCommand("QuoteOfTheDay", "Forces the bot to post a Sagar QOTD")]
        private async Task ForceQuote(InteractionContext ctx)
        {
            var Quote = Program._SagarismClient.GetRandomQuote();
            //var Quote = Program._SagarismClient.SagarQuotes.Source.First(x => x.Attachments is not null && x.Attachments.Count > 1);

            Program._SagarismClient.Commands.UpdateDailyQuoteCacheFile();

            var Channel = await Program._DiscordBot.Client.GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Sending QOTD"));
            await Program._DiscordBot.Client.SendMessageAsync(Channel, Program._SagarismClient.BuildSagarQuoteOfTheDay(Quote));
        }

        [SlashCommand("devTest", "Dev Testing")]
        private async Task DevTest(InteractionContext ctx)
        {
            var Quote = Program._SagarismClient.SagarQuotes.Source.First(x => x.MessageID == 765343459049209906);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                Program._SagarismClient.BuildSagarQuoteReply(Quote));
        }

    }
}
