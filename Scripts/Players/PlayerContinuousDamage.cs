using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerContinuousDamage : NetworkBehaviour
{   
    /*This class should be used for things like poison or fire damage */

    // Vector3.x is DAMAGE, Vector3.y is DAMAGE AGAIN AFTER HOW y SECONDS, Vector3.z is DURATION OF DPS
    private List<Vector3> continuousDamage; 
    private List<float> lastTimeDamaged;
    // Start is called before the first frame update
    void Start()
    {
        continuousDamage = new List<Vector3>();
        lastTimeDamaged = new List<float>();
    }

    // Update is called once per frame
    void LateUpdate() {
        for (int i = 0; i < continuousDamage.Count; i++) {
            if (Time.time >= continuousDamage[i].z) {
                continuousDamage.RemoveAt(i);
                lastTimeDamaged.RemoveAt(i);
                i--;
            }
            else if (Time.time - lastTimeDamaged[i] >= continuousDamage[i].y) {
                gameObject.GetComponent<PlayerDamage>().takenDamage((int) continuousDamage[i].x, gameObject);
                lastTimeDamaged[i] = Time.time;
            }
        }
    }

    public void addDamage(int damage, float damageAgainTime, float duration) {
        continuousDamage.Add(new Vector3 (damage, damageAgainTime,Time.time + duration));
        lastTimeDamaged.Add(Time.time - damageAgainTime);
    }

    [ClientRpc]
    public void RpcAddDamage(int damage, float damageAgainTime, float duration, GameObject target) {
        target.GetComponent<PlayerContinuousDamage>().continuousDamage.Add(new Vector3 (damage, damageAgainTime,Time.time + duration));
        target.GetComponent<PlayerContinuousDamage>().lastTimeDamaged.Add(Time.time - damageAgainTime);
    }
    
    [Command] 
    public void CmdAddDamage(int damage, float damageAgainTime, float duration, GameObject target) {
        RpcAddDamage(damage, damageAgainTime, duration, target);
    }
}
