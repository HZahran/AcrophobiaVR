using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleCollider : MonoBehaviour {

    void OnTriggerEnter(Collider coll) {
        GameManager.EndGame(false);
    }
}
