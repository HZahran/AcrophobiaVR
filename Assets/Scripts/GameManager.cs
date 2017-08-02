using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private TextMesh time,fear;
    private static float fearPerc = 100;
	// Use this for initialization
	void Start () {
        time = transform.GetChild(1).GetComponent<TextMesh>();
        fear = transform.GetChild(2).GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update () {
        time.text = "Time " + (int)Time.time + "s";
        fear.text = "Fear " + fearPerc + "%";
    }

    public static void updateFear(float fearPercentage)
    {
        fearPerc = fearPercentage;
    }
    public static float getFear()
    {
        return fearPerc;
    }
}
