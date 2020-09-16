using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RandomMovement : NetworkBehaviour
{   
    //This class should be attached to the SPRITE and its parent has the script NormalBullet
    public NormalBullet normalBullet;
    public float maxUpdateRotTime;
    public float minUpdateRotTime;
    public float maxRotation; // the amount of rotation updated each time updateRotTime has been passed
    public float minRotation;
    private float lastRotatedTime;
    private float updateRotTime;

    void Start() {
         lastRotatedTime = Time.time;
         updateRotTime = Random.Range(minUpdateRotTime, maxUpdateRotTime);
         if (normalBullet == null) {
            Debug.LogError("Script must have normalBullet assigned");
            Destroy(gameObject);
         }
         transform.rotation = Quaternion.identity;
    }

    void Update() {
        if (Time.time - lastRotatedTime >= updateRotTime) {
            if (Random.Range(0,2) == 0) {
                float tempR = Random.Range(minRotation, maxRotation);
                normalBullet.gameObject.transform.Rotate(0f,0f, tempR);
                if (normalBullet.gameObject.transform.localScale.x < 0)
                    transform.Rotate(0f,0f, tempR);
                else 
                    transform.Rotate(0f,0f, -tempR);
            }
            else {
                float tempR = Random.Range(minRotation, maxRotation);
                normalBullet.gameObject.transform.Rotate(0f,0f, -tempR);
                if (normalBullet.gameObject.transform.localScale.x < 0)
                    transform.Rotate(0f,0f, -tempR);
                else 
                    transform.Rotate(0f,0f, tempR);
            }
            normalBullet.networkUpdate();
            updateRotTime = Random.Range(minUpdateRotTime, maxUpdateRotTime);
            lastRotatedTime = Time.time;   
        }
        //if (normalBullet.shooter.GetComponent<PlayerMovement>() != null && normalBullet.shooter.GetComponent<PlayerMovement>().hasAuthority)
        //    normalBullet.shooter.GetComponent<PlayerDamage>()
    }

}
