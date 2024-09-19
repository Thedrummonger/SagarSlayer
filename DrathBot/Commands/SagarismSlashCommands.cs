using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using SagarSlayer.DataStructure;
using System.ComponentModel;
using TDMUtils;
using static SagarSlayer.DataStructure.CronDebt;

namespace DrathBot.Commands
{
    internal class SagarismSlashCommands
    {

        [Command("SagarQuote"), Description("Say a random Sagar Quote")]
        public async Task GetSagarQuote(CommandContext ctx)
        {
            var Quote = Program._SagarismClient.GetRandomSagarQuote();
            if (Quote is null)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("No quotes available").AsEphemeral());
                return;
            }
            await ctx.RespondAsync(Program._SagarismClient.BuildSagarQuoteReply(Quote));
        }

        [Command("Quote"), Description("Say a random Quote")]
        public async Task GetMiscQuote(CommandContext ctx, [Parameter("QuotedUser"), Description("Get a quote from a specific person")] string? user = null)
        {
            DataStructure.ExtendedDiscordObjects.SerializeableDiscordMessage? Quote = null;
            if (user is not null) { Quote = Program._SagarismClient.GetRandomMiscQuoteWithSubject(user.ToLower());}
            else { Quote = Program._SagarismClient.GetRandomMiscQuote(); }
            if (Quote is null)
            {
                if (user is not null) 
                {
                    var Users = DiscordUtility.GetAllRelevantUsers(Program._SagarismClient.MiscQuotes.Source);
                    var Matches = DiscordUtility.GetClosestMatch(user, Users);
                    await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Could not find any quotes for user {user}. did you mean one of these? [{string.Join(", ", Matches)}]").AsEphemeral()); 
                }
                else { await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("No quotes available").AsEphemeral()); }
                return;
            }
            await ctx.RespondAsync(Program._SagarismClient.BuildSagarQuoteReply(Quote));
        }

        [Command("GetUserID"), Description("Gets the discord ID of a user")]
        public async Task GetUserID(CommandContext ctx, [Parameter("User"), Description("User to get the ID of")] DiscordUser user)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"User ID: {user.Id}").AsEphemeral());
        }

        [Command("TellUser"), Description("Say something in general")]
        public async Task TellUser(CommandContext ctx, [Parameter("Message"), Description("Message to say")] string Reply, [Parameter("User"), Description("User to ping")] DiscordUser? user = null)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("Sending").AsEphemeral());

            var Channel = await Program._DiscordBot.GetClient().GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
            var builder = new DiscordMessageBuilder();
            string Message = string.Empty;
            if (user is not null)
            {
                builder.WithAllowedMention(new UserMention(user));
                Message += $"{user.Mention} ";
            }
            Message += Reply;
            builder.WithContent(Message);
            await builder.SendAsync(Channel);
        }

        [Command("TellUsers"), Description("'TellUser' but can ping multiple users or users not in this server")]
        public async Task TellUsers(CommandContext ctx, [Parameter("Message"), Description("Message to say")] string Reply, [Parameter("Ping"), Description("Discord ID to Ping (separate multiple with commas)")] string UserID = "")
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent("Sending").AsEphemeral());

            var Channel = await Program._DiscordBot.GetClient().GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
            var builder = new DiscordMessageBuilder();
            string Message = string.Empty;

            if (!string.IsNullOrWhiteSpace(UserID))
            {
                Console.WriteLine($"ID string Passed [{UserID}]" );
                var IDs = UserID.TrimSplit(",");
                List<IMention> mentions = [];
                foreach(var ID in IDs)
                {
                    Console.WriteLine($"ID [{ID}]");
                    var U = await DiscordUtility.GetUserByIDString(ID);
                    if (U is null) { Console.WriteLine($"Failed to get User"); continue; }
                    mentions.Add(new UserMention(U));
                    Message += $"{U.Mention} ";
                    Console.WriteLine($"Pinging User {U.Username} {U.Mention}");
                }
                builder.WithAllowedMentions(mentions);
            }
            Message += Reply;
            builder.WithContent(Message);
            await builder.SendAsync(Channel);
        }

        [Command("PrintCronDebt"), Description("Get Sagars Cron Debt")]
        public async Task PrintDebt(CommandContext ctx, [Parameter("Currency"), Description("Debt Currency")] CronDebt.Currency type)
        {
            string Currency;
            ulong Quote;
            switch (type)
            {
                case CronDebt.Currency.Cron:
                    Quote = Program._SagarismClient.Debt.GetCronDebt();
                    Currency = "Cron";
                    break;
                case CronDebt.Currency.Silver:
                    Quote = Program._SagarismClient.Debt.GetSilverDebt();
                    Currency = "Silver";
                    break;
                default:
                    Quote = 0;
                    Currency = "Unknown Currency";
                    break;
            }
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Sagar is {Quote} {Currency} in debt"));
        }

        [Command("AddCronDebt"), Description("Add the given value to Jordans Cron Debt")]
        public async Task AddDebt(CommandContext ctx, [Parameter("Currency"), Description("Debt Currency")] CronDebt.Currency type, [Parameter("Amount"), Description("Amount to add to debt")] long Amount)
        {
            await UpdateCronDebt(ctx, type, Amount, false);
        }

        [Command("SetCronDebt"), Description("Sets the value of Jordans Cron Debt")]
        public async Task SetDebt(CommandContext ctx, [Parameter("Currency"), Description("Debt Currency")] CronDebt.Currency type, [Parameter("Amount"), Description("Current Debt Value")] long Amount)
        {
            await UpdateCronDebt(ctx, type, Amount, true);
        }

        public async Task UpdateCronDebt(CommandContext ctx, CronDebt.Currency type, long Amount, bool Set)
        {
            ulong DebtAmount = type == CronDebt.Currency.Silver ? (ulong)Amount : (ulong)Amount * CronDebt.SilverConversionRate;
            ulong Current = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            if (Set) { Program._SagarismClient.Debt.SetSilverDebt(DebtAmount, ctx.User); }
            else { Program._SagarismClient.Debt.UpdateSilverDebt(DebtAmount, ctx.User); }
            Program._SagarismClient.Commands.UpdateCronData();
            if (Program._SagarismClient.SagarConfig.UserStatus is null) { await Program._SagarismClient.SetDebtAsStatus(); }
            ulong New = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            string CurrencyName = type == CronDebt.Currency.Silver ? "Silver" : "Cron";

            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Sagar Debt was updated from {Current} to {New} {CurrencyName}"));
        }

        [Command("UndoLastTransaction"), Description("Reverts the last Debt Change")]
        public async Task UndoLastDebtChange(CommandContext ctx)
        {
            var LastTransaction = Program._SagarismClient.Debt.UndoLastTransaction();
            if (LastTransaction is null)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"No transactions exist to undo"));
                return;
            }
            Program._SagarismClient.Commands.UpdateCronData();
            if (Program._SagarismClient.SagarConfig.UserStatus is null) { await Program._SagarismClient.SetDebtAsStatus(); }

            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .WithContent($"Reverted Transaction adding {LastTransaction.SilverAdded} to {LastTransaction.PreviousValue} resulting in {LastTransaction.NewValue}\n" +
                $"{LastTransaction.PreviousValue} is now the current value"));
        }
    }
}
