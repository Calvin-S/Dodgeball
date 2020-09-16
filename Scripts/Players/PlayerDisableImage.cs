using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisableImage : MonoBehaviour
{
    public PlayerMovement playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        if (playerMovement != null && !playerMovement.isLocalPlayer)
            gameObject.SetActive(false);
    }
}
