using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Melee : NetworkBehaviour
{
    [SerializeField] private bool canDestroyGround = true;
    [SerializeField] private float timeHealth = 0.05f;
    [SerializeField] private float damagePercent;
    private int damage = 0;
    [Header("Forces")]
    public bool explosiveForce = true;
    [SerializeField] private Vector2 ForceAppliedOnTarget = new Vector2 (0,0);
    [SerializeField] private float durationOnTarget = 0;
    [SerializeField] private Vector2 ForceAppliedOnSelf = new Vector2 (0,0);
    
    [Header("Bleed/Continuous Damage")]
    [SerializeField] private bool bleedDamage = false;
    [SerializeField] private bool continuousDamage = false;
    [SerializeField] private float timeDamageAgain = 0;
    [SerializeField] private float bleedDuration = 0;
    [Header("Animation and others")]
    private List<GameObject> hitBefore;
    public string animBool = "";
    private bool isSinglePlayer = true;
    public GameObject owner;
    private float timeCreated;
    private bool isDead;
    private List<float> lastTimeHit;
    private List<GameObject> toHit;

    // Start is called before the first frame update
    void Start()
    {
        damage = (int) (damagePercent * owner.GetComponent<PlayerAbilities>().damage);
        isDead = false;
        isSinglePlayer = owner.GetComponent<PlayerMovement>().isSinglePlayer;
        timeCreated = Time.time;
        if (continuousDamage) {
            lastTimeHit = new List<float>();
            toHit = new List<GameObject>();
        }

        float invertX = owner.transform.localScale.x > 0 ? 1 : -1;
        ForceAppliedOnSelf = new Vector2 (ForceAppliedOnSelf.x * invertX, ForceAppliedOnSelf.y);

        if (owner.GetComponent<PlayerMovement>().hasAuthority)
            owner.GetComponent<PlayerKnockback>().addImpulseForce(ForceAppliedOnSelf, timeHealth);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Time.time - timeCreated > timeHealth) {
            if (animBool != "")
                    owner.GetComponent<Animator>().SetBool(animBool, false);
            owner.GetComponent<PlayerDamage>().destroyObject(gameObject);
            Destroy(gameObject);
        }
        else {
            if (animBool != "")
                owner.GetComponent<Animator>().SetBool(animBool, true);
        }
    }

    private void OnTriggerExit(Collider other) {
        int otherIndex = toHit.IndexOf(other.gameObject);
        if (otherIndex >= 0) {
            lastTimeHit.RemoveAt(otherIndex);
            toHit.RemoveAt(otherIndex);
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Player" && other.gameObject != owner) {
            if (continuousDamage && !bleedDamage && owner.GetComponent<PlayerMovement>().hasAuthority) {
                for (int i = 0; i < lastTimeHit.Count; i++) {
                    if (Time.time - lastTimeHit[i] >= timeDamageAgain) {
                        owner.GetComponent<PlayerDamage>().takenDamage(damage, other.gameObject);
                        lastTimeHit[i] = Time.time;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" && canDestroyGround) {
            if (isSinglePlayer) {
                Destroy(other.gameObject);
            }
            else {
                owner.GetComponent<PlayerDamage>().destroyObject(other.gameObject);
                Destroy(other.gameObject);
            }
        }

        if (other.gameObject.tag == "Player" && other.gameObject != owner) {
            if (!isSinglePlayer) {
                if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()) { return; }
                
                if (!bleedDamage && !continuousDamage && owner.GetComponent<PlayerMovement>().hasAuthority) {
                    owner.GetComponent<PlayerDamage>().takenDamage(damage, other.gameObject);
                }
                else if (bleedDamage && !continuousDamage && owner.GetComponent<PlayerMovement>().hasAuthority) {
                    if (notHitBefore(other.gameObject)) {
                        owner.GetComponent<PlayerContinuousDamage>().CmdAddDamage(damage, timeDamageAgain, bleedDuration, other.gameObject);
                    }
                }
                else if (continuousDamage) {
                    toHit.Add(other.gameObject);
                    lastTimeHit.Add(Time.time);
                }

                float invertX = owner.transform.localScale.x > 0 ? 1 : -1;
                ForceAppliedOnTarget = new Vector2 (Mathf.Abs(ForceAppliedOnTarget.x) * invertX, ForceAppliedOnTarget.y);
                if (notHitBefore(other.gameObject))
                    owner.GetComponent<PlayerMovement>().applyForce(ForceAppliedOnTarget, durationOnTarget, true, other.gameObject);
                addHitBefore(other.gameObject);
                
            }

        }
    }

    // returns if GameObject o has not been hit before
    private bool notHitBefore(GameObject o) {
        return hitBefore == null || (hitBefore != null && !hitBefore.Contains(o.gameObject));
    }

    private void addHitBefore(GameObject o) {
        if (hitBefore == null)
            hitBefore = new List<GameObject>();
        hitBefore.Add(o);
    }

}
