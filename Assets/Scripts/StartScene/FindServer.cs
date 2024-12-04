using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPBroadCast
{static int portNum = 1059;
    static void Main(string[] args)
    {
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.EnableBroadcast = true;
        IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, portNum);
        string prefix = "ZombieGameShinjuku";
        string hostName = Dns.GetHostName();
        string message = prefix;
        byte[] data = Encoding.UTF8.GetBytes(message);
        byte[] buffer = new byte[1024]; 
        EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0); 
        try
        {
            udpSocket.SendTo(data, broadcastEndPoint);
            Console.WriteLine("Broadcast message sent.");
            int receivedBytes = udpSocket.ReceiveFrom(buffer,ref serverEndPoint);
            string recv = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Console.WriteLine(recv);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            udpSocket.Close();
        }
    }
}
