using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCollider : MonoBehaviour {
    
    void OnTriggerEnter(Collider coll)
    {
        GameManager.EndGame(true);
    }
}
