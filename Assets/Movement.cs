using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 212.0f;
    private float pitch = 21.0f;

    public int moving = 0;

    // Use this for initialization
    void Start () {
        //Cursor.lockState = CursorLockMode.Locked;
    }
	
	// Update is called once per frame
	void Update () {

        //yaw += speedH * Input.GetAxis("Mouse X");
        //pitch -= speedV * Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.RightArrow))
            yaw += speedH;
        if (Input.GetKey(KeyCode.LeftArrow))
            yaw -= speedH;

        if (Input.GetKey(KeyCode.UpArrow))
            pitch -= speedV;
        if (Input.GetKey(KeyCode.DownArrow))
            pitch += speedV;

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");
        if (Camera.current != null && moving == 0)
        {
            Camera.current.transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));
        }
    }
}
