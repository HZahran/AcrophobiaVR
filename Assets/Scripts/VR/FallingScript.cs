using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void fall()
    {
        GetComponent<Animator>().SetBool("fall", true);
        transform.Rotate(-10, 0, 0);
        transform.Translate(0, 0.8f, -1);
    }
}
