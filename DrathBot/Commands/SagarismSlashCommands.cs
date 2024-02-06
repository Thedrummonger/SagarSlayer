using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

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
    }
}
