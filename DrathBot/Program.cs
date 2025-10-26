using DrathBot.Commands;
using DrathBot.DataStructure;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.VoiceNext;
using SagarSlayer.AI;
using SagarSlayer.DataStructure;
using SagarSlayer.Lib;
using System.Diagnostics;
using TDMUtils;
using static DrathBot.DataStructure.ExtendedDiscordObjects;
using static DrathBot.DataStructure.Sagarism;

namespace DrathBot
{
    internal class Program
    {
        public static bool IsDebug { get { return Debugger.IsAttached && !RunAsLive; } }

        public static bool RunAsLive = false;

        public static ExtendedDiscordObjects.DiscordBot _DiscordBot { get; private set; }
        public static ChatGPTClient _ChatGPTClient { get; private set; }
        public static MessageHandling.Sagarism _SagarismClient { get; private set; }
        static async Task Main(string[] args)
        {
            languageLib.InitializeCommonWords();
            Console.WriteLine($"Debug Mode: {IsDebug}");
            //Validate Data Files
            Console.WriteLine($"Validating AppData Directory");
            VerifyDataFiles();

            //Register Clients
            var OPenAIData = DataFileUtilities.LoadObjectFromFileOrDefault(StaticBotPaths.Sagarism.Files.AIData, new AI.ChatGPTData(), false);
            _ChatGPTClient = new ChatGPTClient(OPenAIData.APIKey);
            _SagarismClient = new MessageHandling.Sagarism();
            _DiscordBot = new ExtendedDiscordObjects.DiscordBot(_SagarismClient.SagarConfig.DiscordData.GetBotKey());

            _DiscordBot.GetBuilder().UseVoiceNext(new VoiceNextConfiguration());

            _DiscordBot.GetBuilder().UseCommands(extension =>
            {
                extension.AddCommands(typeof(SagarConfigSlashCommands), [_SagarismClient.SagarConfig.DiscordData.TestServer.ServerID]);
                extension.AddCommands(typeof(SagarismSlashCommands));
                SlashCommandProcessor processor = new SlashCommandProcessor();
                extension.AddProcessors(processor);
            },
            new CommandsConfiguration()
            {
                DebugGuildId = _SagarismClient.SagarConfig.DiscordData.TestServer.ServerID,
            });

            _DiscordBot.GetBuilder().ConfigureEventHandlers(handler =>
            {
                handler.HandleMessageCreated(MessageHandling.ParseMessage._Client_MessageCreated);
                handler.HandleSessionCreated(Client_SessionCreated);
            });
            Console.WriteLine($"Initializing Discord Client");

            _DiscordBot.Build();

            await _DiscordBot.GetClient().ConnectAsync();

            var GlobalCommands = await _DiscordBot.GetClient().GetGlobalApplicationCommandsAsync();
            await _DiscordBot.GetClient().BulkOverwriteGlobalApplicationCommandsAsync(GlobalCommands);
            if (!IsDebug)
            {
                var ProdGuildCommands = await _DiscordBot.GetClient().GetGuildApplicationCommandsAsync(_SagarismClient.SagarConfig.DiscordData.ProdServer.ServerID);
                await _DiscordBot.GetClient().BulkOverwriteGuildApplicationCommandsAsync(_SagarismClient.SagarConfig.DiscordData.ProdServer.ServerID, ProdGuildCommands);
            }
            var TestGuildCommands = await _DiscordBot.GetClient().GetGuildApplicationCommandsAsync(_SagarismClient.SagarConfig.DiscordData.TestServer.ServerID);
            await _DiscordBot.GetClient().BulkOverwriteGuildApplicationCommandsAsync(_SagarismClient.SagarConfig.DiscordData.TestServer.ServerID, TestGuildCommands);

            await Task.Delay(-1);

        }

        private static bool SessionCreated = false;
        private static Task Client_SessionCreated(DiscordClient sender, DSharpPlus.EventArgs.SessionCreatedEventArgs args)
        {
            if (SessionCreated) { return Task.CompletedTask; ; }
            SessionCreated = true;
            Console.WriteLine($"Bot is Live");
            Console.WriteLine($"Connecting as {_DiscordBot.GetClient().CurrentUser.Username}({_DiscordBot.GetClient().CurrentUser.Id})");
            _DiscordBot.BotIsLive = true;
            _SagarismClient.Initialize();
            return Task.CompletedTask;
        }

        static void VerifyDataFiles()
        {
            if (!Directory.Exists(DataStructure.StaticBotPaths.AppDataFolder))
            {
                Directory.CreateDirectory(DataStructure.StaticBotPaths.AppDataFolder);
            }
        }
    }
}