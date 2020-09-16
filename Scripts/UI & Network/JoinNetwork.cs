using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class JoinNetwork : MonoBehaviour
{
     [SerializeField] private NetworkManage networkManager = null;

    [Header("UI")]
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        NetworkManage.OnClientConnected += HandleClientConnected;
        NetworkManage.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManage.OnClientConnected -= HandleClientConnected;
        NetworkManage.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;   
        if (ipAddress.Equals(""))
            ipAddress = "localhost";
        if (networkManager == null)
            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManage>();
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }

    public void HostButton() {
        if (networkManager == null)
            GameObject.Find("NetworkManager").GetComponent<NetworkManage>().StartHost();
        networkManager.StartHost();
    }
}

