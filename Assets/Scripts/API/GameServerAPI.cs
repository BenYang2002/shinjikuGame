using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

namespace API{
    public class GameServerAPI
    {
        private const int timeout = 500;
        static int lrPortNum = 1059; // port num for listenRegister
        static int otPortNum = 1060; // port num for establish TCP
        static int chatTCPPort = 1061;
        static string prefix = "ZombieGameShinjuku";
        static List<UserInfo> users = new List<UserInfo>();
        static List<string> userChatTCPList = new List<string>();
        private Socket chatServerSocket;
        private int maxPlayer = 0;
        volatile private Queue<string> messageQueue = new Queue<string>();
        private Dictionary<string, Socket> clientSockets = new Dictionary<string, Socket>();
        private System.Object lockerMsg = new System.Object();
        private System.Object lockerDictionary = new System.Object();
        private System.Object lockuserChatTCPList = new System.Object();
        public GameServerAPI(int maxPlayerNum){
            maxPlayer = maxPlayerNum;
        }

        public System.Object LockuserChatTCPList
        {
            get => lockuserChatTCPList;
        }

        public List<string> UserChatTCPList
        {
            get => userChatTCPList;
        }

        public void listenRegister(){
            Thread thread = new Thread(listenRegisterHelper);
            thread.IsBackground = true;
            thread.Start();
        }
       public void listenRegisterHelper()
       {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, lrPortNum);
            udpSocket.Bind(localEndPoint);
            Debug.Log("Server is listening on port " + lrPortNum);
            byte[] buffer = new byte[1024]; 
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0); 
            
            try
            {
                while (true)
                {   
                    try
                    {
                        int receivedBytes = udpSocket.ReceiveFrom(buffer, ref clientEndPoint);
                        string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        string[] msgs = message.Split(' ');
                        if (msgs[0] != prefix)
                        {
                            continue;
                        }
                        string deviceName = msgs[1].ToString();
                        int tcpPort = int.Parse(msgs[2].ToString());
                        int udpPort = int.Parse(msgs[3].ToString());
                        UserInfo newUser = new UserInfo(deviceName,tcpPort,udpPort);
                        users.Add(newUser);
                        lock(lockuserChatTCPList){
                            userChatTCPList.Add(deviceName);
                        }
                        Debug.Log("Received from ： " + clientEndPoint.ToString());
                        string hostName = Dns.GetHostName();
                        string response = hostName + " " + chatTCPPort.ToString();
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        udpSocket.SendTo(responseData, clientEndPoint);
                        Debug.Log($"Sent response to {clientEndPoint}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error1: {ex.Message}");
                    }
                } // Closing brace for the while loop
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error2: {ex.Message}");
            }
            finally
            {
                udpSocket.Close();
            }
        }
        public void openTcpServer(){
            chatServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any,chatTCPPort);
            try{
                chatServerSocket.Bind(serverEndpoint);
                chatServerSocket.Listen(maxPlayer);
            } catch (Exception ex){
                Debug.LogError(ex.Message);
            }
        }

        public void handleClient(System.Object param){ // will expect the username as the first packet being sent
            Socket clientSocket = (Socket) param;
            string userName = null;
            try{
                byte[] buffer = new byte[1024];
                int byteRead = 0;
                byteRead = clientSocket.Receive(buffer);
                userName = Encoding.UTF8.GetString(buffer,0,byteRead);
                lock(lockerDictionary){
                    clientSockets.Add(userName,clientSocket);
                }
                while(true){
                    byteRead = clientSocket.Receive(buffer);
                    string message = userName + " : " + Encoding.UTF8.GetString(buffer,0,byteRead);
                    Debug.Log(message);
                    lock(lockerMsg){ // lock to synchronize
                        messageQueue.Enqueue(message);
                        Monitor.Pulse(lockerMsg);
                        Debug.Log("new message added " + messageQueue.Count);
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"SocketException: {ex.Message} (ErrorCode: {ex.ErrorCode})");
                clientSocket.Close(); // Clean up the socket
                if(userName != null && clientSockets.ContainsKey(userName)){
                    clientSockets.Remove(userName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected exception: {ex.Message}");
                clientSocket.Close(); // Ensure the socket is cleaned up
                if(userName != null && clientSockets.ContainsKey(userName)){
                    clientSockets.Remove(userName);
                }
            }
        }

        public void distributeMessage(){
            Thread thread = new Thread(distributeMessageHelper);
            thread.IsBackground = true;
            thread.Start();
        }

        public void distributeMessageHelper(){
            while(true){
                string message = null;
                lock(lockerMsg){
                    if(messageQueue.Count == 0){
                        Monitor.Wait(lockerMsg);
                        message = messageQueue.Dequeue();
                    }
                }
                if(message != null){
                    string[] pars = message.Split(' ');
                    string userName = pars[0];
                    byte[] msg = Encoding.UTF8.GetBytes(message);
                    lock(lockerDictionary){
                        var keys = clientSockets.Keys.ToList();

                        for (int i = 0; i < keys.Count; i++)
                        {
                            var recv = keys[i];
                            if (recv == userName)
                            {
                                continue;
                            }
                            Socket sock = clientSockets[recv];
                            sock.Send(msg);
                        }
                    }
                }
            }
        }

        public void acceptChatConnect(){
            Thread thread = new Thread(acceptChatConnectHelper);
            thread.IsBackground = true;
            thread.Start();
        }

        public void acceptChatConnectHelper(){
            try{
                while(true){
                    Socket clientSocket = chatServerSocket.Accept();
                    Debug.Log("Client connected: " + clientSocket.RemoteEndPoint.ToString());
                    Thread newThread = new Thread(new ParameterizedThreadStart(handleClient));
                    newThread.IsBackground = true;
                    newThread.Start((System.Object)clientSocket);
                }
            } catch (Exception ex){
                Debug.LogError("acceptChatConnectHelper Error: " + ex.Message);
            }
        }

        public void openNewTCPConnection(string deviceName){
            int index = -1;
            for(int i = 0; i < users.Count; i++){
                if(users[i].DeviceName == deviceName){
                    index = i;
                }
            }
            if(index == -1){
                Debug.Log("device: " + deviceName + " is not in the list");
                return;
            }
            int udpPort = users[index].UdpPort;
            if(users[index].Connected){
                Debug.Log(users[index].TcpPort + " client already connected");
                if(userChatTCPList.Contains(users[index].DeviceName)){
                    userChatTCPList.Remove(users[index].DeviceName);
                }
                return;
            }
            string clientName = users[index].DeviceName;
            string message = "establishTCP";
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, otPortNum);
            udpSocket.Bind(localEndPoint);
            byte[] buffer = new byte[1024]; 
            IPAddress[] cAddr = Dns.GetHostAddresses(clientName);
            IPAddress[] ipv4Addr = Array.FindAll(cAddr, addr => addr.AddressFamily == AddressFamily.InterNetwork);
            EndPoint clientEndPoint = new IPEndPoint(ipv4Addr[0], udpPort);
            byte[] msg = Encoding.UTF8.GetBytes(message);
            // trying to establish tcp connection
            int count = 0;
            int trialLimit  = 3;
            bool ack = false;
            udpSocket.ReceiveTimeout = timeout;
            try{
                while(ack == false && count < trialLimit){
                    udpSocket.SendTo(msg,clientEndPoint);
                    try{
                        int receivedBytes = udpSocket.ReceiveFrom(buffer,ref clientEndPoint);
                        string acknowledge = Encoding.UTF8.GetString(buffer,0,receivedBytes);
                        if(acknowledge == "ack"){
                            lock(lockuserChatTCPList){
                                userChatTCPList.Remove(deviceName);
                            }
                            users[index].Connected = true;
                            ack = true;
                        }
                        else{
                            Debug.Log("unrecognized operation ");
                            count++;
                        }
                    } catch (SocketException ex){
                        if(ex.SocketErrorCode == SocketError.TimedOut){
                            count++;
                        }
                        else{
                            Debug.LogError($"Error3: {ex.Message}");
                        }
                    }
                }

            } catch (Exception ex){
                Debug.LogError($"Error4: {ex.Message}");
            }
            finally{
                udpSocket.Close();
            }
        }
    }
}