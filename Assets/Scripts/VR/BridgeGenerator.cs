using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum Lane { Left, Middle, Right };
public class BridgeGenerator : MonoBehaviour {

    public Transform bridge;
    public int repetitions;
    private float distanceZ = 0;
    private Lane lane = Lane.Middle;
    private float scaleX = 2;

    // Use this for initialization
    void Start () {
        System.Random rand = new System.Random();

        while (repetitions-- > 0)
        {
            Transform newObj = Instantiate(bridge, new Vector3(bridge.position.x + (lane == Lane.Left? -10:(lane == Lane.Right? 10 : 0)),
                bridge.position.y, distanceZ), bridge.rotation);
            newObj.localScale = new Vector3(scaleX, 1, 1);
            newObj.SetParent(this.gameObject.transform, false);
            if (repetitions % 18 == 0)
            {
                
                if (lane == Lane.Left || lane == Lane.Right)
                    lane = Lane.Middle;
                else 
                    lane = rand.Next(0, 2) == 1 ? Lane.Right : Lane.Left;
                distanceZ += 10;
            }
            else distanceZ += 5.5f;
            if (scaleX > 0.5) scaleX -= 0.001f;
        }
    }

    // Update is called once per frame
    void Update () {
    
    }
}
