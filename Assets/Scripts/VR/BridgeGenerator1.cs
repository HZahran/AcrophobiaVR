using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGenerator1 : MonoBehaviour {

    public Transform bridge;
    public Transform fps;
    public int holesFreq;
    private float initposZ;
    private float distanceY, distanceZ;
    private float totalDis = 3050;
    private float scaleX = 1f;
    private bool enableCollider = false; // Enable Hole Collider

    // Use this for initialization
    void Start () {
        initposZ = fps.position.z;

        //Instantiate first 5 bridges
        for (int i = 0; i < 5; i++)
            addBridge(false);
    }

    // Update is called once per frame
    void Update () {

        if ((fps.position.z - initposZ > distanceZ / 2) && (distanceZ < totalDis))
            addBridge(Math.Min(1, UnityEngine.Random.Range(0, holesFreq)) == 1); // Increase probability of holes
    }

    private void addBridge(bool withHole){

        Transform newObj = Instantiate(bridge, bridge.position, bridge.rotation);
        newObj.SetParent(this.gameObject.transform, false);
        newObj.localScale = new Vector3(scaleX, newObj.localScale.y, newObj.localScale.z);
        newObj.Rotate(90, 0, 0);
        newObj.Translate(new Vector3(0, distanceY, distanceZ));
        newObj.Rotate(-90, 0, 0);

        distanceZ += (withHole ?  45 : 10);
        distanceY -= (withHole ?  15 : 0);
        if (scaleX > 0.3) scaleX -= 0.005f;

        if (enableCollider)
        {
            newObj.GetComponent<BoxCollider>().enabled = true;
            enableCollider = false;
        }
        if (withHole)
            enableCollider = true;
    }
}
