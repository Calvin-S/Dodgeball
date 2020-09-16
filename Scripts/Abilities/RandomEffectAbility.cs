using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEffectAbility : Abilities
{
    [Header("Area Passive")]
    [SerializeField] private bool triggerEnter = false;

    [Header("Activatable")]
    [SerializeField] private bool effectAllPlayers = false;

    [Header("Instant Damage")]
    public bool instantDamageOn = false;
    [SerializeField] private float instantDamagePercent = 0;
    [SerializeField] private float damageChance = 1;

    [Header("Bleed")]
    public bool bleed = false;
    [SerializeField] private float bleedTime = 0;
    [SerializeField] private float bleedAgainTime = 0;
    [SerializeField] private float bleedDamagePercent = 0;

    [Header("Slow and Root")]
    public bool slowAndRoot = false;
    [SerializeField] private float slowRootTime = 0;
    [SerializeField] private float slowMultiplier = 0;
    [SerializeField] private float rootMultiplier = 0;

    [Header("Freeze")]
    public bool freeze = false;
    [SerializeField] private float freezeTime = 0;

    [Header("Stun")]
    public bool stun = false;
    [SerializeField] private float stunTime = 0;
    [SerializeField] private bool freezePlayer = false;

    [Header("Animation")]
    [SerializeField] private string deathAnim = "";
    [SerializeField] private float deathDelay = 0;

    private int playerDamage = 0;
    private List<string> effects;
    private float lastActivate;
    public GameObject owner;

    private Vector3 lastPos;
    private Animator anim;
    // Start is called before the first frame update
    void Start() {
        if (owner == null)
            owner = gameObject;
        anim = owner.GetComponent<Animator>();
        isSinglePlayer = owner.GetComponent<PlayerMovement>().isSinglePlayer;
        base.setKey();
        lastActivate = Time.time;

        effects = new List<string>();
        if (bleed)
            effects.Add("bleed");
        if (stun)
            effects.Add("stun");
        if (freeze)
            effects.Add("freeze");
        if (instantDamageOn)
            effects.Add("instantDamage");
        if (slowAndRoot)
            effects.Add("slow");
        setPlayerDamage();
    }

    public void setPlayerDamage() {
        playerDamage = owner.GetComponent<PlayerAbilities>() != null ? owner.GetComponent<PlayerAbilities>().damage : 0;
    }

    private void Update() {

    }

    private void instantDamage(GameObject target) {
        float r = Random.Range(0,1f);
        Debug.Log(playerDamage);
        Debug.Log((int) (playerDamage * instantDamagePercent));
        Debug.Log(r);
        if (r < damageChance)
            owner.GetComponent<PlayerDamage>().takenDamage((int) (playerDamage * instantDamagePercent), target);
    }

    private void bleedTarget(GameObject target) {
        int damage = (int) (playerDamage * bleedDamagePercent);
        if (!isSinglePlayer)
            owner.GetComponent<PlayerContinuousDamage>().CmdAddDamage(damage, bleedAgainTime, bleedTime, target);
        else 
            target.GetComponent<PlayerContinuousDamage>().addDamage(damage, bleedAgainTime, bleedTime);
    }

    private void stunTarget(GameObject target) {
        if (!isSinglePlayer)
            owner.GetComponent<PlayerAbilities>().CmdStunPlayer(freezePlayer, stunTime, target);
        else
            target.GetComponent<PlayerAbilities>().stunPlayer(freezePlayer, stunTime);
    }

    private void freezeTarget(GameObject target) {
        if (!isSinglePlayer) 
            owner.GetComponent<PlayerAbilities>().CmdFreezePlayer(freezeTime, target);
        else
            target.GetComponent<PlayerAbilities>().freezePlayer(freezeTime);
    }

    private void slowTarget(GameObject target) {
        if (!isSinglePlayer) {
            owner.GetComponent<PlayerMovement>().CmdSlow(slowRootTime, slowMultiplier, target);
            owner.GetComponent<PlayerMovement>().CmdRoot(slowRootTime, rootMultiplier, target);
        }
        else {
            target.GetComponent<PlayerMovement>().slow(slowRootTime, slowMultiplier);
            target.GetComponent<PlayerMovement>().root(slowRootTime, rootMultiplier);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (triggerEnter && other.gameObject.tag == "Player" && other.gameObject != owner) {
            if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()){ return; }

            foreach (string e in effects) {
                Debug.Log(e);
                if (e == "bleed")
                    bleedTarget(other.gameObject);
                else if (e == "instantDamage")
                    instantDamage(other.gameObject);
                else if (e == "freeze")
                    freezeTarget(other.gameObject);
                else if (e == "stun")
                    stunTarget(other.gameObject);
            }
        }
    }
}
