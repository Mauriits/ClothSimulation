using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAndMove : MonoBehaviour {

    bool moving = false;

    float xAxisValue, yAxisValue, zAxisValue;
    float speed = 0.1f;
    Color matcol;
    GameObject cloth;
    int lockedClothCorner = -1;

    // Use this for initialization
    void Start () {
        matcol = gameObject.GetComponent<Renderer>().material.color;
        cloth = GameObject.Find("Cloth");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == name && Input.GetMouseButtonDown(0))
                {
                    Camera.main.GetComponent<Movement>().moving += moving ? -1 : 1;
                    //for (int i = 0; i < cubes.Length; i++)
                    //{
                    //    cubes[i].GetComponent<Renderer>().material.color = matcol;
                    //    cubes[i].GetComponent<ClickAndMove>().moving = false;
                    //}

                    moving = !moving;

                    gameObject.GetComponent<Renderer>().material.color = moving ? Color.green : matcol;
                }
                if (hit.transform.name == name && Input.GetMouseButtonDown(1))
                {
                    if (Input.GetKey(KeyCode.Alpha1))
                    {
                        lockedClothCorner = 0;
                        cloth.GetComponent<SimulateCloth>().lockedPositions[0] = transform.position;
                        cloth.GetComponent<SimulateCloth>().isPositionLocked[0] = true;
                    }

                    if (Input.GetKey(KeyCode.Alpha2))
                    {
                        lockedClothCorner = 1;
                        cloth.GetComponent<SimulateCloth>().lockedPositions[1] = transform.position;
                        cloth.GetComponent<SimulateCloth>().isPositionLocked[1] = true;
                    }

                    if (Input.GetKey(KeyCode.Alpha3))
                    {
                        lockedClothCorner = 2;
                        cloth.GetComponent<SimulateCloth>().lockedPositions[2] = transform.position;
                        cloth.GetComponent<SimulateCloth>().isPositionLocked[2] = true;
                    }

                    if (Input.GetKey(KeyCode.Alpha4))
                    {
                        lockedClothCorner = 3;
                        cloth.GetComponent<SimulateCloth>().lockedPositions[3] = transform.position;
                        cloth.GetComponent<SimulateCloth>().isPositionLocked[3] = true;
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.Escape) && moving)
        {
            Camera.main.GetComponent<Movement>().moving -= 1;
            gameObject.GetComponent<Renderer>().material.color = matcol;
            moving = false;
        }

        if (moving)
        {
            if (Input.GetKey(KeyCode.A))
                xAxisValue = speed;
            if (Input.GetKey(KeyCode.D))
                xAxisValue = -speed;

            if (Input.GetKey(KeyCode.W))
                zAxisValue = -speed;
            if (Input.GetKey(KeyCode.S))
                zAxisValue = speed;

            if (Input.GetKey(KeyCode.E))
                yAxisValue = speed;
            if (Input.GetKey(KeyCode.Q))
                yAxisValue = -speed;

            transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            zAxisValue = 0.0f;
            yAxisValue = 0.0f;
            xAxisValue = 0.0f;

            if (lockedClothCorner != -1)
                cloth.GetComponent<SimulateCloth>().lockedPositions[lockedClothCorner] = transform.position;
        }
    }
}
