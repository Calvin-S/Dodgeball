using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerShoot : Abilities {
    public GameObject normal; 
    float lastNormalShot;
    public KeyCode normalShot = KeyCode.None;
    private string bulletName;
    private Animator anim;
    public float delaySpawn = 0;
    public float offsetY = 0; // positive means higher up e.g. offsetY of 3 will shoot the bullet 3 Y pos higher 
    public float offsetX = 0; // should always be positive
    private bool spawn;
    Vector3 lastPos;
    // Start is called before the first frame update
    void Start()
    {
        base.setKey();
        if (normalShot == KeyCode.None)
            normalShot = abilityKey;
        anim = gameObject.GetComponent<Animator>();
        gameObject.name = gameObject.name + gameObject.GetInstanceID();
        bulletName = normal.name;
        spawn = false;
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.x < 0)
            offsetX = -Mathf.Abs(offsetX);
        else
            offsetX = Mathf.Abs(offsetX);
        shoot();
    }

    public bool shootKeyDown() {
        return Input.GetKey(normalShot);
    }
    
    void shoot() {
        onCooldown = Time.time - lastNormalShot < cooldown;
        Vector3 pos = new Vector3 (transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z);
        if (Input.GetKey(normalShot) && Time.time - lastNormalShot >= cooldown) {
            spawn = true;
            lastNormalShot = Time.time;
            if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
                anim.SetTrigger("Shoot");
            else if (isLocalPlayer) {
                anim.SetTrigger("Shoot");
                gameObject.GetComponent<NetworkAnimator>().SetTrigger("Shoot");
            }
        }
        if (spawn && Time.time - lastNormalShot >= delaySpawn) {
            spawn = false;
            spawnBullet(pos);
        }
    }

    void spawnBullet(Vector3 pos) {
        GameObject bullet;
            if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer) {
                bullet = Instantiate(normal, pos, transform.rotation);
                if (bullet.GetComponent<NormalBullet>().dir.x != 0)
                    bullet.transform.localScale = transform.localScale;
                bullet.transform.GetComponent<NormalBullet>().updateTrue();
                bullet.transform.GetComponent<NormalBullet>().shooter = gameObject;
            }
            else if (isLocalPlayer){
                CmdSpawnBullet(pos, transform.rotation, transform.localScale, bulletName);
            }
            
    }

    [Command]
    void CmdSpawnBullet(Vector3 pos, Quaternion rot, Vector3 scale, string name) {
        //NetworkServer.Spawn(spawnBullet(pos, rot, scale, name));
        string assignedName = NetworkManage.uniq_id().ToString();
        RpcSpawnBullet(pos, rot, scale, name, assignedName);
    }

    [ClientRpc]
    void RpcSpawnBullet(Vector3 pos, Quaternion rot, Vector3 scale, string name, string assignedName) {
        spawnBullet(pos, rot, scale, bulletName, assignedName);
    }


    GameObject spawnBullet(Vector3 pos, Quaternion rot, Vector3 scale, string bulletName, string assignedName) {
        GameObject bullet = Instantiate(NetworkManager.singleton.spawnPrefabs.Find(prefab => prefab.name == bulletName), pos, rot);
        if (bullet.GetComponent<NormalBullet>().dir.x != 0)
            bullet.transform.localScale = scale;
        bullet.transform.GetComponent<NormalBullet>().updateTrue();
        bullet.transform.GetComponent<NormalBullet>().shooter = gameObject;
        bullet.name = assignedName;
        return bullet;
    }
}
