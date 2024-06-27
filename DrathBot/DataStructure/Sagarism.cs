using DSharpPlus.Entities;

namespace DrathBot.DataStructure
{
    public class Sagarism
    {
        public class SagarResponse(string _Message, int _Weight = 100)
        {
            public Guid ID = Guid.NewGuid();
            public int Weight = _Weight;
            public string Message = _Message;
        }

        public class SagarConfig
        {
            public List<ulong> ReplyTargets = [];
            public double ReplyChance = 0.90;
            public bool ReduceDuplicateResponses = true;
            public bool ReduceDuplicateQuotes = true;
            public DiscordActivity? UserStatus;
            public SagarBotData DiscordData;
        }

        public class SagarBotData
        {

            public SagarismServer ProdServer = new();
            public SagarismServer TestServer = new();
            public SagarismUsers Users = new();
            public SagarismBotKeys BotKeys = new();
            public string GetBotKey() { return Program.IsDebug ? BotKeys.Testing : BotKeys.Production; }
            public ulong GetServerID() { return Program.IsDebug ? TestServer.ServerID : ProdServer.ServerID; }
            public ulong GetSagarQuotesChannel() { return Program.IsDebug ? TestServer.Channels.SQuotes : ProdServer.Channels.SQuotes; }
            public ulong GetMiscQuotesChannel() { return Program.IsDebug ? TestServer.Channels.RQuotes : ProdServer.Channels.RQuotes; }
            public ulong GetGeneralChannel() { return Program.IsDebug ? TestServer.Channels.General : ProdServer.Channels.General; }
            public ulong GetSagarUser() { return Program.IsDebug ? Users.TestUser : Users.ProductionUser; }
        }
        public class SagarismUsers
        {
            public ulong Developer;
            public ulong ProductionUser;
            public ulong TestUser;
        }

        public class SagarismServer
        {
            public ulong ServerID;
            public SagarismChannels Channels = new();
        }

        public class SagarismChannels
        {
            public ulong General;
            public ulong SQuotes;
            public ulong RQuotes;
            public ulong Config;
        }

        public class SagarismBotKeys
        {
            public string Production;
            public string Testing;
        }

        public class SagarOptionEditStatus
        {
            public bool WasError = false;
            public string Status = "OK";
        }
    }
}
