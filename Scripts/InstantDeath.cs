using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InstantDeath : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag.Contains("Player") && other.gameObject.GetComponent<PlayerMovement>().isLocalPlayer) {
            if (other.gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
                Destroy(other);
            else {
                other.gameObject.GetComponentInChildren<PlayerHealth>().death();
                other.gameObject.GetComponent<PlayerHealth>().CmdDeath();
            }
        }
    }

}
