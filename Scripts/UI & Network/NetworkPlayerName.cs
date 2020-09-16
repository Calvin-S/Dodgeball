using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayerName : NetworkBehaviour
{
     [SyncVar]
    public string displayName = "Loading...";

    private NetworkManage room;
    private NetworkManage Room {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManage;
        }
    }

    public override void OnStartClient()
    {
        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }
}
