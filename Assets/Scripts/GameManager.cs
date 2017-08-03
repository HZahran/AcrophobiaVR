using Kino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private static GameManager gm;
    private static TextMesh time,fear;
    private static float fearPerc = 100;

    private static Transform canvas;
    private static TextMesh screenText;
    private float transition = 0;
    private static bool gameEnded, gameStarted;
    private static int gameStartTime = 0;
    private static int currTime = 0;

    public Transform playerArea;
    public Material skybox, noSkybox;
    // Use this for initialization
    void Start () {
        gm = this;
        time = transform.GetChild(1).GetComponent<TextMesh>();
        fear = transform.GetChild(2).GetComponent<TextMesh>();
        canvas = GameObject.Find("First Person Controller").transform.GetChild(1).GetChild(2).GetChild(2);
        screenText = canvas.GetChild(0).GetComponent<TextMesh>();
        StartGame();
    }

    // Update is called once per frame
    void Update () {
        currTime = (int)Time.time - gameStartTime;
        time.text = "Time " + currTime + "s";
        fear.text = "Fear " + fearPerc + "%";

        if (gameEnded) {
            //if (transition < 1) // Fadeout Anim
            //{
            //    transition += 0.001f;
            //    vignette.intensity = Mathf.Lerp(vignette.intensity, 10.0f, transition);
            //}
        }
    }

    public static void UpdateFear(float fearPercentage)
    {
        fearPerc = fearPercentage;
    }
    public static float GetFear()
    {
        return fearPerc;
    }

    public static bool GameEnded()
    {
        return gameEnded;
    }
    public void StartGame()
    {
        gameStarted = true;
        changeVisibility(true);
        RenderSettings.skybox = gm.skybox;
        gameStartTime = (int)Time.time;
    }

    public static void EndGame()
    {
        Debug.Log("Game Over");
        gameEnded = true;
        changeVisibility(false);
        RenderSettings.skybox = gm.noSkybox;

        // Show Final Time
        screenText.text = "Game over\nPress To Retry\nTime : " + currTime +"s";
    }


    private static void changeVisibility(bool vis)
    {
        GameObject.Find("Mountains").SetActive(vis);
        time.gameObject.SetActive(vis);
        fear.gameObject.SetActive(vis);

        //Show Canvas when others are hidden and vice-versa
        canvas.gameObject.SetActive(!vis);
    }
}
