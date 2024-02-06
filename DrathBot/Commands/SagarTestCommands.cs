using DrathBot;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagarSlayer.Commands
{
    internal class SagarTestCommands : ApplicationCommandModule
    {

        [SlashCommand("SagarSpeaks", "Let him speak his holy word")]
        public async Task JoinCurrentCommand(InteractionContext ctx)
        {
            var channel = ctx.Member?.VoiceState?.Channel;
            if (channel is null)
            {
                await ctx.CreateResponseAsync("You must be in a voice channel to run this command");
                return;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Sagar Speaks").AsEphemeral());

            try
            {
                var Sounds = Directory.GetFiles(DrathBot.DataStructure.StaticBotPaths.Sagarism.Directories.SagarismSoundClips);
                var Sound = Sounds.PickRandom();
                var pcm = DiscordUtility.ConvertAudioToPcm(Sound);

                var connection = await channel.ConnectAsync();
                var transmit = connection.GetTransmitSink();

                await pcm.CopyToAsync(transmit);
                await pcm.DisposeAsync();

                connection.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //[SlashCommand("AskAI", "Send a prompt to ChatGPT")]
        public async Task AI(InteractionContext ctx, [Option("Prompt", "AI Prmpt")] string Reply)
        {
            //https://www.bytehide.com/blog/chatbot-chatgpt-csharp
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var Result = Program._ChatGPTClient.SendMessage(Reply);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(Result));

        }
    }
}
