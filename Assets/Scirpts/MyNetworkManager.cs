using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public string playerName = "test";
    public GameObject localPlayer;
    public Player player;

    public override void OnStartServer()
    {
        Debug.Log("Server Started");
    }

    public override void OnStopServer()
    {
        Debug.Log("Server Stopped");
    }

    public virtual void OnClientConnect(NetworkConnection conn)
    {
        Player localPlayer = conn.identity.GetComponent<Player>();
        localPlayer.SetName(playerName);
        player = localPlayer.GetComponent<Player>();
        Debug.Log("Connected to Server");
    }

    public virtual void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("Disconnected from Server");
    }

}
