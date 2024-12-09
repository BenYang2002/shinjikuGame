using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace API
{
    public class LobbyInfo
    {
        private string lobbyName;
        private int playerCount;
        private int readyPlayerCount;
        private string privacyStatus;
        private string gameStatus;
        public LobbyInfo(string lobbyName, string privateStatus)
        {
            this.lobbyName = lobbyName;
            this.privacyStatus = privateStatus;
            playerCount = 0;
            readyPlayerCount = 0;
            gameStatus = "waiting";
        }
        public string LobbyName 
        {
            get => lobbyName;
            set => lobbyName = value;
        }
        public int PlayerCount
        {
            get => playerCount;
            set => playerCount = value;
        }

        public int ReadyPlayerCount
        {
            get => readyPlayerCount;
            set => readyPlayerCount = value;
        }
        public string PrivacyStatus
        {
            get => privacyStatus;
            set => privacyStatus = value;
        }
        public string GameStatus
        {
            get => gameStatus;
            set => gameStatus = value;
        }

        // Convert object to JSON string
        public string ToJson()
        {
            string text = "lobbyname:" + lobbyName + ";"
                        + "playercount:" + playerCount + ";"
                        + "readyPlayerCount:" + readyPlayerCount + ";"
                        + "privacyStatus:" + privacyStatus + ";"
                        + "gameStatus:" + gameStatus + " ";
            return text;
        }

        // Reconstruct object from JSON string
        static public LobbyInfo FromJson(string json)
        {
            string[] fields = json.Split(';');
            string lobby = fields[0].Split(':')[1];
            string playercount = fields[1].Split(':')[1];
            string readyplayercount = fields[2].Split(':')[1];
            string privacystatus = fields[3].Split(':')[1];
            string gameStatus = fields[4].Split(':')[1].Replace(" ", "");
            LobbyInfo newLobby = new LobbyInfo(lobby, privacystatus);
            newLobby.PlayerCount = int.Parse(playercount);
            newLobby.ReadyPlayerCount = int.Parse(readyplayercount);
            newLobby.GameStatus = gameStatus;
            return newLobby;
        }
    }
}