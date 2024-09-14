using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using TDMUtils;

namespace DrathBot.DataStructure
{
    public class ExtendedDiscordObjects
    {
        public class RecievedMessage
        {
            public RecievedMessage(MessageCreateEventArgs Args)
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
                Smessage.AuthorID = message.Author.Id;
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
        public class DiscordBot
        {
            public DiscordBot(string APIKEY)
            {
                APIKey = APIKEY;
                Configuration.Token = APIKey;
                Client = new DiscordClient(Configuration);
            }
            public bool BotIsLive = false;
            public string APIKey;
            public DiscordClient Client;
            public DiscordConfiguration Configuration = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };
        }
    }
}
