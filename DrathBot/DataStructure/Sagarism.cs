namespace DrathBot.DataStructure
{
    public class Sagarism
    {
        public class SagarResponse
        {
            public SagarResponse(string _Message, int _Weight = 100)
            {
                ID = Guid.NewGuid();
                Message = _Message;
                Weight = _Weight;
            }
            public Guid ID;
            public int Weight = 100;
            public string Message;
        }

        public class SagarConfig
        {
            public List<ulong> ReplyTargets = [];
            public double ReplyChance = 0.90;
            public bool ReduceDuplicateResponses = true;
            public bool ReduceDuplicateQuotes = true;
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
            public ulong GetQuotesChannel() { return Program.IsDebug ? TestServer.Channels.Quotes : ProdServer.Channels.Quotes; }
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
            public ulong Quotes;
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
