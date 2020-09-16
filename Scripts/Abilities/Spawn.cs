using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spawn : Abilities
{
    public GameObject toSpawn;
    private float lastTimeSpawn;
    public Transform[] spawnLocation = new Transform[0];
    public bool setAsChild = false;
    public bool usePlayerScale = true;
    public GameObject owner;
    private Animator anim;
    public float animBoolTimer = 0;
    [Header("Normal Bullet Dir")]
    [SerializeField] private Vector2 dir = new Vector2 (0,0); // only assign if spawned object uses NormalBullet script
    
    [Header("Spawning a Spawner")]
    public bool spawnsSpawnerCorner = false;
    public GameObject topRightCorner;
    public GameObject topLeftCorner;

    [Header("Passive Spawner")]
    public bool passiveSpawner = false;
    public bool useRandomTime = false;
    [SerializeField] private float randomSpawningTimeMin = 0;
    [SerializeField] private float randomSpawningTimeMax = 0;

    [Header("Affecting Weather")]
    public bool causeRain = false;
    public bool causeSnow = false;
    [SerializeField] private float changeWeatherChance = 0; // displayed in decimals
    // Start is called before the first frame update
    void Start()
    {  
        topRightCorner = GameObject.Find("TopRightCorner");
        topLeftCorner = GameObject.Find("TopLeftCorner");
        base.setKey();
        lastTimeSpawn = Time.time; 
        anim = gameObject.GetComponent<Animator>();
        if (toSpawn.GetComponent<Melee>() != null)
            toSpawn.GetComponent<Melee>().animBool = animBool;
        if (useRandomTime && passiveSpawner)
            cooldown = Random.Range(randomSpawningTimeMin, randomSpawningTimeMax);
    }

    // Update is called once per frame
    void Update()
    {
        isSinglePlayer = getIsSinglePlayer();
        // for players
        if (gameObject.GetComponent<PlayerMovement>() != null) {
            if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer || (!gameObject.GetComponent<PlayerMovement>().isSinglePlayer && isLocalPlayer)) {
                if (passiveSpawner || abilityKey == KeyCode.None) 
                    SpawnPassive(true);
                else 
                    SpawnAbility();
            }
        }
        // for spawned objects like Quetzalcoatl
        else {
            if (passiveSpawner && owner.GetComponent<PlayerMovement>().isLocalPlayer)
                SpawnPassive(false);
        }
    }

    void SpawnAbility() {
        onCooldown = Time.time - lastTimeSpawn < cooldown;
        if (animBoolTimer != 0 && Time.time - lastTimeSpawn >= animBoolTimer && animBool != "")
            anim.SetBool(animBool, false);            
        if (Time.time - lastTimeSpawn >= cooldown && Input.GetKey(abilityKey)) {
            if (changeWeatherChance >= Random.Range(0f,1f))
                if (gameObject.GetComponent<PlayerAbilities>() != null) {
                    string w = causeRain ? "rain" : causeSnow ? "snow" : "clear";
                    gameObject.GetComponent<PlayerAbilities>().CmdChangeWeather(w);
                }
            lastTimeSpawn = Time.time;
            if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer) {
                foreach (Transform t in spawnLocation) {
                    if (usePlayerScale)
                        Spawning(t.position, t.rotation, transform.localScale, "");
                    else
                        Spawning(t.position, t.rotation, t.localScale, "");
                }
            }
            else if (isLocalPlayer) {
                if (animTrigger != "")
                    gameObject.GetComponent<NetworkAnimator>().SetTrigger(animTrigger);
                
                Quaternion rot;
                foreach (Transform t in spawnLocation) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(t.rotation) : t.rotation;              
                    if (usePlayerScale)
                        CmdSpawn(t.position, rot, transform.localScale, toSpawn.name);
                    else 
                        CmdSpawn(t.position, rot, t.localScale, toSpawn.name);
                }
                if (spawnsSpawnerCorner && transform.localScale.x > 0) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(topLeftCorner.transform.rotation) : topLeftCorner.transform.rotation;
                    CmdSpawn(topLeftCorner.transform.position, rot, transform.localScale, toSpawn.name);
                }
                else if (spawnsSpawnerCorner && transform.localScale.x < 0) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(topRightCorner.transform.rotation) : topRightCorner.transform.rotation;
                    CmdSpawn(topRightCorner.transform.position, rot, transform.localScale, toSpawn.name);
                }
            }

            if (animTrigger != "") 
                anim.SetTrigger(animTrigger);
            else if (animBool != "") 
                anim.SetBool(animBool, true);
        }
    }

    public bool getIsSinglePlayer() {
        if (gameObject.GetComponent<PlayerMovement>() != null)
            return gameObject.GetComponent<PlayerMovement>().isSinglePlayer;
        return owner.GetComponent<Spawn>().getIsSinglePlayer();
    }

    void SpawnPassive(bool usingPlayer) {
        onCooldown = Time.time - lastTimeSpawn < cooldown;
        if (animBoolTimer != 0 && Time.time - lastTimeSpawn >= animBoolTimer && animBool != "")
            anim.SetBool(animBool, false);
        if (Time.time - lastTimeSpawn >= cooldown) {
            if (changeWeatherChance >= Random.Range(0f,1f))
                if (gameObject.GetComponent<PlayerAbilities>() != null) {
                    string w = causeRain ? "rain" : causeSnow ? "snow" : "clear";
                    gameObject.GetComponent<PlayerAbilities>().CmdChangeWeather(w);
                }

            lastTimeSpawn = Time.time;
            cooldown = useRandomTime ? Random.Range(randomSpawningTimeMin, randomSpawningTimeMax) : cooldown; 
            if (isSinglePlayer) {
                foreach (Transform t in spawnLocation) {
                    if (usePlayerScale)
                        Spawning(t.position, t.rotation, transform.localScale, "");
                    else
                        Spawning(t.position, t.rotation, t.localScale, "");
                }
            }
            else if (!usingPlayer) {
                if (animTrigger != "")
                    gameObject.GetComponent<NetworkAnimator>().SetTrigger(animTrigger);
                    Quaternion rot;
                GameObject o = owner == null ? gameObject : owner;
                foreach (Transform t in spawnLocation) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(t.rotation) : t.rotation;
                    if (usePlayerScale)
                        o.GetComponent<Spawn>().CmdSpawn(t.position, rot, transform.localScale, toSpawn.name);
                    else 
                        o.GetComponent<Spawn>().CmdSpawn(t.position, rot, t.localScale, toSpawn.name);
                }
                if (spawnsSpawnerCorner && transform.localScale.x > 0) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(topLeftCorner.transform.rotation) : topLeftCorner.transform.rotation;
                    o.GetComponent<Spawn>().CmdSpawn(topLeftCorner.transform.position, rot, transform.localScale, toSpawn.name);
                }
                else if (spawnsSpawnerCorner && transform.localScale.x < 0) {
                    rot = !usePlayerScale && transform.localScale.x < 0 ? Quaternion.Inverse(topRightCorner.transform.rotation) : topRightCorner.transform.rotation;
                    o.GetComponent<Spawn>().CmdSpawn(topRightCorner.transform.position, rot, transform.localScale, toSpawn.name);
                }
            }

            if (animTrigger != "") 
                anim.SetTrigger(animTrigger);
            else if (animBool != "") 
                anim.SetBool(animBool, true);
        }
    }

    GameObject Spawning(Vector3 pos, Quaternion rot, Vector3 scale, string toSpawnName, string assignedName = "") {
        GameObject spawned;
        if (toSpawnName == "")
            spawned = Instantiate(toSpawn, pos, rot);
        else
            spawned = Instantiate(NetworkManager.singleton.spawnPrefabs.Find(prefab => prefab.name == toSpawnName), pos, rot);  
        
        GameObject master = owner == null ? gameObject : owner;

        if (assignedName != "")
            spawned.name = assignedName;
        if (spawned.GetComponent<EnemyFollow>() != null)
            spawned.GetComponent<EnemyFollow>().owner = master;
        if (spawned.GetComponent<Melee>() != null)
            spawned.GetComponent<Melee>().owner = master;
        if (spawned.GetComponent<NormalBullet>() != null) {
            spawned.GetComponent<NormalBullet>().shooter = master;
            spawned.GetComponent<NormalBullet>().dir = dir;
            owner = spawned.GetComponent<NormalBullet>().shooter;
            spawned.GetComponent<NormalBullet>().isSinglePlayer = isSinglePlayer;
        }
        if (spawned.GetComponent<RandomEffect>() != null) {
            spawned.GetComponent<RandomEffect>().owner = master;
            spawned.GetComponent<RandomEffect>().setPlayerDamage();
        }

        if (spawned.GetComponent<Spawn>() != null) {
            spawned.GetComponent<Spawn>().owner = master;
            if (spawned.GetComponent<NormalBullet>() != null && spawnsSpawnerCorner)
                spawned.GetComponent<NormalBullet>().setTimeHealth(1.1f * Vector3.Distance(topLeftCorner.transform.position, topRightCorner.transform.position) / spawned.GetComponent<NormalBullet>().getSpeed());
                Debug.Log(1.1f * Vector3.Distance(topLeftCorner.transform.position, topRightCorner.transform.position) / spawned.GetComponent<NormalBullet>().getSpeed());
        }

        if (spawned.GetComponent<PolygonCollider2D>() != null) {
            spawned.GetComponent<PolygonCollider2D>().enabled = false;
            spawned.GetComponent<PolygonCollider2D>().enabled = true;
        }
        else if (spawned.GetComponent<BoxCollider2D>() != null) {
            spawned.GetComponent<BoxCollider2D>().enabled = false;
            spawned.GetComponent<BoxCollider2D>().enabled = true;
        }

        if (setAsChild)
            spawned.transform.parent = gameObject.transform;
        else
            spawned.transform.localScale = scale;
        Debug.Log(scale);
        return spawned;
    }

    [Command]
    void CmdSpawn(Vector3 pos, Quaternion rot, Vector3 scale, string toSpawnName) {
        string assignedName = NetworkManage.uniq_id().ToString();
        RpcSpawn(pos, rot, scale, toSpawnName, assignedName);
    }

    [ClientRpc] 
    void RpcSpawn(Vector3 pos, Quaternion rot, Vector3 scale, string toSpawnName, string assignedName) {
        Debug.Log("spawning " + toSpawnName);
        Spawning(pos, rot, scale, toSpawnName, assignedName);
    }
}
