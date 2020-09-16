using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerAbilities : NetworkBehaviour
{
    bool stunned = false;
    bool cantMove = false;
    public Abilities normal;
    public Abilities s1;
    public Abilities s2;
    public Abilities s3;
    public Abilities passive;
    public int damage;
    public bool isMainPlayer = true;

    [Header("Ability Display")]
    public Image[] images;

    private Abilities[] abilities;
    private float[] abilityCooldown;
    private bool[] usingInstantCooldown;
    private bool instantCooldownOn;

    private float unstunTime = 0;
    private float unfreezetime = 0;
    private bool lastStunFreeze = false;

    void Start() {
        instantCooldownOn = false;
        abilities = new Abilities[5];
        abilities[0] = passive;
        abilities[1] = s1;
        abilities[2] = s2;
        abilities[3] = s3;
        abilities[4] = normal;
        abilityCooldown = new float[4];
        usingInstantCooldown = new bool[3];
        for (int i = 0; i < 4; i++) {
             abilityCooldown[i] = abilities[i+1].getCooldown();
             if (i < 3)
                usingInstantCooldown[i] = false;
        }
        if (!gameObject.GetComponent<PlayerMovement>().hasAuthority && !gameObject.GetComponent<PlayerMovement>().isLocalPlayer) {
            Debug.Log("stunning " + gameObject.name);
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            stun (true);
            isMainPlayer = false;
        }
    }

    private void stun(bool freezeMovement) {
        stunned = true;
        cantMove = freezeMovement;
        if (freezeMovement) {

            gameObject.GetComponent<PlayerMovement>().enabled = false;
        }

        for (int i = 0; i < 5; i++) {
            if (abilities[i] != null) {
                abilities[i].enabled = false;
                abilities[i].special = i;
            }
        }
    }

    public void unstun() {
        stunned = false;
        cantMove = false;
        gameObject.GetComponent<PlayerMovement>().enabled = true;
        for (int i = 0; i < 5; i++) {
            if (abilities[i] != null)
                abilities[i].enabled = true;
        }
    }

    private void freeze() {
        cantMove = true;
        gameObject.GetComponent<PlayerMovement>().enabled = false;
    }

    public void unfreeze() {
        cantMove = false;
        gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    public void death() {
        for (int i = 0; i < abilities.Length; i++) {
            if (abilities[i] != null) {
                abilities[i].deleteLayover();
                abilities[i].enabled = false;
            }
        }
    }

    public void instantCooldown() {
        instantCooldownOn = true;
        for (int i = 1; i < 4; i++) {
            abilities[i].setCooldown(0);
            usingInstantCooldown[i-1] = true;
        }
    }

    public void resetCooldownToOriginal() {
        for (int i = 1; i < 5; i++) {
            abilities[i].setCooldown(abilityCooldown[i-1]);
        }
    }


    //Requires tomodify is a list of length 4
    public void modifyCooldown(float f, bool[] toModify) {
        if (toModify.Length != 4)
            return;
        for (int i = 1; i < 5; i++) {
            if (toModify[i-1]) {
                abilities[i].setCooldown(abilities[i].getCooldown() * f);
            }
        }
    }

    void LateUpdate() {
        if (instantCooldownOn) {
            for (int i = 1; i < 4; i++) {
                if (Input.GetKey(abilities[i].getAbilityKey())) {
                    abilities[i].setCooldown(abilityCooldown[i-1]);
                    usingInstantCooldown[i-1] = false;
                }
            }
            instantCooldownOn = usingInstantCooldown[0] || usingInstantCooldown[1] || usingInstantCooldown[2];
        }
        for (int i = 0; i < images.Length; i++) {
            Color fade = images[i].GetComponent<Image>().color;
            if (abilities[i+1].getOnCooldown()) {
                fade = new Color(fade.r, fade.g, fade.b, 0.5f);
            }
            else {
                fade = new Color(fade.r, fade.g, fade.b, 1);
            }
            images[i].GetComponent<Image>().color = fade;
        }
        
        if (isMainPlayer && (stunned || cantMove)) {
            if (Time.time - unstunTime >= 0 && Time.time - unfreezetime >= 0) {
                unfreeze();
                unstun();
            }
            else if (Time.time - unstunTime >= 0 && Time.time - unfreezetime < 0) {
                unstun();
                freeze();
            }
            else if (Time.time - unstunTime < 0 && Time.time - unfreezetime >= 0) {
                unfreeze();
                stun(lastStunFreeze);
            }
        }
    }

    public void stunPlayer(bool freezeMovement, float duration) {
        lastStunFreeze = freezeMovement;
        stun (freezeMovement);
        unstunTime = Time.time + duration;
    }


    public void freezePlayer(float duration) {
        freeze();
        unfreezetime = Time.time + duration;
    }

    [Command]
    public void CmdFreezePlayer(float duration, GameObject o) {
        RpcFreezePlayer(duration, o);
    }
    [ClientRpc]
    public void RpcFreezePlayer(float duration, GameObject o) {
        o.GetComponent<PlayerAbilities>().freezePlayer(duration);
    }

    [Command]
    public void CmdStunPlayer(bool freezeMovement, float duration, GameObject o) {
        RpcStunPlayer(freezeMovement, duration, o);
    }
    [ClientRpc]
    public void RpcStunPlayer(bool freezeMovement, float duration, GameObject o) {
        o.GetComponent<PlayerAbilities>().stunPlayer(freezeMovement, duration);
    }

    [Command]
    public void CmdChangeWeather(string weather)
    {
        WeatherManagement.wm.weather = weather; 
    }
}
