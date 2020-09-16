using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDamage : NetworkBehaviour
{
    [SerializeField] private float damageMultiplier = 1;

    public void setDamageMultiplier(float dm) {
        damageMultiplier = dm;
    }

    // target should be a player
    // SHOULD BE USED FOR ALL DAMAGE CALCULATIONS  
    // REQUIRES damage > 0
    public void takenDamage(int damage, GameObject target) {
        if (damage <= 0) return;
        if (isServer)
            RpcDamage(damage, target);
        else if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
            normalDamage(damage, target);
        else if (hasAuthority)
            CmdDamage(damage, target);
    }

    // SHOULD BE USED FOR ALL HEALING CALCULATIONS
    public void heal(int healing, GameObject target) {
        if (healing <= 0) return;
        if (isServer)
            RpcHeal (healing, target);
        else
            CmdHeal(healing, target);
    }

    [Command]
    void CmdHeal(int healing, GameObject target) {
        RpcHeal(healing, target);
    }

    [ClientRpc]
    void RpcHeal(int healing, GameObject target) {
        target.GetComponent<PlayerHealth>().Heal(healing);
    }

    [Command]
    void CmdDamage(int damage, GameObject target) {
        RpcDamage(damage, target);
    }
    [ClientRpc]
    void RpcDamage(int damage, GameObject target) {
        int shield = target.GetComponent<PlayerHealth>().getShield();
        if (shield > 0) { 
            if (damage > shield) {
                damage -= shield;
                shield = 0;
            }
            else {
                shield -= damage;
                damage = 0;
            }
        }
        if (target.GetComponent<PlayerDamage>().damageMultiplier != 1)
            damage = (int) (target.GetComponent<PlayerDamage>().damageMultiplier * damage);
        Debug.Log(damage);
        target.GetComponent<PlayerHealth>().setShield(shield);
        target.GetComponent<PlayerHealth>().TakeDamage(damage);
    }

    private void normalDamage(int damage, GameObject target) {
        int shield = target.GetComponent<PlayerHealth>().getShield();
        if (shield > 0) { 
            if (damage > shield) {
                damage -= shield;
                shield = 0;
            }
            else {
                shield -= damage;
                damage = 0;
            }
        }
        if (damageMultiplier != 1)
            damage = (int) (damageMultiplier * damage);
        target.GetComponent<PlayerHealth>().setShield(shield);
        target.GetComponent<PlayerHealth>().TakeDamage(damage);
    }

    public void destroyObject(GameObject o) {
        if (gameObject.GetComponent<PlayerMovement>().isSinglePlayer)
            Destroy(o);
        else {
            if (hasAuthority)
                CmdDestroyObject(o.name, 0);
        }
    }

    public void networkDestroy(GameObject o) {
        NetworkServer.Destroy(o);
    }

    [Command]
    public void CmdDestroyGround(GameObject o) {
        RpcDestroyGround(o);
    }

    [ClientRpc]
    public void RpcDestroyGround(GameObject o) {
        Destroy(o);
    }

    [Command]
    public void CmdDestroyObject(string o, float d) {
        RpcDestroyObject(o, d);
    }

    [ClientRpc]
    public void RpcDestroyObject(string o, float d) {
        GameObject go = GameObject.Find(o);
        if (go != null)
            Destroy(go, d);
    }

    [Command]
    public void CmdKillBullet(string name) {
        RpcKillBullet(name);
    }

    [ClientRpc]
    public void RpcKillBullet(string name) {
        GameObject o = GameObject.Find(name);
        if (o != null && o.GetComponent<NormalBullet>() != null)
            o.GetComponent<NormalBullet>().destroySelf();
    }

    [Command]
    public void CmdDestroyObjectTime(GameObject o, float delay) {
        RpcDestroyObjectTime(o, delay);
    }

    [ClientRpc]
    public void RpcDestroyObjectTime(GameObject o, float delay) {
        Destroy(o, delay);
    }

    [Command]
    public void CmdUpdatePos(GameObject p, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale) {
        RpcSyncedPos(p, syncedPos, syncedRotation, syncedScale);
    }

    [ClientRpc]
    public void RpcSyncedPos(GameObject p, Vector3 syncedPos, Quaternion syncedRotation, Vector3 syncedScale)
    {
            if (p != null && (p.GetInstanceID() == gameObject.GetInstanceID() && !isLocalPlayer)) {
                p.transform.position = syncedPos;
                p.transform.rotation = syncedRotation;
                p.transform.localScale = syncedScale;
            }
    }

}
