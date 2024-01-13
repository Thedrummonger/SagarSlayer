using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SagarSlayer.DataStructure;
using DrathBot.DataStructure;
using DSharpPlus;

namespace DrathBot.MessageHandeling
{
    internal class ParseMessage
    {
        public static Task _Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args)
        {
            if (args.Author.IsCurrent) { return Task.CompletedTask; }

            Console.WriteLine($"Message Recieved\n{args.Author}\n{args.Guild}\n{args.Channel}\n{args.Message}");

            if (args.Author.IsBot) { return Task.CompletedTask; }

            MessageHandeling.ParseMessage.Parse(new ExtendedDiscordObjects.RecievedMessage(args));

            return Task.CompletedTask;
        }
        public static void Parse(ExtendedDiscordObjects.RecievedMessage Message)
        {
            var SagarData = Program._SagarismClient.SagarConfig.DiscordData;
            if (Message.Server is null)
            {

            }
            else
            {
                if (Message.Server.Id.In(SagarData.ProdServer.ServerID, SagarData.TestServer.ServerID)) { Program._SagarismClient.ReplyToSagar(Message); }
                if (Message.Channel.Id.In(SagarData.GetQuotesChannel())) { Program._SagarismClient.AddSagarQuote(Message); }
            }
        }
    }
}
