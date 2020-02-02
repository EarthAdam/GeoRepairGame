using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shipz : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Collided()
    {
        print("Enabled");
        gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
    }
    public void Uncollided()
    {
        print("Disabled");
        gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
    }

    public void ColliderClicked()
    {
        print("Clicked correctlyShipp");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
