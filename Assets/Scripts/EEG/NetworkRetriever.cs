using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using NetMQ;
using Assets;

struct threadDataStruct
{
    public string messageString;
    public bool running;
    public float[][] arr;
    public float[][] arrRaw;
    public int timeout;
    public bool setDone;
    public object mutex;
    public DataSaver saver;
    public bool newData;
}

public class NetworkRetriever : MonoBehaviour
{
    private static
    Thread serverThread;
    threadDataStruct threadData;
    Object mutex = new Object();
    private ArrayList history = new ArrayList();

    // Use this for initialization
    void Start()
    {
        threadData = new threadDataStruct();
        threadData.arr = new float[200][];
        for(int i = 0; i< 200; i++)
        {
            threadData.arr[i] = new float[3];
        }
        threadData.arrRaw = new float[200][];
        for (int i = 0; i < 200; i++)
        {
            threadData.arrRaw[i] = new float[3];
        }
        threadData.timeout = 1;
        threadData.setDone = false;
        threadData.mutex = this.mutex;
        threadData.newData = false;
        threadData.saver = new DataSaver();
        serverThread = new Thread(lambdaFunc => threadWorker(ref threadData));
        serverThread.Start();
        StartCoroutine("coroutineUpdateValues"); //use instead of update block
    }

    // Update is called once per frame
    void Update()
    {
        /*bool newData = false;
        //-----------------------------------------------------
        //remove and use coroutine for performance increase!
        if (this.history.Count == 0)
        {
            float[][] arr;
            float[][] rawArr;
            lock (threadData.mutex)
            {
                arr = threadData.arr;
                rawArr = threadData.arrRaw;
                newData = threadData.newData;
            }
            if (newData)
            {                
                    //  brModel.setActivityForNode(counter, value, rawArr[counter]);
                    //Debug.Log(threadData.emotion.fearLvl);
                
            }
        }else
        {
            float[][] arr =(float[][]) history[0];
            int counter = 0;
            foreach (float[] value in arr)
            {
                Debug.Log(counter + " " + value);
                // brModel.setActivityForNode(counter, value, value);
                counter++;

            }
            history.Remove(0);

        }
        //-----------------------------------------------------*/
    }

    private void threadWorker(ref threadDataStruct data)
    {
        string recString;
        JsonBrainObject.createZeroValArr();
        JsonBrainObject obj = null;

        AsyncIO.ForceDotNet.Force();
        NetMQ.Sockets.PullSocket server;
        System.TimeSpan timeOut = new System.TimeSpan(0, 0, 0, 0, 100);

        print(data.timeout);
        print(data);
        print(data.setDone);
        using (server = new NetMQ.Sockets.PullSocket(">tcp://localhost:5557"))
        {
            print("Pull socket is up and running");
            server.TryReceiveFrameString(timeOut, out recString);

            bool workDone = false;

            // This pattern lets us interrupt the work at a safe point if neeeded.
            while (!workDone)
            {
                obj = null;
                try
                {
                    server.TryReceiveFrameString(timeOut, out recString);
                    obj = JsonConvert.DeserializeObject<JsonBrainObject>(recString);

                    lock (data.mutex)
                    {
                        try { data.arr = obj.getArray(); } catch { print("couldnt get Array"); }
                        data.arrRaw = obj.rawData;
                        data.saver.addToHistory(obj.getArray());
                        data.messageString = recString;
                        data.newData = true;

                        //emotions
                        EmotionClassifier.fillAlphaBeta(data.arrRaw);
                        EmotionClassifier.update();

                        workDone = data.setDone;
                    }
                }
                catch
                {
                        lock (data.mutex)
                        {
                            data.newData = false;
                            workDone = data.setDone;
                        }
                }
            }
            print("Shutting down socket!");
            server.Dispose();
            NetMQConfig.Cleanup();
        }
    }

    private IEnumerator coroutineUpdateValues()
    {
        float[][] arr;
        float[][] rawArr;
        while (enabled)
        {
            lock (threadData.mutex)
            {
                arr = threadData.arr;
                rawArr = threadData.arrRaw;
            }
            //emotions
             EmotionClassifier.fillAlphaBeta(rawArr);
             EmotionClassifier.update();
             yield return null;
                
            }

    }
    void OnApplicationQuit()
    {
        lock (threadData.mutex)
        {
            threadData.setDone = true;
        }
        serverThread.Join();
    }

    public void playHistoryFrom(float percentage)
    {
        this.history = threadData.saver.getHistoryFrom(percentage);
    }

}
