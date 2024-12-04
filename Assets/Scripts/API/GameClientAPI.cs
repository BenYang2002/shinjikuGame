using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
namespace API{
    public class GameClientAPI
    {
        private const string prefix = "ZombieGameShinjuku";
        private int serverPort = 1059;
        private int clientTCPPort = 2059;
        private int clientUDPPort = 2060;
        private const int timeout = 1000;
        private const int MAXRETRY = 3;
        private int serverChatPort;
        volatile bool chatConnected = false;

        private string hostName;
        private static GameClientAPI instance;
        private ConcurrentQueue<string> messageQ = new ConcurrentQueue<string>();

        public static GameClientAPI GetInstance(int tcp = 2000, int udp = 3000)
        {
            if (instance == null)
            {
                instance = new GameClientAPI(tcp, udp);
            }
            return instance;
        }

        public ConcurrentQueue<string> MessageQ{
            get => messageQ;
        }

        Socket chatSocket; 

        public GameClientAPI(int tcpPort,int udpPort){
            clientTCPPort = tcpPort;
            clientUDPPort = udpPort;
        }

        public bool ChatConnected{
            get => chatConnected;
            set => chatConnected = value;
        }
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
                    hostName = info[0];
                    serverChatPort = int.Parse(info[1]);
                }
                catch (SocketException ex)
                {
                    if(ex.SocketErrorCode == SocketError.TimedOut){
                        count++;
                        if(count < MAXRETRY){
                            Debug.LogError("Timeout " + count + "th retrying to connect");
                        }
                        else{
                            Debug.LogError("Cannot find a server");
                        }
                    }
                    else{
                        Debug.LogError($"Error: {ex.Message}");
                    }
                }
            }
            udpSocket.Close();
            return connected;
        }

        public void TCPChatSocketSetup(){
            chatSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, clientTCPPort);
            try{
                chatSocket.Bind(localEndPoint);
            } catch (Exception ex){
                Debug.LogError(ex.Message);
            }
        }

        public void connectRemoteChatServer(){
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                IPAddress[] ipv4Addr = Array.FindAll(addresses, addr => addr.AddressFamily == AddressFamily.InterNetwork);
                chatSocket.Connect(ipv4Addr[0], serverChatPort);
        }

        public void sendMessage2Chat(string message){
            if(message != null){
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                chatSocket.Send(messageBytes);
            }
        }

        public void storeDisplayMsg(){
            Thread thread = new Thread(storeDisplayMsgHelper);
            thread.IsBackground = true;
            thread.Start();
        }

        public void storeDisplayMsgHelper(){
            while(true){
                byte[] buffer = new byte[1024];
                int receivedBytes = chatSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                messageQ.Enqueue(response);
            }
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
                if(udpSocket.Available > 0) {
                    int receivedBytes = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    string msg = Encoding.UTF8.GetString(buffer,0,receivedBytes);
                    Debug.Log($"Received from {remoteEndPoint}");
                    Debug.Log(msg);
                    string[] msgArr = msg.Split(' ');
                    string serverHeader = msgArr[0];
                    if(serverHeader == "establishTCP"){
                        if(!chatConnected){
                            TCPChatSocketSetup();
                            connectRemoteChatServer();
                            byte[] message = Encoding.UTF8.GetBytes("ack");
                            udpSocket.SendTo(message,remoteEndPoint);
                            chatConnected = true;
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        
    }
}