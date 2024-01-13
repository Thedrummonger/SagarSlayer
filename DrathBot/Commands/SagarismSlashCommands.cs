using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

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

        //[SlashCommand("AskAI", "Send a prompt to ChatGPT")]
        public async Task SetReplyChance(InteractionContext ctx, [Option("Prompt", "AI Prmpt")] string Reply)
        {
            //https://www.bytehide.com/blog/chatbot-chatgpt-csharp
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var Result = Program._ChatGPTClient.SendMessage(Reply);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(Result));

        }
    }
}
