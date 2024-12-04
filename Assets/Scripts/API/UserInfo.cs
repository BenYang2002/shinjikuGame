using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace API{
    public class UserInfo{
        private string userName;
        private string deviceName;
        private int tcpPort;
        private int udpPort;
        bool chatConnected;
        public UserInfo(string name, int tcp, int udp){
            deviceName = name;
            tcpPort = tcp;
            udpPort = udp;
            chatConnected = false;
        }

        public string DeviceName
        {
            get => deviceName; // Getter
            set => deviceName = value; // Setter
        }

        public int TcpPort
        {
            get => tcpPort; // Getter
            set => tcpPort = value; // Setter
        }

        public int UdpPort
        {
            get => udpPort; // Getter
            set => udpPort = value; // Setter
        }

        public bool Connected
        {
            get => chatConnected;
            set => chatConnected = value;
        }
    }
}