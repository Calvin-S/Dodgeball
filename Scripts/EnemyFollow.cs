using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyFollow : NetworkBehaviour
{
    public GameObject owner;
    [SerializeField] private int health = 0;
    [SerializeField] private bool locksOnTarget = false;
    [SerializeField] private bool passThroughGround = false;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float damagePercent;
    private int damage = 0;
    [SerializeField] private float resetTarget = 1; // searches for a new closest target every "resetTarget" seconds
    [SerializeField] private float timeHealth = 0;
    private float timeCreated;
    public bool isSinglePlayer = true;
    private float lastResetTarget;
    private Transform target;
    private Vector3 lastPos;

    void Start()
    {
        lastPos = transform.position;
        damage = (int) damagePercent * owner.GetComponent<PlayerAbilities>().damage;
        timeCreated = Time.time;
        lastResetTarget = Time.time;
        isSinglePlayer = owner.GetComponent<PlayerMovement>().isSinglePlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeCreated >= timeHealth) {
            Destroy (gameObject);
        }
        if (!locksOnTarget && Time.time - lastResetTarget >= resetTarget) 
            target = getClosestPlayer();
        if (target != null && target.position.x > transform.position.x)
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (target != null && target.position.x < transform.position.x)
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (passThroughGround && target != null) {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        if (!isSinglePlayer)
            NetworkUpdate();
        
    }

    void handleSpriteDir() {
        if (target.transform.position.x > transform.position.x) 
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    Transform getClosestPlayer() {
        Transform t = null;
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Player")) {
            if (t == null && o != owner)
                t = o.transform;
            else if (o != owner && diff (transform, o.transform) < diff(transform, t))
                t = o.transform;
        }
        return t;
    }

    float diff (Transform t1, Transform t2) {
        return Vector3.Distance(t1.position, t2.position);
    }
 
    private void NetworkUpdate() {
       if (Vector3.Distance (lastPos, transform.position) > 0.01f && owner.GetComponent<PlayerMovement>() != null && owner.GetComponent<PlayerMovement>().hasAuthority) {
            lastPos = transform.position;
            owner.GetComponent<PlayerMovement>().CmdSyncPosName(gameObject.name, transform.position, transform.rotation, transform.localScale);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" && !passThroughGround) {
            if (isSinglePlayer) {
                health -= 1;
                Destroy(other.gameObject);
            }
            else {
                health -= 1;
                if (owner.GetComponent<PlayerMovement>().isServer) {
                    owner.GetComponent<PlayerDamage>().RpcDestroyGround(other.gameObject);
                }
                else {
                    owner.GetComponent<PlayerDamage>().CmdDestroyGround(other.gameObject);
                }
            }
        }
        if (other.gameObject.tag == "Player" && other.gameObject != owner) {
            if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()){

            }
            else {
                health -= 1;
                if (owner.GetComponent<PlayerMovement>().hasAuthority)
                    owner.GetComponent<PlayerDamage>().takenDamage(damage, other.gameObject);
            }
        }
        if (health <= 0)  {
            if (isSinglePlayer)
                Destroy(gameObject);
            else if (owner.GetComponent<PlayerMovement>().hasAuthority) {
                if (owner.GetComponent<PlayerMovement>().isServer)
                    owner.GetComponent<PlayerDamage>().RpcDestroyObject(gameObject.name, 0);
                else {
                    owner.GetComponent<PlayerDamage>().CmdDestroyObject(gameObject.name, 0);
                }
            }
            Destroy(gameObject);
        }
    } 

}
