using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TDMUtils;
using static DrathBot.DataStructure.Sagarism;

namespace DrathBot.DataStructure
{
    public class ExtendedDiscordObjects
    {
        public class RecievedMessage
        {
            public RecievedMessage(MessageCreatedEventArgs Args)
            {
                Author = Args.Author;
                Server = Args.Guild;
                Channel = Args.Channel;
                Message = Args.Message;
            }

            public DiscordUser Author;
            public DiscordGuild Server;
            public DiscordChannel Channel;
            public DiscordMessage Message;
            public string MessageText { get { return Message.Content; } }
            public char MessagePrefix { get { return Message.Content[0]; } }
        }
        public class SerializeableDiscordMessage
        {
            public Uri Link;
            public ulong MessageID;
            public ulong ChannelId;
            public ulong AuthorID;
            public string Content;
            public HashSet<string> RelevantUsers = [];
            public DateTimeOffset TimeStamp;
            public List<DiscordAttachment> Attachments;

            public static SerializeableDiscordMessage FromDiscordMessage(DiscordMessage message)
            {
                SerializeableDiscordMessage Smessage = new SerializeableDiscordMessage();
                Smessage.MessageID = message.Id;
                Smessage.ChannelId = message.ChannelId;
                Smessage.AuthorID = message.Author?.Id??ulong.MinValue;
                Smessage.Content = message.Content;
                Smessage.Link = message.JumpLink;
                Smessage.TimeStamp = message.Timestamp;
                if (message.Attachments.Count > 0)
                {
                    Smessage.Attachments = [.. message.Attachments];
                }
                if (!message.Content.IsNullOrWhiteSpace())
                {
                    Smessage.RelevantUsers = DiscordUtility.GetQuotedUsersFromQuote(Smessage);
                }
                return Smessage;
            }
        }
        public class DiscordBot(string APIKEY)
        {
            public bool BotIsLive = false;
            public string APIKey = APIKEY;
            private bool ClientBuilt = false;
            private DiscordClient? Client;
            private DiscordClientBuilder Builder = DiscordClientBuilder.CreateDefault(APIKEY, DiscordIntents.All);

            public DiscordClient Build()
            {
                if (ClientBuilt) { throw new Exception($"Client is already built"); }
                Client = Builder.Build();
                ClientBuilt = true;
                return Client;
            }
            public DiscordClientBuilder GetBuilder()
            {
                if (ClientBuilt) { throw new Exception($"Builder is inaccessible as Client is already built"); }
                return Builder;
            }
            public DiscordClient GetClient()
            {
                if (!ClientBuilt) { throw new Exception($"Client is inaccessible as it has not been built"); }
                return Client;
            }
        }
    }
}
