using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    public static string playerName;
    public TMP_InputField inputField;
    // Start is called before the first frame update
    void Start()
    {
        PlayerName.playerName = "player";
        var sEvent1 = new TMP_InputField.SelectionEvent();
        sEvent1.AddListener(changeName);
        inputField.onDeselect = sEvent1;

    }

    public void changeName(string newName) {
        PlayerName.playerName = newName;
    }

}
