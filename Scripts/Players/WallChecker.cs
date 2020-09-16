using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallChecker : MonoBehaviour
{
    public bool hitWall = false;
    
    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Ground") {
            hitWall = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Ground") {
            hitWall = false;
        }
    }
}
