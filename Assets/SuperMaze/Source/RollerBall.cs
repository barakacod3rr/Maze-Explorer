using UnityEngine;
using System.Collections;

public class RollerBall : MonoBehaviour {

    private Vector3 offset;

    // Use this for initialization
    void Start()
    {
        offset = transform.position - Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float torqueX = 0;
        float torqueZ = 0;
        float t = 15;
        if (Input.GetKey("left"))
        {
            torqueZ += t;
        }
        if (Input.GetKey("right"))
        {
            torqueZ -= t;
        }
        if (Input.GetKey("down"))
        {
            torqueX -= t;
        }
        if (Input.GetKey("up"))
        {
            torqueX += t;
        }
        // apply torque to player
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        rb.AddTorque(new Vector3(torqueX, 0, torqueZ));
        // position camera
        Camera.main.transform.position = transform.position - offset;
        // reload level
        if(Input.GetKey("space"))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}

