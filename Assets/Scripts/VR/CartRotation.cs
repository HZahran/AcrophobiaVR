using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartRotation : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        transform.rotation = new Quaternion(transform.rotation.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
	}
}
