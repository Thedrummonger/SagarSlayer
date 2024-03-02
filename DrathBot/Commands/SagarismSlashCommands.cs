using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SagarSlayer.DataStructure;
using static SagarSlayer.DataStructure.CronDebt;

namespace DrathBot.Commands
{
    internal class SagarismSlashCommands : ApplicationCommandModule
    {

        [SlashCommand("SagarQuote", "Say a random Sagar Quote")]
        public async Task GetSagarQuote(InteractionContext ctx)
        {
            var Quote = Program._SagarismClient.GetRandomQuote();
            await ctx.CreateResponseAsync(Program._SagarismClient.BuildSagarQuoteReply(Quote));

        }

        [SlashCommand("GetUserID", "Gets the discord ID of a user")]
        public async Task GetUserID(InteractionContext ctx, [Option("User", "User to get the ID of")] DiscordUser user)
        {
            await ctx.CreateResponseAsync($"User ID: {user.Id}", true);
        }

        [SlashCommand("TellUser", "Say something in general")]
        public async Task TellUser(InteractionContext ctx, [Option("Message", "Message to say")] string Reply, [Option("User", "User to ping")] DiscordUser? user = null)
        {
            await ctx.CreateResponseAsync("Sending", true);

            var Channel = await Program._DiscordBot.Client.GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
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

        [SlashCommand("TellUsers", "'TellUser' but can ping multiple users or users not in this server")]
        public async Task TellUsers(InteractionContext ctx, [Option("Message", "Message to say")] string Reply, [Option("Ping", "Discord ID to Ping (separate multiple with commas)")] string UserID = "")
        {
            await ctx.CreateResponseAsync("Sending", true);

            var Channel = await Program._DiscordBot.Client.GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
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

        [SlashCommand("PrintCronDebt", "Get Sagars Cron Debt")]
        public async Task PrintDebt(InteractionContext ctx, [Option("Currency", "Debt Currency")] CronDebt.Currency type)
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Sagar is {Quote} {Currency} in debt"));
        }

        [SlashCommand("AddCronDebt", "Add the given value to Jordans Cron Debt")]
        public async Task AddDebt(InteractionContext ctx, [Option("Currency", "Debt Currency")] CronDebt.Currency type, [Option("Amount", "Amount to add to debt")] long Amount)
        {
            await UpdateCronDebt(ctx, type, Amount, false);
        }

        [SlashCommand("SetCronDebt", "Sets the value of Jordans Cron Debt")]
        public async Task SetDebt(InteractionContext ctx, [Option("Currency", "Debt Currency")] CronDebt.Currency type, [Option("Amount", "Current Debt Value")] long Amount)
        {
            await UpdateCronDebt(ctx, type, Amount, true);
        }

        public async Task UpdateCronDebt(InteractionContext ctx, CronDebt.Currency type, long Amount, bool Set)
        {
            ulong DebtAmount = type == CronDebt.Currency.Silver ? (ulong)Amount : (ulong)Amount * CronDebt.SilverConversionRate;
            ulong Current = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            if (Set) { Program._SagarismClient.Debt.SetSilverDebt(DebtAmount, ctx.User); }
            else { Program._SagarismClient.Debt.UpdateSilverDebt(DebtAmount, ctx.User); }
            Program._SagarismClient.Commands.UpdateCronData();
            if (Program._SagarismClient.SagarConfig.UserStatus is null) { await Program._SagarismClient.SetDebtAsStatus(); }
            ulong New = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            string CurrencyName = type == CronDebt.Currency.Silver ? "Silver" : "Cron";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Sagar Debt was updated from {Current} to {New} {CurrencyName}"));
        }

        [SlashCommand("UndoLastTransaction", "Reverts the last Debt Change")]
        public async Task UndoLastDebtChange(InteractionContext ctx)
        {
            var LastTransaction = Program._SagarismClient.Debt.UndoLastTransaction();
            if (LastTransaction is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"No transactions exist to undo"));
                return;
            }
            Program._SagarismClient.Commands.UpdateCronData();
            if (Program._SagarismClient.SagarConfig.UserStatus is null) { await Program._SagarismClient.SetDebtAsStatus(); }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Reverted Transaction adding {LastTransaction.SilverAdded} to {LastTransaction.PreviousValue} resulting in {LastTransaction.NewValue}\n" +
                $"{LastTransaction.PreviousValue} is now the current value"));
        }
    }
}
