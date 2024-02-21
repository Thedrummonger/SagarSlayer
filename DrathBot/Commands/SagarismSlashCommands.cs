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

        [SlashCommand("TellSagar", "Say something to Sagar")]
        public async Task TellSagar(InteractionContext ctx, [Option("Message", "Message to say")] string Reply, [Option("Ping", "Should the message Ping Sagar")] bool Ping)
        {
            await ctx.CreateResponseAsync("Sending", true);

            var Sagar = await Program._DiscordBot.Client.GetUserAsync(Program._SagarismClient.SagarConfig.DiscordData.GetSagarUser());
            var Channel = await Program._DiscordBot.Client.GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());
            var builder = new DiscordMessageBuilder();

            if (Ping) { builder.WithAllowedMentions(new IMention[] { new UserMention(Sagar) }).WithContent($"{Sagar.Mention} {Reply}"); }
            else { builder.WithContent($"{Reply}"); }
            var Message = await builder.SendAsync(Channel);
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
            ulong DebtAddition = type == CronDebt.Currency.Silver ? (ulong)Amount : (ulong)Amount * CronDebt.SilverConversionRate;
            ulong Current = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            Program._SagarismClient.Debt.UpdateSilverDebt(DebtAddition, ctx.User);
            Program._SagarismClient.Commands.UpdateCronData();
            ulong New = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            string CurrencyName = type == CronDebt.Currency.Silver ? "Silver" : "Cron";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder().WithContent($"Sagar Debt was updated from {Current} to {New} {CurrencyName}"));
        }

        [SlashCommand("SetCronDebt", "Sets the value of Jordans Cron Debt")]
        public async Task SetDebt(InteractionContext ctx, [Option("Currency", "Debt Currency")] CronDebt.Currency type, [Option("Amount", "Current Debt Value")] long Amount)
        {
            ulong DebtAmount = type == CronDebt.Currency.Silver ? (ulong)Amount : (ulong)Amount * CronDebt.SilverConversionRate;
            ulong Current = type == CronDebt.Currency.Silver ? Program._SagarismClient.Debt.GetSilverDebt() : Program._SagarismClient.Debt.GetCronDebt();
            Program._SagarismClient.Debt.SetSilverDebt(DebtAmount, ctx.User);
            Program._SagarismClient.Commands.UpdateCronData();
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

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Reverted Transaction adding {LastTransaction.SilverAdded} to {LastTransaction.PreviousValue} resulting in {LastTransaction.NewValue}\n" +
                $"{LastTransaction.PreviousValue} is now the current value"));
        }
    }
}
