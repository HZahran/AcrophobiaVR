using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGenerator : MonoBehaviour {

    public Transform mountain;
    private const float MOUNTAIN_DIST = 2910;
    private int count = 0;
    // Use this for initialization
    void Start () {
        addMountain();
    }
	
	// Update is called once per frame
	void Update () {
 
    }

    public void addMountain()
    {
        Transform newObj = Instantiate(mountain, new Vector3(0, 0, MOUNTAIN_DIST * (count++)), mountain.rotation);
        newObj.SetParent(this.gameObject.transform, false);
    }
}
