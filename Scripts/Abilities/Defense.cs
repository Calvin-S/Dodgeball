using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Defense : Abilities
{
    //cooldown ADDS ONTO defnseModifierTime
    [SerializeField] private float damageMultiplier = 1;
    [SerializeField] private float damageMultiplierTime = 0;
    [SerializeField] private int addedShield = 0;
    private float lastActivate;
    private bool damageMultiplierOn = false;
    // Start is called before the first frame update
    void Start()
    {
        if (damageMultiplier != 1)
            cooldown += damageMultiplierTime;
        base.setKey();
        lastActivate = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        onCooldown = Time.time - lastActivate < cooldown;
        if (Time.time - lastActivate >= cooldown && Input.GetKey(abilityKey) && !damageMultiplierOn) {
            lastActivate = Time.time;
            damageMultiplierOn = true;
            gameObject.GetComponent<PlayerHealth>().addShield(addedShield);
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(damageMultiplier);
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
                CmdBeginDefense();
            base.spawnLayover();
        }
        else if (Time.time - lastActivate >= cooldown && damageMultiplierOn){
            Debug.Log("off");
            damageMultiplierOn = false;
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(1);
            if (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
                CmdEndDefense();
            base.deleteLayover();
        }
    }

    [Command]
    void CmdBeginDefense() {
        RpcBeginDefense();
    }

    [ClientRpc] 
    void RpcBeginDefense() {
        if (!isLocalPlayer) {
            damageMultiplierOn = true;
            gameObject.GetComponent<PlayerHealth>().addShield(addedShield);
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(damageMultiplier);
            base.spawnLayover();
        }
    }

    [Command]
    void CmdEndDefense() {
        RpcEndDefense();
    }

    [ClientRpc] 
    void RpcEndDefense() {
        if (!isLocalPlayer) {
            damageMultiplierOn = false;
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(1);
            base.deleteLayover();
        }
    }
}
