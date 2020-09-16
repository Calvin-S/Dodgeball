using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Abilities : NetworkBehaviour
{
    public GameObject layover;
    private GameObject spawnedLayover;
    public Transform layoverTransform;

    [SerializeField] protected float cooldown;
    [SerializeField] protected bool onCooldown = false;

    protected KeyCode abilityKey = KeyCode.None;
    public string animTrigger = "";
    public string animBool = "";
    private Animator animator;
    protected bool isSinglePlayer;
    public int special; // should be 1 or 2 or 3, if not, then will not instantiate PhaseKey 

    public float getCooldown() {return cooldown;}
    public void setCooldown(float c) { cooldown = c;}
    public KeyCode getAbilityKey() {return abilityKey;}
    public bool getOnCooldown() {return onCooldown;}
    // Start is called before the first frame update
    private void Start() {
        animator = GetComponent<Animator>();
        if (gameObject.GetComponent<PlayerMovement>() != null)
            isSinglePlayer = gameObject.GetComponent<PlayerMovement>().isSinglePlayer;
    }

    protected void setKey()
    {
        if (special == 1)
            abilityKey = gameObject.GetComponent<PlayerMovement>().special1;
        else if (special == 2)
            abilityKey = gameObject.GetComponent<PlayerMovement>().special2;
        else if (special == 3)
            abilityKey = gameObject.GetComponent<PlayerMovement>().special3;
        else if (special == 4)
            abilityKey = gameObject.GetComponent<PlayerMovement>().normalShot;
    }

    protected void animateTrigger() {
        if (animTrigger != "") {
            animator.SetTrigger(animTrigger);
            gameObject.GetComponent<NetworkAnimator>().SetTrigger(animTrigger);
        }
    }

    protected void spawnLayover() {
        Debug.Log("should spawn?");
        if (spawnedLayover != null)
            Destroy(spawnedLayover);
        if (layover != null) {
            CmdSpawnLayover(layoverTransform.position, layoverTransform.rotation, transform);
        }
        Debug.Log(spawnedLayover);
    }

    [Command]
    public void CmdSpawnLayover(Vector3 pos, Quaternion rot, Transform parent) {
        RpcSpawnLayover(pos, rot, parent);
    }

    [ClientRpc]
    public void RpcSpawnLayover(Vector3 pos, Quaternion rot, Transform parent) {
        spawnedLayover = Instantiate(layover, layoverTransform.position, layoverTransform.rotation, gameObject.transform);
    }

    [Command]
    public void CmdDeleteLayover() {
        RpcDeleteLayover();
    }
    
    [ClientRpc]
    public void RpcDeleteLayover() {
        if (spawnedLayover != null)
            Destroy(spawnedLayover);
    }
    public void deleteLayover() {
        CmdDeleteLayover();
    }
}
