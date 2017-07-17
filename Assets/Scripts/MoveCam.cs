using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveCam : MonoBehaviour {

    public float speed;
    public AudioClip fall;
    private float checkPoints;
    private float currDistance = 0;
    private float maxDistanceDelta;
    private bool hasFallen = false;
    private const float MOUNTAINS_DISTANCE = 3050;


    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        if (currDistance < MOUNTAINS_DISTANCE)
        {
            float rotX = 0;
            //Debug.Log(rotX);
            //speed = 20 * Mathf.Tan(rotX);
            //Debug.Log(speed);
            if (!isFallen())
            {
                maxDistanceDelta = Time.deltaTime * speed;
                transform.Rotate(0, 0, Mathf.Cos(Time.time * speed) / 15); // Running Effect
            }
            else if(!hasFallen)
            {
                GetComponentInChildren<FallingScript>().fall();
                GetComponent<AudioSource>().clip = fall;
                GetComponent<AudioSource>().loop = false;
                GetComponent<AudioSource>().volume = 0.1f;
                GetComponent<AudioSource>().Play();

                maxDistanceDelta = 0;
                hasFallen = true;
            }
            
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x,
                transform.position.y + 3050.0f * Mathf.Tan(rotX * -1),
                transform.position.z + 3050.0f), maxDistanceDelta);
            currDistance += maxDistanceDelta;
        }
        else
            currDistance = 0;
        
    }

    private bool isFallen()
    {
        return transform.position.y < GameObject.Find("Bridges").transform.position.y + 2;
    }
}
