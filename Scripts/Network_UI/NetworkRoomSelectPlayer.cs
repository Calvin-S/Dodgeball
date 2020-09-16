using Mirror;
using TMPro;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using System.Linq;

public class NetworkRoomSelectPlayer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private string[] playerTitans = new string[4];
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_Text ipDisplay;
    public TMP_Text rematchText;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar(hook = nameof(ReadyUp))]
    public string character = "";

    private bool isLeader;
    public GameObject SelectUI;
    public NetworkConnection playerConnection = null;

    private int lastRoomCount = 0;

    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
    }

    private NetworkManage room;
    private NetworkManage Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManage;
        }
    }
    [SyncVar]
    private string newName = "";
    public override void OnStartAuthority()
    {

    }

    public string changeName (string newName) {
        if (alreadyUsed(newName)) {
            newName += Room.RoomPlayers.Count.ToString();
            return changeName (newName);
        }
        return newName;
    }

    private bool alreadyUsed(string name) {
        int count = 0;
        foreach (NetworkRoomSelectPlayer n in Room.RoomPlayers) {
            if (n.DisplayName == name) {
                count++;
            }
        }
        return Room.RoomPlayers.Count == lastRoomCount? count > 1 : count > 0;
    }

    public override void OnStartClient() {
        if (hasAuthority) {
            Debug.Log(newName + " | "  + PlayerName.playerName + " | " + gameObject.name);
            if (newName == "")
                newName = PlayerName.playerName;
            UpdateDisplay();
            
            if (PlayerName.playerName == "player")
                newName += Room.RoomPlayers.Count.ToString();
            newName = changeName(newName);
            CmdSetDisplayName(newName);
            Debug.Log(newName);
            PlayerName.playerName = newName;
        }
        
        gameObject.name = gameObject.name + gameObject.GetInstanceID().ToString();
        if (!hasAuthority)
            gameObject.GetComponentInChildren<Canvas>().enabled = false;
        else
            SelectUI = gameObject;
        DontDestroyOnLoad(this);
        Room.RoomPlayers.Add(this); 
        HandleReadyToStart(false);
        UpdateDisplay();
        ipDisplay.text = "Host IP: " + Room.ipAddress;
        Debug.Log("adding new client");
        lastRoomCount = Room.RoomPlayers.Count;
    }

    public override void OnStopClient()
    {
        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) {
        UpdateDisplay();
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue) {
        UpdateDisplay();
    }

    public void ReadyUp (string oldValue, string newValue) {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (!hasAuthority) {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }
        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerTitans[i] = Room.RoomPlayers[i].character;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) 
            return;
        if (readyToStart) {
            startGameButton.gameObject.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>().faceColor = new Color32 (0, 255, 0, 255);
        }
        else {
            startGameButton.gameObject.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>().faceColor = new Color32 (255, 20, 0, 255);
        }
        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    private void CmdSetCharacterName(string name) {
        character = name;
        Debug.Log(character);
    }

    [Command]
    public void CmdReadyUp()
    {   
        Debug.Log(character);
        if (character != "")
            IsReady = !IsReady;
        else 
            Debug.Log("choose a character");
        Room.NotifyReady();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }
        if (Room.ReadyToStart()) {
            foreach (var player in Room.RoomPlayers)
                {
                    if (player.hasAuthority)
                    {
                        player.SelectUI.GetComponentInChildren<Canvas>().enabled = false;
                        break;
                    }
                }
            Room.StartGame();
        }
    }

    public void hideUI() {
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
    }

    public void showUI() {
        gameObject.GetComponentInChildren<Canvas>().enabled = true;
    }

    public void setCharacter(string characterName) {
        character = characterName;
        CmdSetCharacterName(character);
        UpdateDisplay();
        if (character == "Empty")
            Debug.LogError("Character can't be found??");
    }

    public void showRematchText() {
        if (SelectUI == null) return;
        SelectUI.GetComponentInChildren<Canvas>().enabled = true;
        int canvasChildren = SelectUI.GetComponentInChildren<Canvas>().transform.childCount;
        for (int i = 0; i < canvasChildren; i++) 
            SelectUI.GetComponentInChildren<Canvas>().transform.GetChild(i).gameObject.SetActive(false);
        rematchText.gameObject.SetActive(true);
    }

    public void hideRematchText() {
        rematchText.gameObject.SetActive(false);
        if (SelectUI == null) return;
        int canvasChildren = SelectUI.GetComponentInChildren<Canvas>().transform.childCount;
        for (int i = 0; i < canvasChildren; i++) 
            SelectUI.GetComponentInChildren<Canvas>().transform.GetChild(i).gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(isLeader);
        rematchText.gameObject.SetActive(false);
        Debug.Log(rematchText.gameObject);
    }

    public void setClientRematchText() {
        rematchText.text = "The host can restart the match now.";
    }

    [Command]
    public void CmdChangeWeather(string weather)
    {
        WeatherManagement.wm.weather = weather; 
    }
}