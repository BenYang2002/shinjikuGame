using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;
namespace API{
    public class GameClientAPI
    {

        private const string prefix = "ZombieGameShinjuku";
        private string hostName;
        private int serverUDPPort = 1059;
        private const int timeout = 1000;
        private int clientTCPPort = 2059;
        private int clientUDPPort = 2060;
        private const int MAXRETRY = 3;
        private int serverTCPPort;
        private UserInfo thisUser = null;
        private static GameClientAPI instance = null;
        private ConcurrentQueue<string> messageQ = new ConcurrentQueue<string>();
        private List<LobbyInfo> lobbyList = new List<LobbyInfo>();
        private System.Object lockLobbyList = new System.Object();
        private bool updateLobbyList = false;
        private System.Object lockUpdateLobbyList = new System.Object();
        public bool UpdateLobbyList
        {
            get
            {
                lock (lockUpdateLobbyList)
                {
                    return updateLobbyList;
                }
            }
            set
            {
                lock (lockLobbyList)
                {
                    updateLobbyList = value;
                }
            }
        }

        public List<LobbyInfo> LobbyList
        {
            get
            {
                lock (lockLobbyList) 
                {
                    return lobbyList;
                }
            }
        }
        public int ClientTCPPort
        {
            get => clientTCPPort;
        }
        public int ClientUDPPort
        {
            get => clientUDPPort;
        }

        public static GameClientAPI GetInstance(int tcp = 2059, int udp = 2060)
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

        Socket clientTCPSocket; 

        private GameClientAPI(int tcp,int udp){
            thisUser = new UserInfo(Dns.GetHostName(), tcp, udp);
        }

        public UserInfo ThisUser{
            get => thisUser;
        }
        public bool register()
        {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.EnableBroadcast = true;
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, serverUDPPort);
            string message = prefix;
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
                    string recv = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    string[] info = recv.Split(' ');
                    hostName = info[0];
                    serverTCPPort = int.Parse(info[1]);
                    UDPListen();
                    TCPSocketSetup();
                    connected = ConnectRemoteTCPServer();
                    thisUser.Connected = connected;
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

        public void TCPSocketSetup(){
            clientTCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, clientTCPPort);
            try{
                clientTCPSocket.Bind(localEndPoint);
            } catch (Exception ex){
                Debug.LogError(ex.Message);
            }
        }

        public bool ConnectRemoteTCPServer(){
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                IPAddress[] ipv4Addr = Array.FindAll(addresses, addr => addr.AddressFamily == AddressFamily.InterNetwork);
                if(ipv4Addr.Length == 0)
                {
                    return false;
                }
                clientTCPSocket.Connect(ipv4Addr[0], serverTCPPort);
                thisUser.ClientSocket = clientTCPSocket;
                return true;
            }catch(Exception ex){
                Debug.LogError(ex.Message );
                return false;
            }
        }

        public void sendTCPMessage2Server(string message){
            try
            {
                if (message != null)
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    clientTCPSocket.Send(messageBytes);
                }
            }catch(Exception ex)
            {
                Debug.LogError("sendTCPMessage2Server: " + ex.Message);
            }
            
        }

        /*public void storeDisplayMsg(){
            Thread thread = new Thread(storeDisplayMsgHelper);
            thread.IsBackground = true;
            thread.Start();
        }

        public void storeDisplayMsgHelper(){
            while(true){
                byte[] buffer = new byte[1024];
                int receivedBytes = clientTCPSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                string header = response.Split(' ')[0];
                int indexOf = response.IndexOf(' ');
                response = response.Substring(indexOf+1);
                if(header == "chat"){
                    messageQ.Enqueue(response);
                }
                else if(header == "lobbyCreation"){
                    lobbyQ.Enqueue(response);
                }
                else if(header == "one"){
                    pressingQ.Enqueue(response);
                }
                Debug.Log("message: " + response);
            }
        }*/
        public void TCPListen()
        {
            Debug.Log("starting to listen tcp incoming");
            Thread thread = new Thread(TCPListenHelper);
            thread.IsBackground = true;
            thread.Start();
        }

        public void TCPListenHelper()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int receivedBytes = clientTCPSocket.Receive(buffer);
                string request = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                Debug.Log("received message" + request);
                string header = request.Split(' ')[0];
                request = RemoveHeader(request);//remove the header
                if (header == "chat")
                {
                    Debug.Log("received message" + request);
                    messageQ.Enqueue(request);
                }
                else if (header == "lobbyInfo")
                {

                    string[] lobbyInfos = request.Split(' '); 
                    lock (lockLobbyList)
                    {
                        lobbyList.Clear();
                        for (int i = 0; i < lobbyInfos.Length - 1; i++)
                        {
                            lobbyList.Add(LobbyInfo.FromJson(lobbyInfos[i]));
                        }
                    }
                    lock (lockUpdateLobbyList)
                    {
                        updateLobbyList = true;
                    }
                }
                else if(header == "hitting")
                {
                    
                }
            }
        }

        public string RemoveHeader(string msg)
        {
            int spaceIndex = msg.IndexOf(' ');
            if (spaceIndex == -1)
            {
                Console.WriteLine("removeHeader: no header detected");
            }
            else
            {
                msg = msg.Substring(spaceIndex + 1);
            }
            return msg;
        }
        public void UDPListen()
        {
            Thread thread = new Thread(UDPListenHelper);
            thread.IsBackground = true;
            thread.Start();
        }
        public void UDPListenHelper(){ 
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, clientUDPPort);
            udpSocket.Bind(localEndPoint);
            Debug.Log($"UDP Listener started on port {clientUDPPort}...");
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    if (udpSocket.Available > 0)
                    {
                        int receivedBytes = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                        string msg = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        string[] msgArr = msg.Split(' ');
                        string serverHeader = msgArr[0];
                        msg = RemoveHeader(msg); // removed the header
                        
                    }
                    Thread.Sleep(100);
                }
            } catch(Exception ex)
            {
                Debug.LogError("UDPListen: " + ex.Message);
            }
        }

        
    }
}