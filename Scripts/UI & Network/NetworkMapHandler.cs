using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using UnityEngine.SceneManagement;

 

public class NetworkMapHandler : MonoBehaviour
{   
    [SerializeField] private List<string> mapsPVP = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getRandomPVPMap() {
        int r = Random.Range(0, mapsPVP.Count);
        return mapsPVP[r];
    }

}
