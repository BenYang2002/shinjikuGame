using UnityEngine;
using API;
using System.Threading;
public class ButtonHandler : MonoBehaviour
{
    public void OnButtonClick()
    {
        int tcp = 2000;
        int udp = 3000;
        GameClientAPI myapi = new GameClientAPI(tcp,udp);
        bool findServer = myapi.register();
        if(findServer){
            Thread udpListener = new Thread(myapi.UDPListen);
            udpListener.IsBackground = true;
            udpListener.Start();
        }

        else{
            Debug.Log("fail to connect to the chat server");
        }
    }
}
