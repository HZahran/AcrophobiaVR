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

    // Use this for initialization
    void Start () {
        targetPoint = endPoint;
        GetComponent<AudioSource>().clip = runClip;
       // speed = 19;
    }

    // Update is called once per frame
    void Update () {

        if (GameManager.GameEnded())
        {
            hasFallen = false;
            GetComponent<CharacterMotor>().movement.gravity = 50f;
            Transform character = transform.GetChild(0);
            character.gameObject.GetComponent<Animator>().SetBool("fall", false);

        }

        if (GameManager.GameStarted()) {

            // Running Sound
            if (GetComponent<AudioSource>().clip == null) {
                GetComponent<AudioSource>().clip = runClip;
                GetComponent<AudioSource>().loop = true;
                GetComponent<AudioSource>().volume = 0.5f;
                GetComponent<AudioSource>().Play();
            }

            float fear = GameManager.GetFear();
            fear = 0f;
            if (!reachedTarget)
            {
                if (!hasFallen)
                {
                    maxDistanceDelta = Time.deltaTime * speed;
                    transform.Rotate(0, 0, Mathf.Cos(Time.time * speed) / 10); // Running Effect
                    speed = Mathf.Lerp(22, 26, fear); //Normal to Max Speed 
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

            // Surpassed the target
            if (currDistance > endPoint.position.z - startPoint.position.z)
                fallPlayer();
        }
        
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
