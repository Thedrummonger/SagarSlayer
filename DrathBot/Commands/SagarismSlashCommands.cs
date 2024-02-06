using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;

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
        public async Task TellSagar(InteractionContext ctx, [Option("Message", "Message to say")] string Reply)
        {
            var Sagar = await Program._DiscordBot.Client.GetUserAsync(Program._SagarismClient.SagarConfig.DiscordData.GetSagarUser());

            await ctx.CreateResponseAsync("Sending", true);

            var Channel = await Program._DiscordBot.Client.GetChannelAsync(Program._SagarismClient.SagarConfig.DiscordData.GetGeneralChannel());

            var builder = await new DiscordMessageBuilder()
            .WithContent($"{Sagar.Mention}{Reply}")
            .WithAllowedMentions(new IMention[] { new UserMention(Sagar) })
            .SendAsync(Channel);

        }
    }
}
