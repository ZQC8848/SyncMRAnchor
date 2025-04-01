using Mirror;
using Mirror.Discovery;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    public NetworkDiscovery networkDiscovery;
    float RefreshRoomTime;//房间信息刷新
    public bool isHostVR;// 软件分为host端和clinet端
#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif
    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
        Connect(info);
    }
    public void FindServer() // 搜索房间
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }
    public void StartHost() // 创建房间
    {
        discoveredServers.Clear();
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
        Debug.Log("Server started");
    }
    public void ExitHost() //退出房间
    {
        if (NetworkServer.active && NetworkClient.isConnected) //房主关闭房间
        {
            NetworkManager.singleton.StopHost();
            networkDiscovery.StopDiscovery();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
            networkDiscovery.StopDiscovery();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if(isHostVR)
            StartHost();
    }

    // Update is called once per frame
    void Update()
    {
        if(isHostVR)
            return;
        
        RefreshRoomTime += Time.deltaTime;
        if(RefreshRoomTime>1)//一个简单的计数器 每1秒执行一次
        {
            RefreshRoomTime = 0;
            FindServer();
        }
    }
    public void Connect(ServerResponse info)
    {
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.uri);
        Debug.Log("Join in Server:"+info.serverId);
    }
}
