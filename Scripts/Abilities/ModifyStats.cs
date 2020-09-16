using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModifyStats : Abilities
{

    [Header("Event Based Activating")]
    [SerializeField] private bool inRain = false;
    [SerializeField] private bool inSnow = false;
    [Header("Activatable Ability")]
    [SerializeField] private float updatedStatsTime = 0;
    [SerializeField] private bool shieldWillBeRemoved = false;

    [Header("StatsToChange")]
    [SerializeField] private int addShield = 0;
    [SerializeField] private int changeJumpForce = 0;
    [SerializeField] private int changeSpeed = 0;
    [SerializeField] private int changeWallSpeed = 0;
    [SerializeField] private int changeDamageOutput = 0; 
    [SerializeField] private int setDamageMultiplier = 1; // REPRESENTS THE DAMAGE YOU TAKE IN MULTIPLIED

    
    [Header("HealthCondition")]
    public bool healthCondition;
    [SerializeField] private int healthLessThan = 0;
    [SerializeField] private float healthPercentLessThan = 0;

    [Header("SpeedCondition")]
    public bool speedCondition;
    [SerializeField] private int speedLessThan = 0;
    
    private bool updated;
    private float lastActivated;
    void Start() {
        base.setKey();
        updated = false;
        lastActivated = Time.time;
        if (inRain && !inSnow) {
            WeatherManagement.wm.onRain += onActivateEvent;
            WeatherManagement.wm.onClear += onDeactivateEvent;
            WeatherManagement.wm.onSnow += onDeactivateEvent;
        }
        else if (inSnow && !inRain) {
            WeatherManagement.wm.onSnow += onActivateEvent;
            WeatherManagement.wm.onClear += onDeactivateEvent;
            WeatherManagement.wm.onRain += onDeactivateEvent;
        }
        
    }

    private void OnDestroy() {
        if (inRain && !inSnow) {
            WeatherManagement.wm.onRain -= onActivateEvent;
            WeatherManagement.wm.onClear -= onDeactivateEvent;
            WeatherManagement.wm.onSnow -= onDeactivateEvent;
        }
        else if (inSnow && !inRain) {
            WeatherManagement.wm.onSnow -= onActivateEvent;
            WeatherManagement.wm.onClear -= onDeactivateEvent;
            WeatherManagement.wm.onRain -= onDeactivateEvent;
        }
    }

    void Update() {
        if (healthCondition && !updated && cooldown == 0) {
            if (healthLessThan != 0 && gameObject.GetComponent<PlayerHealth>().getHealth() < healthLessThan)
                handleUpdate();
            else if (healthPercentLessThan != 0 && gameObject.GetComponent<PlayerHealth>().getHealth() < gameObject.GetComponent<PlayerHealth>().maxHealth * healthPercentLessThan)
                handleUpdate();
            
        }
        else if (speedCondition && !updated && cooldown == 0) {
            if (speedLessThan != 0 && gameObject.GetComponent<PlayerMovement>().speed < speedLessThan)
                handleUpdate();
        }
        else if (abilityKey != KeyCode.None && cooldown != 0) {
            onCooldown = Time.time - lastActivated < cooldown;
            if (Time.time - lastActivated >= cooldown && Input.GetKey(abilityKey)) {
                updateStats();
            }
            else if (Time.time - lastActivated >= updatedStatsTime && updated) {
                reverseUpdateStats();
            }
        }
    }

    void handleUpdate() {
        if (isSinglePlayer) 
            updateStats();
        else if (!isSinglePlayer && isLocalPlayer) {
            updateStats();
            CmdUpdateStats(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    void handleRevertUpdate() {
        if (isSinglePlayer) 
            reverseUpdateStats();
        else if (!isSinglePlayer && isLocalPlayer) {
            reverseUpdateStats();
            CmdRevertUpdateStats(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command]
    void CmdRevertUpdateStats(uint id) {
        RpcRevertUpdateStats(id);
    }
    [ClientRpc]
    void RpcRevertUpdateStats(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            reverseUpdateStats();
        }
    }

    [Command]
    void CmdUpdateStats(uint id) {
        RpcUpdateStats(id);
    }
    [ClientRpc]
    void RpcUpdateStats(uint id) {
        if (!isLocalPlayer && gameObject.GetComponent<NetworkIdentity>().netId == id) {
            updateStats();
        }
    }

    void updateStats() {
        if (addShield > 0)
            gameObject.GetComponent<PlayerHealth>().addShield(addShield);
        if (changeSpeed != 0)
            gameObject.GetComponent<PlayerMovement>().speed += changeSpeed;
        if (changeJumpForce != 0)
            gameObject.GetComponent<PlayerMovement>().jumpForce += changeJumpForce;
        if (changeWallSpeed != 0)
            gameObject.GetComponent<PlayerMovement>().wallSpeed += changeWallSpeed;
        if (changeDamageOutput != 0)
            gameObject.GetComponent<PlayerAbilities>().damage += changeDamageOutput;
        if (setDamageMultiplier != 1)
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(setDamageMultiplier);
        updated = true;
    }

    // used for active key ability. NOTE: DAMAGEMULTIPLIER set to 1
    void reverseUpdateStats() {
        if (addShield > 0 && shieldWillBeRemoved)
            gameObject.GetComponent<PlayerHealth>().addShield(-addShield);
        if (changeSpeed != 0)
            gameObject.GetComponent<PlayerMovement>().speed -= changeSpeed;
        if (changeJumpForce != 0)
            gameObject.GetComponent<PlayerMovement>().jumpForce -= changeJumpForce;
        if (changeWallSpeed != 0)
            gameObject.GetComponent<PlayerMovement>().wallSpeed -= changeWallSpeed;
        if (changeDamageOutput != 0)
            gameObject.GetComponent<PlayerAbilities>().damage -= changeDamageOutput;
        if (setDamageMultiplier != 1)
            gameObject.GetComponent<PlayerDamage>().setDamageMultiplier(1);
        updated = false;
    }

    private void onActivateEvent() {
        handleUpdate();
        Debug.Log("buff " + gameObject.GetComponent<PlayerMovement>().speed);
    }

    private void onDeactivateEvent() {
        handleRevertUpdate();
        Debug.Log("debuff " + gameObject.GetComponent<PlayerMovement>().speed);
    }

}
