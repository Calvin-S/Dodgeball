using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    // follows target if target is not null. If anything else, see cantMove
    public bool follow; 
    // cantMove is false if camera can be controlled by player input    
    public bool cantMove;
    public float smoothSpeed = 0.25f;
    private float playerControlSpeed = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        follow = true;
        cantMove = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (follow && target != null) {
            Vector3 desiredPosition = new Vector3 (target.transform.position.x, target.transform.position.y, transform.position.z);
            Vector3 smoothedPos = Vector3.Lerp (transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPos;
        }
        else if (!cantMove) {
            float moveHorizontal = Input.GetAxis ("Horizontal");
            float moveVertical = Input.GetAxis ("Vertical");
            Vector3 movement = playerControlSpeed * new Vector3 (moveHorizontal, moveVertical, 0);
            transform.Translate (movement * Time.deltaTime);
        }
    }
    

}
