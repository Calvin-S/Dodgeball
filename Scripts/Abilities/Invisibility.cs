using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Invisibility : Abilities
{
    [SerializeField] private bool isInvisible = false;

    [SerializeField] private float invisibleDuration = 0; // should be 0 for passive

    // cooldown AFTER invisibility has ended OR passively: how long before player turns invisible by standing still

    // records when ability key pressed to turn invisible OR passively: records when player has last moved
    [SerializeField] private float lastTimeMoving; 
    private Vector3 lastPos;

    void Start() {
        base.setKey();
        lastPos = gameObject.transform.position;
        cooldown += invisibleDuration;
    }

    void Update() {
        onCooldown = Time.time - lastTimeMoving < cooldown;
        if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer || (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)) {
            if (abilityKey == KeyCode.None && gameObject.name.Contains("phasma"))
                InvisiblePassive();
            else 
                Invisible();
            lastPos = transform.position;
        }
    }
    
    public void InvisiblePassive() {
        lastTimeMoving = hasMoved() ? Time.time : lastTimeMoving;
        if (Time.time - lastTimeMoving >= cooldown) {
            isInvisible = true;
            Color c = gameObject.GetComponent<SpriteRenderer>().color;
            c.a = 0.5f;
            gameObject.GetComponent<SpriteRenderer>().color = c;
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)
                CmdStartInvisible(gameObject.GetComponent<NetworkIdentity>().netId);
        }
        else if (isInvisible) {
            Color c = gameObject.GetComponent<SpriteRenderer>().color;
            if (c.a == 0.5f)
                gameObject.GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 1f);
            isInvisible = false;
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)
                CmdEndInvisible(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    bool hasMoved() {
        bool moving =  Mathf.Abs( Vector3.Distance(transform.position, lastPos)) > 0.01;
        bool movementKey = gameObject.GetComponent<PlayerMovement>().movementKeyDown();
        bool abilityKeys = gameObject.GetComponent<PlayerMovement>().abilityKeyDown();
        if (gameObject.GetComponent<PlayerShoot>() != null) 
            abilityKeys = abilityKeys || gameObject.GetComponent<PlayerShoot>().shootKeyDown();
        return moving || movementKey || abilityKeys;
    }

    void Invisible() {
        if (Input.GetKey(abilityKey) && Time.time - lastTimeMoving >= cooldown && !isInvisible) {
            lastTimeMoving = Time.time;
            isInvisible = true;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,0.5f);
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)
                CmdStartInvisible(gameObject.GetComponent<NetworkIdentity>().netId);
        }
        else if (Time.time - lastTimeMoving >= invisibleDuration && isInvisible) {
            if ( gameObject.GetComponent<SpriteRenderer>().color == new Color(1f,1f,1f,0.5f))
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
            isInvisible = false;
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)
                CmdEndInvisible(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command]
    void CmdStartInvisible(uint id) {
        RpcStartInvisible(id);
    }

    [ClientRpc]
    void RpcStartInvisible(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            isInvisible = true;
            gameObject.GetComponent<PlayerHealth>().disableHealthBar();
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,0f);
        }
    }

    [Command]
    void CmdEndInvisible(uint id) {
        RpcEndInvisible(id);
    }

    [ClientRpc]
    void RpcEndInvisible(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            isInvisible = false;
            gameObject.GetComponent<PlayerHealth>().EnableHealthBar();
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
        }
    }
}
