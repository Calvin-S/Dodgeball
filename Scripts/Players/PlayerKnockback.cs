using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerKnockback : NetworkBehaviour
{
    private List<Vector2> forcesImpulse;
    private List<float> timesImpulse;
    private List<Vector2> forcesForce;
    private List<float> timesForce;
    private void Start() {
        forcesImpulse = new List<Vector2>();
        timesImpulse = new List<float>();
        forcesForce = new List<Vector2>();
        timesForce = new List<float>();
    }
    public void addImpulseForce(Vector2 newF, float duration) {
        forcesImpulse.Add(newF);
        timesImpulse.Add(Time.time + duration);
    }

    public void addForce(Vector2 newF, float duration) {
        forcesForce.Add(newF);
        timesForce.Add(Time.time + duration);
    }

    public Vector2 calculateImpulse() {
        Vector2 totalF = new Vector2 (0,0);
        for (int i = 0; i < forcesImpulse.Count; i++) {
            if (Time.time > timesImpulse[i]) {
                forcesImpulse.RemoveAt(i);
                timesImpulse.RemoveAt(i);
                i--;
            }
            else {
                totalF += forcesImpulse[i];
            }
        }
        return totalF;
    }

    public Vector2 calculateForce() {
        Vector2 totalF = new Vector2 (0,0);
        for (int i = 0; i < forcesForce.Count; i++) {
            if (Time.time > timesForce[i]) {
                forcesForce.RemoveAt(i);
                timesForce.RemoveAt(i);
                i--;
            }
            else {
                totalF += forcesForce[i];
            }
        }
        return totalF;
    }
}
