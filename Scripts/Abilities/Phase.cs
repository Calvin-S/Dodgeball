using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Phase : Abilities
{
    [SerializeField] private bool isPhasing = false;
    public float phaseTime;
    private float beginPhase;
    private Collider2D c;

    void Start() {
        base.setKey();
        c = GetComponent<Collider2D>();
        cooldown += phaseTime;
    }
    void Update()
    {
        onCooldown = Time.time - beginPhase < cooldown;
        if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer || (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)) {
            if (abilityKey == KeyCode.None)
                return;
            else
                phase();
        }
    }

    public void phase() {
        
        if (Input.GetKey(abilityKey) && Time.time - beginPhase >= cooldown && !isPhasing) {
            Color c = gameObject.GetComponent<SpriteRenderer>().color;
            c.a = 0.5f;
            c.g = 0;
            gameObject.GetComponent<SpriteRenderer>().color = c;
            isPhasing = true;
            if (animBool != "")
                gameObject.GetComponent<Animator>().SetBool(animBool, isPhasing);
            beginPhase = Time.time;
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer) 
                CmdBeginPhase(gameObject.GetComponent<NetworkIdentity>().netId);
        }
        else if (Time.time - beginPhase >= phaseTime && isPhasing) {
            Color c = gameObject.GetComponent<SpriteRenderer>().color;
            if (c.g == 0 && c.a == 0.5f)
                gameObject.GetComponent<SpriteRenderer>().color = new Color(c.r,1f,c.b,1f); 
            isPhasing = false;
            if (animBool != "")
                gameObject.GetComponent<Animator>().SetBool(animBool, isPhasing);
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer) 
                CmdEndPhase(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    public bool getPhase() {
        return isPhasing;
    }

    [Command]
    void CmdBeginPhase(uint id) {
        RpcBeginPhase(id);
    }

    [ClientRpc] 
    void RpcBeginPhase(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            isPhasing = true;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,0f,1f,0.5f);
        }
    }

    [Command]
    void CmdEndPhase(uint id) {
        RpcEndPhase(id);
    }

    [ClientRpc] 
    void RpcEndPhase(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            isPhasing = false;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
        }
    }

}
