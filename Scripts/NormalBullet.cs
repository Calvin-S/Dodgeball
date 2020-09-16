using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NormalBullet : NetworkBehaviour
{
    public GameObject shooter;
    [Header("General Attributes")]
    [SerializeField] private float damagePercent = 0;
    private int damage = 0;
    [SerializeField] private int health = 0;
    [SerializeField] private float speed = 0;
    [SerializeField] private float timeHealth = 0;
    [SerializeField] private bool canDestroyGround = true;
    [SerializeField] private bool passThroughGround = false;
    [SerializeField] private bool bounceOffGround = false;
    [SerializeField] private bool takeDamageFromGround = true;
    [SerializeField] private bool continuousDamage = false;
    [SerializeField] private bool persists = false; //this projectile will only die from time
    [Header("Animation")]
    [Tooltip("Sets the death delay. Should be based off of death animation time.")]
    [SerializeField] private float deathDelay = 0;
    [SerializeField] private string deathAnim = "";
    private Animator anim;

    private bool update; // used for updating after transform.localScale.x is changed from shooting
    float timeCreated;
    public Vector2 dir; // dir should always be pos.
    public bool isSinglePlayer = true;

    public LayerMask groundLayer;
    public LayerMask unitLayer;
    [Header("Scaling")]
    [SerializeField] private float scale = 1; //will scale by this amount each frame e.g. 1.1 is 1.1x per frame
    [SerializeField] private float scaleTime = 1000; //will scale by 'scale' every scaleTime seconds
    private float lastScaleTime;

    private bool isClientBullet = false;
    private bool alreadyDead;  
    private Vector3 lastPos;

    void Start() {
        alreadyDead = false;
        if (deathAnim != "")
            anim = gameObject.GetComponent<Animator>();
        damage = (int) damagePercent * shooter.GetComponent<PlayerAbilities>().damage;
        timeCreated = Time.time;
        if (dir.x < 0) 
            Debug.LogError ("Direction x of bullet must be greater than zero.");
        lastScaleTime = Time.time;
        if (shooter.GetComponent<PlayerMovement>().hasAuthority)
            isClientBullet = true;
        lastPos = transform.position;
    }

    public void setTimeHealth(float h) {
        timeHealth = h;
    }

    public float getSpeed() {return speed;}

    public void updateTrue() {
        update = true;
    }

    void Update()
    {
        bullet();
    } 

    public void destroySelf() {
        if (alreadyDead) return;
        alreadyDead = true;
        if (deathAnim != "")
            anim.SetTrigger(deathAnim);
        Debug.Log(isClientBullet + " " + isSinglePlayer);
        if (isClientBullet && !isSinglePlayer)
            shooter.GetComponent<PlayerDamage>().CmdKillBullet(gameObject.name);
        Destroy(gameObject, deathDelay);
    }

    public bool die = false;
    public void dies() {
        if(die)
        destroySelf();
    }

    void bullet() {
        dies();
        if (Time.time - timeCreated >= timeHealth || health <= 0) {
            Debug.Log("death");
            destroySelf();
        }
        if (alreadyDead || !isClientBullet)
            return;
        if (Time.time - lastScaleTime >= scaleTime) {
            transform.localScale = transform.localScale * scale;
            lastScaleTime = Time.time;
        }
        if (dir.x != 0 && dir.y != 0)
            dir.Normalize();
        if (update) {
            if (dir.x == 0 && dir.y == 0) 
                transform.rotation = transform.rotation;
            else if (dir.x != 0 && transform.localScale.x > 0)
                transform.rotation = Quaternion.Euler(0f,0f, 180 / Mathf.PI * Mathf.Atan(dir.y / dir.x));
            else if (dir.x != 0 && transform.localScale.x < 0)
                transform.rotation = Quaternion.Euler(0f,0f, 360 - (180 / Mathf.PI * Mathf.Atan(dir.y / dir.x)));
            else if (dir.y > 0)
                transform.rotation = Quaternion.Euler(0f,0f,90f);
            else 
                transform.rotation = Quaternion.Euler(0f,0f,-90f);
            update = false;
            isSinglePlayer = shooter.GetComponent<PlayerMovement>().isSinglePlayer;
        }
        move();

        if (!isSinglePlayer && shooter == null) {
            Destroy(gameObject);
        }
    }

    void move() {
        if (transform.localScale.x < 0 && dir.x != 0) 
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        else 
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        if (Vector3.Distance (lastPos, transform.position) > 0.01f && shooter.GetComponent<PlayerMovement>() != null && shooter.GetComponent<PlayerMovement>().hasAuthority) {
            lastPos = transform.position;
            networkUpdate();
        }
    }

    public void networkUpdate() {
        if (!isSinglePlayer)
                shooter.GetComponent<PlayerMovement>().CmdSyncPosName(gameObject.name, transform.position, transform.rotation, transform.localScale);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (alreadyDead || !isClientBullet) return;
        if (continuousDamage && other.gameObject.tag == "Player" && other.gameObject != shooter) {
            if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()){
            }
            else {
                health -= 1;
                shooter.GetComponent<PlayerDamage>().takenDamage(damage, other.gameObject);
            }
        }
        if (health <= 0 && !persists) {
            destroySelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (alreadyDead || !isClientBullet) return;
        if (other.gameObject.tag == "Ground" && bounceOffGround) {
            transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Vector3 move = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            transform.Translate(move * 0.1f);
        }
        if (other.gameObject.tag == "Ground" && !passThroughGround && canDestroyGround) {
            if (isSinglePlayer) {
                if (takeDamageFromGround)
                    health -= 1;
                if (deathAnim != "")
                    anim.SetTrigger(deathAnim);
                Destroy(other.gameObject);
            }
            else {
                if (takeDamageFromGround)
                    health -= 1;
                if (shooter.GetComponent<PlayerMovement>().isServer) {
                    shooter.GetComponent<PlayerDamage>().RpcDestroyGround(other.gameObject);
                }
                else if (shooter.GetComponent<PlayerMovement>().hasAuthority){
                    shooter.GetComponent<PlayerDamage>().CmdDestroyGround(other.gameObject);
                }
            }
        }
        else if (other.gameObject.tag == "Ground" && !passThroughGround && !canDestroyGround && takeDamageFromGround)
            health = 0;
        
        if (!continuousDamage && other.gameObject.tag == "Player" && other.gameObject != shooter) {
            if (other.gameObject.GetComponent<Phase>() != null && other.gameObject.GetComponent<Phase>().getPhase()){ return; }
            else {
                health -= 1;
                shooter.GetComponent<PlayerDamage>().takenDamage(damage, other.gameObject);
            }
        }
        if (health <= 0 && !persists)  {
            destroySelf();
        }
    }
}
