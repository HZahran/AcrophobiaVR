using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGenerator1 : MonoBehaviour {

    public Transform bridge;
    public Transform fps;
    public int holesFreq;
    private float initposZ;
    private float distanceZ = 0;
    private float totalDis = 3050;
    private float scaleX = 2;
    
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
       // Transform bridge = bridges[indx];
        Transform newObj = Instantiate(bridge, bridge.position, bridge.rotation);
        newObj.localScale = new Vector3(scaleX, newObj.localScale.y, newObj.localScale.z);
        newObj.Translate(new Vector3(0, 0, distanceZ));
        newObj.SetParent(this.gameObject.transform, false);

        distanceZ += (withHole ?  32 : 25.5f);
        if (scaleX > 0.5) scaleX -= 0.001f;
    }
}
