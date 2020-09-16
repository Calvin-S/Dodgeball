using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    public GameObject healthbar;
    SpriteRenderer sr;
    Rigidbody2D rb;
    public float speed;
    public float jumpForce;
    public float wallSpeed;
    public bool flying = false;
    private float speedMultiplier = 1;
    private float jumpMultiplier = 1;

    bool isGrounded = false;
    public bool isWall = false;
    bool isRight = true;
    bool moving = false;
    public bool isSinglePlayer = true;

    public Transform isGroundedChecker;
    public Transform isGroundedChecker1;
    public Transform isWallCheckerR;
    public Transform isWallCheckerL;
    public float checkGroundRadius;

    public LayerMask groundLayer;
    public float fallMultiplier;
    public float rememberGroundedFor;
    public float rememberWallFor;
    float lastTimeGrounded;
    float lastTimeWall;

    public KeyCode up = KeyCode.UpArrow;
    public KeyCode right = KeyCode.RightArrow;
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode normalShot = KeyCode.Space;
    public KeyCode special1 = KeyCode.Z;
    public KeyCode special2 = KeyCode.X;
    public KeyCode special3 = KeyCode.C;

    private Vector3 lastPos;
    private Animator anim;
    public WallChecker isWallClimbAnimL;
    public WallChecker isWallClimbAnimR;

    private Vector2 maxVel;
    private Vector3 lastSendPos;
    private float lastTimeSendPos;

    void Start()
    {
        lastSendPos = transform.position;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lastPos = transform.position;
        Physics2D.IgnoreLayerCollision(9,9);
        healthbar = gameObject.transform.Find("Canvas").gameObject.transform.Find("HealthBar").gameObject;
        if (flying) 
            rb.gravityScale = 0;
        maxVel = Vector2.zero;
    }
    private bool updateOnce = true;

    void Update()
    {
        maxVel = Vector2.zero;
        if (updateOnce && (isServer || isClient)) {
            if (isLocalPlayer && GameObject.Find("Main Camera").GetComponent<CameraFollow>() != null) {
                GameObject.Find("Main Camera").GetComponent<CameraFollow>().target = gameObject;
                GameObject.Find("Main Camera").GetComponent<CameraFollow>().follow = true;
            }
            isSinglePlayer = false;
            gameObject.GetComponent<PlayerHealth>().isSinglePlayer = false;
            updateOnce = false;
        }
        else if (updateOnce && !(isServer || isClient)){
            isSinglePlayer = true;
            gameObject.GetComponent<PlayerHealth>().isSinglePlayer = true;
            updateOnce = false;
        }
        if (gameObject.GetComponent<PlayerAbilities>() != null)
            gameObject.GetComponent<PlayerAbilities>().isMainPlayer = isLocalPlayer || isSinglePlayer;
        
        //resetting speed multiplier
        if (speedMultiplier != 1 && Time.time - slowdownTime >= 0)
            speedMultiplier = 1;
        if (jumpMultiplier != 1 && Time.time - jumpSlowdownTime >= 0)
            jumpMultiplier = 1;

        if (isSinglePlayer)
            SinglePlayer();
        else if (!isSinglePlayer && isLocalPlayer) {
            moving = Vector3.Distance (lastPos, transform.position) > 0;
            lastPos = transform.position;
            if (Input.GetKey(up) || Input.GetKey(right) || Input.GetKey(left) || Input.GetKey(down) || moving) {
                Multiplayer();
                //CmdUpdatePos(gameObject, transform.position, transform.rotation, transform.localScale, gameObject.transform.Find("Canvas").Find("HealthBar").transform.localScale);
            }
            else {
                HandleAnimation();
                ExternalForce();
            }
        }
        
        if (!isSinglePlayer && !isLocalPlayer) {
            rb.gravityScale = 0.0f;
        }
    }

    [Command]
    public void CmdSyncPosName(string name, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale) {
        RpcSyncPosName(name, syncedPos, syncedRotation, syncedScale);
    }

    [ClientRpc]
    public void RpcSyncPosName(string name, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale) {
        GameObject o = GameObject.Find(name);
        if (o == null) return;
        o.transform.position = syncedPos;
        o.transform.rotation = syncedRotation;
        o.transform.localScale = syncedScale;
    }

    [ClientRpc]
    public void RpcSyncedPos(GameObject p, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale, Vector3 syncedHpScale) {
        if ((p.GetInstanceID() == gameObject.GetInstanceID() && !isLocalPlayer)) {
            p.transform.position = syncedPos;
            p.transform.rotation = syncedRotation;
            p.transform.localScale = syncedScale;
            p.transform.Find("Canvas").Find("HealthBar").transform.localScale = syncedHpScale;
        }
    }

    [Command]
    void CmdUpdatePos(GameObject p, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale, Vector3 syncedHpScale) {
        RpcSyncedPos(p, syncedPos, syncedRotation, syncedScale, syncedHpScale);
    }

    void Multiplayer() {
        if (isLocalPlayer) {
            CheckIfGrounded();
            if (!flying) {
                CheckIfWall();
                bool wallJumping = Jump();
                Move(wallJumping);
                BetterJump();
            }
            else {
                Move(false);
            }
            HandleAnimation();
            maxVel = new Vector2 (Mathf.Abs(speed * speedMultiplier), Mathf.Abs(speed * speedMultiplier));
            if (!flying)
                maxVel.y = Mathf.Max(Mathf.Abs(jumpForce * jumpMultiplier), Mathf.Abs(wallSpeed * speedMultiplier));
            ExternalForce();
        }
    }
    
    void SinglePlayer() {
        CheckIfGrounded();
        if (!flying) {
            CheckIfWall();
            bool wallJumping = Jump();
            Move(wallJumping);
            BetterJump();
        }
        else {
            Move(false);
        }
        HandleAnimation();
        maxVel = new Vector2 (Mathf.Abs(speed * speedMultiplier), Mathf.Abs(speed * speedMultiplier));
        if (!flying)
            maxVel.y = Mathf.Max(Mathf.Abs(jumpForce * jumpMultiplier), Mathf.Abs(wallSpeed * speedMultiplier));
        ExternalForce();
    }

    void HandleAnimation() {
        anim.SetFloat("xDir", rb.velocity.x);
        anim.SetFloat("yDir", rb.velocity.y);
        CheckIfGrounded();
        anim.SetBool("isGrounded", isGrounded);
    }

    void Move(bool wallJumping) {
        float x = Input.GetKey(right) ? 1 : Input.GetKey(left) ? -1 : 0;
        float moveBy = x * speed * speedMultiplier;
        if ( !wallJumping || (wallJumping && !sameDir(x) && x != 0)) {
            rb.velocity = new Vector2(moveBy, rb.velocity.y);
        }
        Vector3 temp = transform.localScale;
        Vector3 healthTemp = healthbar.transform.localScale;
        if (x < 0 && isRight) {
            temp.x *= -1;
            healthTemp.x *= -1;
            transform.localScale = temp;
            healthbar.transform.localScale = healthTemp;
            isRight = false;
        }
        else if (x > 0 && !isRight) {
            temp.x *= -1;
            healthTemp.x *= -1;
            transform.localScale = temp;
            healthbar.transform.localScale = healthTemp;
            isRight = true;
        }
        if (flying) {
            fly();
            float y = Input.GetKey(up) ? 1 : Input.GetKey(down) ? -1 : 0;
            moveBy = y * speed * speedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, moveBy);
        }
        else { cantFly(); }
    }

    public void fly() {
        flying = true;
        rb.gravityScale = 0;
    }
    
    public void cantFly() {
        flying = false;
        rb.gravityScale = 1;
    }
    
    // Smoothes falling and jumping animation
    void BetterJump() {
        if (rb.velocity.y < 0) {
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    // checks if xDir is same as isright
    bool sameDir(float xDir) {
        if (xDir > 0 && isRight)
            return true;
        else if (xDir < 0 && !isRight)
            return true;
        return false;
    }

    // Allows jumping and wall climbing. Returns true when wall climbing
    bool Jump() {
        if (anim.GetBool("Jump") && !isGrounded) {
            anim.SetBool("Jump", false);
        }
        if (Input.GetKey(up) && (isWall || (!isGrounded && Time.time - lastTimeWall <= rememberWallFor))) {
             rb.velocity = new Vector2(rb.velocity.x, wallSpeed * speedMultiplier);
             if (isWallClimbAnimL == null && isWallClimbAnimR == null)
                anim.SetBool("WallClimb", true);
            else {
                bool temp = isWallClimbAnimL.hitWall || isWallClimbAnimR.hitWall;
                anim.SetBool("WallClimb", temp);
            }
             return true;
        }
        else if (Input.GetKey(up) && (isGrounded || Time.time - lastTimeGrounded <= rememberGroundedFor)) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
            anim.SetBool("Jump", true);
        }
        anim.SetBool("WallClimb", false);
        return false;
    }

    void CheckIfGrounded() {
        Collider2D collider = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);
        Collider2D collider1 = Physics2D.OverlapCircle(isGroundedChecker1.position, checkGroundRadius, groundLayer);
        if (collider != null || collider1 != null) {
            isGrounded = true;
        } else {
            if (isGrounded) 
                lastTimeGrounded = Time.time;
            isGrounded = false;
        }
    }

    void CheckIfWall() {
        if (isWallCheckerR.GetComponent<WallChecker>().hitWall || isWallCheckerL.GetComponent<WallChecker>().hitWall) {
            isWall = true;
        } else {
            if (isWall) {
                lastTimeWall = Time.time;
            }
            isWall = false;
        } 
    }

    public bool movementKeyDown() {
        return Input.GetKey(up) || Input.GetKey(down) || Input.GetKey(right) || Input.GetKey(left);
    }

    public bool abilityKeyDown() {
        return Input.GetKey(special1) || Input.GetKey(special2) || Input.GetKey(special3);
    }

    void ExternalForce() {
        Vector2 impulse = GetComponent<PlayerKnockback>().calculateImpulse();
        Vector2 forces = GetComponent<PlayerKnockback>().calculateForce();
        rb.AddForce(impulse, ForceMode2D.Impulse);
        rb.AddForce(forces, ForceMode2D.Force);
        maxVel += new Vector2 (Mathf.Abs(impulse.x), Mathf.Abs(impulse.y));
        maxVel += new Vector2 (Mathf.Abs(forces.x), Mathf.Abs(forces.y));
        if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(maxVel.x))
            rb.velocity = new Vector2 (rb.velocity.x / Mathf.Abs(rb.velocity.x) * Mathf.Abs(maxVel.x), rb.velocity.y);
        if (Mathf.Abs(rb.velocity.y) > Mathf.Abs(maxVel.y) && rb.velocity.y > 0)
            rb.velocity = new Vector2 (rb.velocity.x, rb.velocity.y / Mathf.Abs(rb.velocity.y) * Mathf.Abs(maxVel.y));
        if ( Time.time - lastTimeSendPos >= 0.3f || (rb.velocity != Vector2.zero && Vector3.Distance(transform.position, lastSendPos) >= 0.01f)) {
            lastSendPos = transform.position;
            lastTimeSendPos = Time.time;
            CmdUpdatePos(gameObject, transform.position, transform.rotation, transform.localScale, gameObject.transform.Find("Canvas").Find("HealthBar").transform.localScale);
        }
    }

    public void applyForce(Vector2 f, float duration, bool impulse, GameObject target) {
        if (gameObject.GetComponent<PlayerMovement>().hasAuthority && f != Vector2.zero) {
            CmdApplyForce(f, duration, impulse, target);
        }
    }
    [Command]
    public void CmdApplyForce(Vector2 f, float duration, bool impulse, GameObject target) {
        RpcApplyForce(f, duration, impulse, target);
    }
    [ClientRpc]
    void RpcApplyForce(Vector2 f, float duration, bool impulse, GameObject target) {
        if (target.GetComponent<PlayerMovement>().isLocalPlayer) {
            if (impulse)
                target.GetComponent<PlayerKnockback>().addImpulseForce(f, duration);
            else
                target.GetComponent<PlayerKnockback>().addForce(f, duration);
        }
    }

    private float slowdownTime = 0;
    private float jumpSlowdownTime = 0;

    public void slow(float slowdownTime, float speedMultiplier) {
        this.slowdownTime = Time.time + slowdownTime;
        this.speedMultiplier = speedMultiplier;
    }

    public void root(float jumpSlowdownTime, float jumpMultiplier) {
        this.jumpSlowdownTime = Time.time + jumpSlowdownTime;
        this.jumpMultiplier = jumpMultiplier;
    }

    [Command]
    public void CmdRoot(float jumpSlowdownTime, float jumpMultiplier, GameObject target) {
        RpcRoot(jumpSlowdownTime, jumpMultiplier, target);
    }

    [ClientRpc]
    public void RpcRoot(float jumpSlowdownTime, float jumpMultiplier, GameObject target) {
        target.GetComponent<PlayerMovement>().root(jumpSlowdownTime, jumpMultiplier);
    }

    [Command]
    public void CmdSlow(float slowdownTime, float speedMultiplier, GameObject target) {
        RpcSlow(slowdownTime, speedMultiplier, target);
    }

    [ClientRpc]
    public void RpcSlow(float slowdownTime, float speedMultiplier, GameObject target) {
        target.GetComponent<PlayerMovement>().slow(slowdownTime, speedMultiplier);
    }

}
