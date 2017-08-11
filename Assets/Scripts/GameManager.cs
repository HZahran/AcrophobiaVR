using Kino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // Game Manager Instance
    private static GameManager gm;

    // Game State
    private static bool gameEnded, gameStarted;
    private static int gameStartTime = 0; // Curr Game Start Time
    private static int currTime = 0; // Curr Game Time
    private static float fearPerc = 0;
    private static float idleLvl = 0;
    private static bool gameWon = false;
    private static float timeFinished = 0;
    private static bool gameSaved = false;

    // Player Screen
    public Transform fps;
    public Transform canvas;
    private TextMesh screenText;

    //Visuals
    private Transform mountains;
    private TextMesh time, fear;

    // Skyboxes
    public Material skybox, noSkybox;

    // Initial State
    private Vector3 initPos;
    private Quaternion initRot;
    private static float timerIntro = 15; // Intro takes 10s

    // Tests
    private Tests tests;

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (gm == null)

            //if not, set instance to this
            gm = this;

        //If instance already exists and it's not this:
        else if (gm != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        tests = new Tests();
    }

    // Use this for initialization
    void Start () {
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
            // Idle Test
            if ((tests.idleTime -= Time.deltaTime) > 0)
            {
                tests.IdleTest = idleLvl;
            }
            else Assets.EmotionClassifier.isIdleRead = false;

            // Head Mov. Test
            if (timerIntro <= 10 && (tests.headTime -= Time.deltaTime) > 0)
                 tests.HeadTest = Mathf.Max(tests.HeadTest, fearPerc);
            
            // Sound Test
            if(timerIntro <= 5)
            {
                if (!transform.GetChild(0).GetComponent<AudioSource>().isPlaying) // Play Wind Sound
                    transform.GetChild(0).GetComponent<AudioSource>().Play();

                tests.SoundTest = Mathf.Max(tests.SoundTest, fearPerc);
                tests.soundTime -= Time.deltaTime;
            }

            // Start or Restart Text
            if (timerIntro <= 10)
                gm.screenText.text = (gameEnded ? "Game Restarts in " : "Explore The World Around You\nPress Trigger To Jump\nGame Starts in ") + (int)timerIntro + "s";

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
            fear.text = "Fear " + Mathf.Max(0, Mathf.Min(200, (int)fearPerc)) + "%";

            // General Max Fear
            tests.MaxFear = Mathf.Max(tests.MaxFear, fearPerc);

            if (fps.GetComponent<FPSInputController>().isTriggerPressed)
            {
                //Press Test
                if ((tests.pressTime -= Time.deltaTime) > 0)
                    tests.PressTest = Mathf.Max(tests.PressTest, fearPerc);
            }

            if (fps.GetComponent<MoveCam>().HasFallen())
            {
                // Falling Test
                if ((tests.fallTime -= Time.deltaTime) > 0)
                    tests.FallingTest = Mathf.Max(tests.FallingTest, fearPerc);
            }
        }

        if (gameEnded) {
            // Visuals Test
            if ((tests.visualTime -= Time.deltaTime) > 0)
                tests.VisualTest = Mathf.Max(tests.VisualTest, fearPerc);
            else if (!gameSaved)
            {
                tests.SaveData(gameWon, timeFinished); // Finished Tests & Save
                gameSaved = true;
            }
        }
    }

    public static void UpdateFear(float fearPercentage)
    {
        fearPerc = fearPercentage;
    }
    public static void UpdateIdle(float idle)
    {
        idleLvl = idle;
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
        gm.transform.GetChild(0).GetComponent<AudioSource>().Stop(); // Stop wind

    }

    public void StartGame()
    {
        gameStarted = true;
        gameEnded = false;
        gm.canvas.gameObject.SetActive(false);
        gameStartTime = (int)Time.time;
        // Start 
        //gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game end", false);
        //gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game start", true);
        gm.fps.GetComponent<AudioSource>().Play();
    }

    public static void EndGame(bool wonGame)
    {
        Debug.Log("Game Over");
        gameStarted = false;
        gameEnded = true;
        changeVisibility(false);
        RenderSettings.skybox = gm.noSkybox;

        gameWon = wonGame;
        // Show Final Time
        timeFinished = currTime;
        gm.screenText.text = (wonGame? ("You Did It In\nTime : " + timeFinished + "s"): "Game over") ;
        timerIntro = 15; // 5 Seconds showing the score
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game end", true);
        gm.fps.transform.GetChild(0).GetComponent<Animator>().SetBool("game start", false);

        gm.transform.GetChild(0).GetComponent<AudioSource>().Stop();

    }

    public static void ResetGame()
    {
        gm.screenText = gm.canvas.GetChild(0).GetComponent<TextMesh>();
        changeVisibility(true);
        gm.fps.position = gm.initPos;
        gm.fps.rotation = gm.initRot;

        gameWon = false;
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
