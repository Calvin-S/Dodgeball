using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Net;

[AddComponentMenu("")]
public class NetworkManage : NetworkManager
{
    [Scene] [SerializeField] private string menuScene;
    [Scene] [SerializeField] private string pinkMap;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomSelectPlayer> RoomPlayers { get; } = new List<NetworkRoomSelectPlayer>();
    public List<NetworkPlayerName> GamePlayers { get; } = new List<NetworkPlayerName>();
    public List<WeatherManagement> WeatherManage {get; } = new List<WeatherManagement>();
    public List<GameObject> Titans = new List<GameObject>();
    public NetworkMapHandler mapHandler;
    public GameObject roomPlayer;
    public string ipAddress = "";
    private int deaths = 0;
    private bool canJoin = true;

    public void addDeath() {
        deaths += 1;
        canEndGame = deaths >= RoomPlayers.Count - 1;
        Debug.Log(deaths + " " + canEndGame);
    }
    public override void Start() {
        networkAddress = GetLocalIPv4();
        Debug.Log(networkAddress);
        base.Start();
    }

    public override void OnStartServer() {
        //spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        ipAddress = GetLocalIPv4();
    }

    public override void OnStartClient() {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public string GetLocalIPv4()
    {
         return Dns.GetHostEntry(Dns.GetHostName())
             .AddressList.First(
                 f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
             .ToString();
    }  

    public override void OnClientConnect(NetworkConnection conn) {
        RoomPlayers.Clear();
        GamePlayers.Clear();
        WeatherManage.Clear();
        base.OnClientConnect(conn);
        NetworkManage nm = NetworkManager.singleton as NetworkManage;
        spawnPrefabs = nm.spawnPrefabs;
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        RoomPlayers.Clear();
        GamePlayers.Clear();
        WeatherManage.Clear();
        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn) {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
        if (!menuScene.Contains(SceneManager.GetActiveScene().name))
        {
            conn.Disconnect();
            return;
        }

    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        if (menuScene.Contains(SceneManager.GetActiveScene().name) && canJoin)
        {
            bool isLeader = RoomPlayers.Count == 0;
            GameObject o = Instantiate(roomPlayer);
            NetworkRoomSelectPlayer roomPlayerInstance = o.GetComponent<NetworkRoomSelectPlayer>();
            WeatherManagement w = o.GetComponent<WeatherManagement>();
            Debug.Log(w);
            roomPlayerInstance.IsLeader = isLeader;
            roomPlayerInstance.hideRematchText();
            NetworkServer.AddPlayerForConnection(conn,w.gameObject);
        }
        canJoin = true;
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if (conn.identity != null) {
            var player = conn.identity.GetComponent<NetworkRoomSelectPlayer>();
            RoomPlayers.Remove(player);
                if (player == null) {
                for (int i = 0; i < RoomPlayers.Count; i++) {
                    NetworkRoomSelectPlayer n = RoomPlayers[i];
                    if (conn.identity.GetComponent<PlayerHealth>() != null && n.DisplayName == conn.identity.GetComponent<PlayerHealth>().displayName) {
                        Debug.Log("removed");
                        Debug.Log(n.DisplayName);
                        RoomPlayers.Remove(n);
                        Debug.Log(RoomPlayers.Count);
                        i--;
                        addDeath();
                        NetworkServer.Destroy(n.gameObject);
                    }
                }
            }
            NotifyReady();
            Debug.Log(conn.identity.gameObject);
            NetworkServer.Destroy(conn.identity.gameObject);
        }
        NotifyReady();
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer() {
        RoomPlayers.Clear();
        GamePlayers.Clear();
        WeatherManage.Clear();
        base.OnStopServer();
    }

    public override void ServerChangeScene(string newSceneName){
        base.ServerChangeScene(newSceneName);
    }
    
    public override void OnServerSceneChanged(string sceneName) {
        if (!sceneName.Contains("M_Choose_Character")) {
            for (int i = 0; i < RoomPlayers.Count; i++)
            {
                var conn = RoomPlayers[i].connectionToClient;
                if (conn == null)
                    conn = RoomPlayers[i].playerConnection;
                GameObject titan = null;
                Debug.Log(RoomPlayers.Count);
                foreach (GameObject c in Titans) {
                    if (c.name == RoomPlayers[i].character)
                        titan = c;
                }
                if (titan == null)
                    Debug.LogError("No titan spawning");
                GameObject gameplayerInstance = Instantiate(titan);

                // Dealing with spawnpoints
                if (i == 0) {
                    gameplayerInstance.transform.position = getRandomSpawnpoint(true);
                }
                else 
                    gameplayerInstance.transform.position = getRandomSpawnpoint(false);

                Debug.Log(conn);
                GameObject temp = conn.identity.gameObject;
                gameplayerInstance.GetComponent<PlayerHealth>().displayName = RoomPlayers[i].DisplayName;
                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
                RoomPlayers[i].playerConnection = conn;
            }
            foreach (NetworkRoomSelectPlayer r in RoomPlayers) {
                r.hideUI();
            }
        }
        else {
            NotifyReady();
        }
    }

    private List<Vector3> spawnLocations;

    Vector3 getRandomSpawnpoint(bool newMap) {
        if (newMap || spawnLocations == null) {
            spawnLocations = new List<Vector3>();
            GameObject locations = GameObject.Find("Spawnpoints");
            if (locations != null)
                foreach (Transform child in locations.transform)
                    spawnLocations.Add(child.position);
            else 
                return Vector3.zero;
        }
        int r = UnityEngine.Random.Range(0,spawnLocations.Count);
        Vector3 position = spawnLocations[r];
        spawnLocations.RemoveAt(r);
        return position;        
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        base.OnClientSceneChanged(conn);
        deaths = 0;
        if (!SceneManager.GetActiveScene().name.Contains("M_Choose_Character")) {
            foreach (NetworkRoomSelectPlayer r in RoomPlayers) {
                r.gameObject.GetComponentInChildren<Canvas>().enabled = false;
            }
        }
        else {
            canEndGame = false;
            foreach (NetworkRoomSelectPlayer r in RoomPlayers) {
                r.hideRematchText();
            }
        }
    }

    public void StartGame() {
        string mapPVP = mapHandler.getRandomPVPMap();
        ServerChangeScene(mapPVP);
    }

    public void NotifyReady() {
        foreach (var player in RoomPlayers)
            {
                player.HandleReadyToStart(ReadyToStart());
            }
    }

    public bool ReadyToStart() {
        if (numPlayers < 1) // should be 2
            return false;
        foreach (var players in RoomPlayers) {
            if (!players.IsReady)
                return false;
        }
        return true;
    }

    private bool canEndGame = false;

    public override void LateUpdate() {
        base.LateUpdate();
        if (!SceneManager.GetActiveScene().name.Contains("M_Choose_Character")) {
            if (canEndGame) {
                for (int i = 0; i < RoomPlayers.Count; i++) {
                    if (RoomPlayers[i].isServer) {
                        RoomPlayers[i].showRematchText();
                    }
                    else {
                        RoomPlayers[i].setClientRematchText();
                        RoomPlayers[i].showRematchText();
                    }
                }
            }
            if (canEndGame && Input.GetKey(KeyCode.R) && ipAddress != "") {
                canEndGame = false;
                deaths = 0;
                for (int i = 0; i < RoomPlayers.Count; i++) {
                    RoomPlayers[i].hideRematchText();
                    var conn = RoomPlayers[i].playerConnection;
                    if (!conn.identity.gameObject.GetComponent<PlayerMovement>().isServer)
                        continue;
                    if (PlayerName.playerName == RoomPlayers[i].DisplayName) {
                        RoomPlayers[i].showUI();
                        //canJoin = false;
                        NetworkServer.ReplacePlayerForConnection(conn, RoomPlayers[i].gameObject);
                        RoomPlayers[i].playerConnection = conn;
                        RoomPlayers[i].CmdChangeWeather("clear");
                        Debug.Log(RoomPlayers[i].connectionToClient);
                        Debug.Log(RoomPlayers[i].playerConnection);
                    }
                    else {
                        NetworkServer.ReplacePlayerForConnection(conn, RoomPlayers[i].gameObject);
                        RoomPlayers[i].playerConnection = conn;
                    }
                }
                NetworkManage nm = NetworkManager.singleton as NetworkManage;
                nm.ServerChangeScene(menuScene);
            }
        }
    }

    private static int uniqId = 0;
    public static int uniq_id () {
        NetworkManage.uniqId += 1;
        return NetworkManage.uniqId;
    }
    
}
