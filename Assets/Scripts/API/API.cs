using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace API {
    class GameAPI : MonoBehaviour
    {
        private const string prefix = "ZombieGameShinjuku";
        private int serverPort = 1059;
        private int clientTCPPort = 1059;
        private int clientUDPPort = 1060;
        private const int timeout = 1000;
        private const int MAXRETRY = 3;
        public bool register()
        {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.EnableBroadcast = true;
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, serverPort);
            string message = prefix + " " + Dns.GetHostName() + " " + clientTCPPort.ToString() + " " + clientUDPPort.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] buffer = new byte[1024]; 
            EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0); 
            bool connected = false;
            int count = 0;
            udpSocket.ReceiveTimeout = timeout;
            while(count < MAXRETRY && !connected){
                try
                {
                    udpSocket.SendTo(data, broadcastEndPoint);
                    Debug.Log("Broadcast message sent.");
                    int receivedBytes = udpSocket.ReceiveFrom(buffer,ref serverEndPoint);
                    connected = true;
                    string recv = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Debug.Log(recv);
                    string[] info = recv.Split(' ');
                    serverPort = int.Parse(info[1]);
                    Debug.Log(serverPort.ToString());
                }
                catch (SocketException ex)
                {
                    if(ex.SocketErrorCode == SocketError.TimedOut){
                        count++;
                        if(count < MAXRETRY){
                            Debug.Log("Timeout " + count + "th retrying to connect");
                        }
                        else{
                            Debug.Log("Cannot find a server");
                        }
                    }
                    else{
                        Debug.Log($"Error: {ex.Message}");
                    }
                }
            }
            udpSocket.Close();
            return connected;
        }

        public void UDPListen(){ 
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, clientUDPPort);
            udpSocket.Bind(localEndPoint);
            Debug.Log($"UDP Listener started on port {clientUDPPort}...");
            byte[] buffer = new byte[1024];
            while (true)
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                Debug.Log($"Received from {remoteEndPoint}");
            }
        }
    }
}