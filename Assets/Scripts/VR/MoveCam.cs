using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveCam : MonoBehaviour {

    public float speed;
    public AudioClip fall;
    public Transform startPoint, endPoint;
    private float currDistance = 0;
    private float maxDistanceDelta;
    private bool hasFallen = false;
    private bool reachedTarget = false;
    private Transform targetPoint;

    // Use this for initialization
    void Start () {
        targetPoint = endPoint;
        speed = 19;
    }

    // Update is called once per frame
    void Update () {

        float fear = GameManager.GetFear();
        fear = 0.5f;
        if (!reachedTarget)
        {
            if (!hasFallen)
            {
                maxDistanceDelta = Time.deltaTime * speed;
                transform.Rotate(0, 0, Mathf.Cos(Time.time * speed) / 10); // Running Effect
                speed = Mathf.Lerp(18, 23, fear); //
            }
            else
                GetComponent<CharacterMotor>().movement.gravity += 4f; // Fall faster

            // Move Player
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(targetPoint.position.x, 
                transform.position.y,
                targetPoint.position.z)
                , maxDistanceDelta);
            currDistance += maxDistanceDelta;
        }
        else
            currDistance = 0;
        
    }

    public void fallPlayer()
    {
        // Falling Animation
        Transform character = transform.GetChild(0);
        character.gameObject.GetComponent<Animator>().SetBool("fall", true);
        character.Rotate(15, 0, 0);
        character.localPosition = new Vector3(0.3f, -2.3f, -0.1f);
        character.Rotate(-39, 0, 0);

        // Falling Sound
        GetComponent<AudioSource>().clip = fall;
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().volume = 0.1f;
        GetComponent<AudioSource>().Play();

        // Stop Moving Forward
        maxDistanceDelta = 0;
        hasFallen = true;
    }

    public bool HasFallen()
    {
        return hasFallen;
    }
}
