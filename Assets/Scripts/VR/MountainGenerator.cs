using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGenerator : MonoBehaviour {

    public Transform mountain;
    private const float MOUNTAIN_DIST = 2910;
    private int count = 0;
    private Vector3 scaleInc = new Vector3(10f / 6f, 40f / 13f, 15f / 9f); // Scale Ratios
    private float currZ = 0;
    public Vector3 currTower, nextTower; // position of towers

    // Use this for initialization
    void Start () {
        addMountain();
    }
	
	// Update is called once per frame
	void Update () {
 
    }

    public void addMountain()
    {
        Transform newObj = Instantiate(mountain, new Vector3(mountain.position.x, mountain.position.y, mountain.position.z + currZ * count), mountain.rotation);
        newObj.SetParent(this.gameObject.transform, false);

        if (count != 0)
            newObj.localScale = Vector3.Scale(newObj.localScale, scaleInc);

        count++;
        currZ = newObj.GetChild(0).GetComponent<MeshRenderer>().bounds.size.z;

        currTower = nextTower;
        nextTower = newObj.GetChild(0).GetChild(0).position;

      //  GameObject.Find("Bridges").GetComponent<BridgeGenerator1>().isNewBridge = true;

    }
}
