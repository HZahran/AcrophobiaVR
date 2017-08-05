using Kino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private static GameManager gm;
    private TextMesh time,fear;
    private static float fearPerc = 100;

    public Transform canvas;
    private TextMesh screenText;
    private static bool gameEnded, gameStarted;
    private static int gameStartTime = 0;
    private static int currTime = 0;
    private static float timerIntro = 10;

    public Transform playerArea;
    public Transform fps;
    public Material skybox, noSkybox;

    private Vector3 initPos;
    private Quaternion initRot;

    private Transform mountains;

    // Use this for initialization
    void Start () {
        gm = this;
        time = transform.GetChild(1).GetComponent<TextMesh>();
        fear = transform.GetChild(2).GetComponent<TextMesh>();
        gm.screenText = gm.canvas.GetChild(0).GetComponent<TextMesh>();
        initPos = gm.fps.position;
        initRot = gm.fps.rotation;
        mountains = GameObject.Find("Mountains").transform;
        StartIntro();

    }

    // Update is called once per frame
    void Update () {

        // Intro or Restart time
        if (timerIntro > 0)
        {
            if (timerIntro <= 10)
                gm.screenText.text = (gameEnded ? "Game Restarts in " : "Explore The World Around You\nGame Starts in ") + (int)timerIntro + "s";
            timerIntro -= Time.deltaTime;
        }
        else if (!gameStarted)
        {
            ResetGame();
            StartGame();
        }
        
        if (gameStarted)
        {
            currTime = ((int)Time.time) - gameStartTime;
            time.text = "Time " + currTime + "s";
            fear.text = "Fear " + (int)(fearPerc * 100 )+ "%";
        }

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

    public static bool GameStarted()
    {
        return gameStarted;
    }

    public static bool GameEnded()
    {
        return gameEnded;
    }

    public void StartIntro()
    {
        ResetGame();
        gm.canvas.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        gameStarted = true;
        gameEnded = false;
        gm.canvas.gameObject.SetActive(false);
        gameStartTime = (int)Time.time;
        // Start 
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game end", false);
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game start", true);
    }

    public static void EndGame(bool wonGame)
    {
        Debug.Log("Game Over");
        gameStarted = false;
        gameEnded = true;
        changeVisibility(false);
        RenderSettings.skybox = gm.noSkybox;

        // Show Final Time
        gm.screenText.text = (wonGame? "You Did It !!":"Game over") + "\nTime : " + currTime +"s";
        timerIntro = 13; // 3 Seconds showing the score
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game end", true);
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game start", false);
    }

    public static void ResetGame()
    {
        gm.screenText = gm.canvas.GetChild(0).GetComponent<TextMesh>();
        changeVisibility(true);
        gm.fps.position = gm.initPos;
        gm.fps.rotation = gm.initRot;

        RenderSettings.skybox = gm.skybox;
        currTime = 0;
    }
    private static void changeVisibility(bool vis)
    {
        gm.mountains.gameObject.SetActive(vis);
        gm.time.gameObject.SetActive(vis);
        gm.fear.gameObject.SetActive(vis);

        //Show Canvas when others are hidden and vice-versa
        gm.canvas.gameObject.SetActive(!vis);
    }
}
