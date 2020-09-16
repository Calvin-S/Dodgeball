using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModifyCooldown : Abilities
{
    /* this class modifies the cooldowns of normal and abilities based off of cooldownModifier. 
    cooldown ADDS onto modifierTime.*/

    public PlayerAbilities playerAbilities;
    [SerializeField] private float modifierTime = 0;
    [SerializeField] private float cooldownModifier = 1;
    [SerializeField] private bool ability1 = false;
    [SerializeField] private bool ability2 = false;
    [SerializeField] private bool ability3 = false;
    [SerializeField] private bool normal = false;
    
    private float lastActivated;
    private bool[] toModify;
    void Start() {
        base.setKey();
        lastActivated = Time.time;
        toModify = new bool[4];
        toModify[0] = ability1;
        toModify[1] = ability2;
        toModify[2] = ability3;
        toModify[3] = normal;
        cooldown += modifierTime; 
    }
    
    void Update() {
        onCooldown = Time.time - lastActivated < cooldown;
        if (Time.time - lastActivated >= cooldown && Input.GetKey(abilityKey)) {
            playerAbilities.modifyCooldown(cooldownModifier, toModify);
            lastActivated = Time.time;
        }
        else if (Time.time - lastActivated >= modifierTime) {
            playerAbilities.resetCooldownToOriginal();
        }
    }
}
