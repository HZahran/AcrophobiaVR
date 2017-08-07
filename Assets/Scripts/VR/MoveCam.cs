using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveCam : MonoBehaviour {

    public float speed;
    public AudioClip runClip, fallClip;
    public Transform startPoint, endPoint;
    private float currDistance = 0;
    private float maxDistanceDelta;
    private bool hasFallen = false;
    private bool reachedTarget = false;
    private Transform targetPoint;
    private Quaternion initRot;

    // Use this for initialization
    void Start () {
        targetPoint = endPoint;
        GetComponent<AudioSource>().clip = null;
        //Move Cart Forward
    }

    // Update is called once per frame
    void Update () {

        if (GameManager.GameEnded())
        {
            hasFallen = false;
            GetComponent<CharacterMotor>().movement.gravity = 50f;
            transform.localRotation = initRot;
            //Transform character = transform.GetChild(0);
            //character.gameObject.GetComponent<Animator>().SetBool("fall", false);

        }

        if (GameManager.GameStarted()) {

            // Running Sound
            if (GetComponent<AudioSource>().clip == null) {
                GetComponent<AudioSource>().clip = runClip;
                GetComponent<AudioSource>().loop = true;
                GetComponent<AudioSource>().volume = 0.2f;
                GetComponent<AudioSource>().Play();
            }

            float fear = Mathf.Max(0, 1 - GameManager.GetFear());
            if (!reachedTarget)
            {
                if (!hasFallen)
                {
                    maxDistanceDelta = Time.deltaTime * speed;
                    transform.Rotate(0, 0, Mathf.Cos(Time.time * speed) / 10); // Running Effect
                    speed = Mathf.Lerp(17, 20, fear); //Speed decreases with increasing fear
                }
                else
                {
                    transform.Rotate(Time.deltaTime * 20, 0, 0); // Rotate Cart while falling
                    GetComponent<CharacterMotor>().movement.gravity += 4f; // Fall faster
                }

                // Move Player
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(targetPoint.position.x,
                    transform.position.y,
                    targetPoint.position.z)
                    , maxDistanceDelta);
                currDistance += maxDistanceDelta;
            }

            // Surpassed the target
            if (currDistance > endPoint.position.z - startPoint.position.z)
                fallPlayer();
        }
        
    }

    public void fallPlayer()
    {
        // Falling Sound
        GetComponent<AudioSource>().clip = fallClip;
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().volume = 0.05f;
        GetComponent<AudioSource>().Play();


        // Stop Moving Forward
        maxDistanceDelta = 0;
        currDistance = 0;
        hasFallen = true;

    }

    public bool HasFallen()
    {
        return hasFallen;
    }
}
