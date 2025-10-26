using DrathBot.DataStructure;
using DSharpPlus;
using TDMUtils;

namespace DrathBot.MessageHandling
{
    internal class ParseMessage
    {
        public static Task _Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreatedEventArgs args)
        {
            if (args.Author.IsCurrent) { return Task.CompletedTask; }

            Console.WriteLine($"Message Received\n{args.Author}\n{args.Guild}\n{args.Channel}\n{args.Message}");

            if (args.Author.IsBot) { return Task.CompletedTask; }

            Parse(new ExtendedDiscordObjects.RecievedMessage(args));

            return Task.CompletedTask;
        }
        public static void Parse(ExtendedDiscordObjects.RecievedMessage Message)
        {
            var SagarData = Program._SagarismClient.SagarConfig.DiscordData;

            if (Message.Channel.Id.In(SagarData.GetSagarQuotesChannel()))
                Program._SagarismClient.AddSagarQuote(Message);
            else if (Message.Channel.Id.In(SagarData.GetMiscQuotesChannel()))
                Program._SagarismClient.AddMiscQuote(Message);
            else if (Message.Server is not null && Message.Server.Id.In(SagarData.ProdServer.ServerID, SagarData.TestServer.ServerID)) 
                Program._SagarismClient.ReplyToSagar(Message);
        }
    }
}
