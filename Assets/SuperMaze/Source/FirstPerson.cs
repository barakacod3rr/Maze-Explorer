using UnityEngine;
using System.Collections;

public class FirstPerson : MonoBehaviour {

    // movement speed
    public float speed = 0.2f;

    // store camera angles
    private float ay = 0;
    private float ax = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        // update camera angles
        float s = 3;
        ay += s * Input.GetAxis("Mouse X");
        ax -= s * Input.GetAxis("Mouse Y");
        ax = Mathf.Clamp(ax, -80.0f, 80.0f);
        Camera.main.transform.rotation = Quaternion.Euler(ax, ay, 0);
        // position camera
        Camera.main.transform.position = transform.position;
        // get camera directions
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();
        // move
        Vector3 movement = new Vector3(0,0,0);
        if(Input.GetKey("w"))
        {
            movement += forward;
        }
        if (Input.GetKey("s"))
        {
            movement -= forward;
        }
        if (Input.GetKey("a"))
        {
            movement -= right;
        }
        if (Input.GetKey("d"))
        {
            movement += right;
        }
        GetComponent<Rigidbody>().MovePosition(transform.position + movement * speed * Time.deltaTime);
        // reload level
        if (Input.GetKey("space"))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}
