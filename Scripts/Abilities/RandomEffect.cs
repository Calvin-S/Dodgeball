using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RandomEffect : NetworkBehaviour
{
    public GameObject owner;

    [Header("General Stats")]
    public bool diesByTime = true;
    [SerializeField] private float timeHealth = 0;
    [SerializeField] private bool syncPosition = false;
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

    private float timeCreated;
    private bool isSinglePlayer;
    private bool isClientObject;
    private int playerDamage = 0;
    private List<string> effects;
    private Vector3 lastPos;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        lastPos = transform.position;
        timeCreated = Time.time;
        isSinglePlayer = owner.GetComponent<PlayerMovement>().isSinglePlayer;
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
        isClientObject = owner.GetComponent<PlayerMovement>().hasAuthority;
    }

    private void NetworkUpdate() {
        if (Vector3.Distance(lastPos, transform.position) > 0.01f) {
            lastPos = transform.position;
            owner.GetComponent<PlayerMovement>().CmdSyncPosName(gameObject.name, transform.position, transform.rotation, transform.localScale);
        }
    }

    public void setPlayerDamage() {
        Debug.Log(owner);
        if (owner == null) return;
        playerDamage = owner.GetComponent<PlayerAbilities>() != null ? owner.GetComponent<PlayerAbilities>().damage : 0;
    }

    private void Update() {
        if (!isClientObject) return;
        if (syncPosition)
            NetworkUpdate();
        if (diesByTime && Time.time - timeCreated > timeHealth) {
            Destroy(gameObject);
        }
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
        if (other.gameObject.tag == "Player" && other.gameObject != owner) {
            if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()){ return; }
                int r = Random.Range(0, effects.Count);
                string e = effects[r];
                Debug.Log(e);
                if (e == "bleed")
                    bleedTarget(other.gameObject);
                else if (e == "instantDamage")
                    instantDamage(other.gameObject);
                else if (e == "freeze")
                    freezeTarget(other.gameObject);
                else if (e == "stun")
                    stunTarget(other.gameObject);
                destroySelf();
        }
    }

    void destroySelf() {
        if (deathAnim != "")
            anim.SetTrigger(deathAnim);
        if (isClientObject)
            owner.GetComponent<PlayerDamage>().CmdKillBullet(gameObject.name);
        Destroy(gameObject, deathDelay);
    }
}
