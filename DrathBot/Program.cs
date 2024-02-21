﻿using DrathBot.Commands;
using DrathBot.DataStructure;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using SagarSlayer.AI;
using SagarSlayer.Commands;
using SagarSlayer.DataStructure;
using System.Diagnostics;

namespace DrathBot
{
    internal class Program
    {
        public static bool IsDebug { get { return Debugger.IsAttached && !RunAsLive; } }

        public static bool RunAsLive = false;

        public static readonly Version Version = new Version(1,5,3);

        public static ExtendedDiscordObjects.DiscordBot _DiscordBot { get; private set; }
        public static ChatGPTClient _ChatGPTClient { get; private set; }
        public static MessageHandeling.Sagarism _SagarismClient { get; private set; }
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Version: {Version}");
            Console.WriteLine($"Debug Mode: {IsDebug}");
            //Validate Data Files
            Console.WriteLine($"Validating AppData Directory");
            VerifyDataFiles();

            //Register Clients
            var OPenAIData = Utility.GetfromFile(StaticBotPaths.Sagarism.Files.AIData, new AI.ChatGPTData(), false);
            _ChatGPTClient = new ChatGPTClient(OPenAIData.APIKey);
            _SagarismClient = new MessageHandeling.Sagarism();
            _DiscordBot = new ExtendedDiscordObjects.DiscordBot(_SagarismClient.SagarConfig.DiscordData.GetBotKey());

            _DiscordBot.Client.UseVoiceNext();

            //Enable Commands
            var slash = _DiscordBot.Client.UseSlashCommands();
            slash.RegisterCommands<SagarConfigSlashCommands>(_SagarismClient.SagarConfig.DiscordData.TestServer.ServerID);
            slash.RegisterCommands<SagarismSlashCommands>();
            slash.RegisterCommands<SagarTestCommands>(_SagarismClient.SagarConfig.DiscordData.TestServer.ServerID);

            //Create Listeners
            _DiscordBot.Client.MessageCreated += MessageHandeling.ParseMessage._Client_MessageCreated;

            //Connect Discord Client
            Console.WriteLine($"Initializing Discord Client");
            await _DiscordBot.Client.ConnectAsync();

            _DiscordBot.Client.SessionCreated += Client_SessionCreated;

            await Task.Delay(-1);

        }

        private static Task Client_SessionCreated(DiscordClient sender, DSharpPlus.EventArgs.SessionReadyEventArgs args)
        {
            Console.WriteLine($"Bot is Live");
            Console.WriteLine($"Connecting as {_DiscordBot.Client.CurrentUser.Username}({_DiscordBot.Client.CurrentUser.Id})");
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