    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shipz : MonoBehaviour
{
    bool shipSelected = false;
    bool tileSelected = false;
    public float minimum = -50.0F;
    public float maximum = 50.0F;
    public Vector3 destinationTile;

    // starting value for the Lerp
    static float t = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Collided()
    {
        print("Enabled");
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
    public void Uncollided()
    {
        print("Disabled");
        gameObject.GetComponent<Renderer>().material.color = Color.white;
        
    }

    public void ColliderClicked()
    {
        shipSelected = true;
        print("Clicked correctlyShipp");
        gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
    }
    public void TileClicked()
    {
        tileSelected = true;
        print("Sending ship to tile");
    }
    // Update is called once per frame
    void Update()
    {
        if (shipSelected == true && tileSelected == true)
        {
            ShipGo();
        }
    }

    void ShipGo()
    {
        const float speed = 0.01f;

        print("Ship going");

        Vector3 start = transform.position;

        var next = new Vector3(
            Mathf.Lerp(start.x, destinationTile.x, t),
            Mathf.Lerp(start.y, destinationTile.y, t),
            Mathf.Lerp(start.z, destinationTile.z, t));

        transform.position = next;
        t += 0.01f * Time.deltaTime;

        if (next == destinationTile) {
            gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            shipSelected = false;
            tileSelected = false;

            // TODO: Trigger pick up or delivery of cargo

            // TODO: After dealing with cargo, begin journey back "home"
        }
    }
}
