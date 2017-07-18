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
       //StartCoroutine("coroutineUpdateValues"); use instead of update block
    }

    // Update is called once per frame
    void Update()
    {
        bool newData = false;
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
                    Debug.Log(arr[0][0] + " " + arr[0][1] + " " + arr[0][2]);
                
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
        //-----------------------------------------------------
    }

    private void FixedUpdate()
    {

    }


    private void threadWorker(ref threadDataStruct data)
    {
        JsonBrainObject.createZeroValArr();
        AsyncIO.ForceDotNet.Force();
        NetMQConfig.ManualTerminationTakeOver();
        NetMQConfig.ContextCreate(true);
        NetMQ.Sockets.PullSocket server;
        string recString;
        double counterRetrieval = 0;
        float[] array = null;
        JsonBrainObject obj = null;
        bool workDone = false;
        float maxCalibrationValue = 0;
        System.TimeSpan timeOut = new System.TimeSpan(0, 0, 0, 0, 100);
        print(data.timeout);
        print(data);
        print(data.setDone);
        using (server = new NetMQ.Sockets.PullSocket(">tcp://localhost:5557"))
        {
            print("Pull socket is up and running");
            server.TryReceiveFrameString(timeOut, out recString);


            // This pattern lets us interrupt the work at a safe point if neeeded.
            while (!workDone)
            {
                obj = null;
                try
                {
                    server.TryReceiveFrameString(timeOut, out recString);
                }
                catch
                {
                    obj = null;
                    recString = null;
                }
                try
                {
                    if (recString != null)
                    {
                        //print("convert");
                        obj = JsonConvert.DeserializeObject<JsonBrainObject>(recString);
                        //print("convert done");
                        //counterRetrieval += 1;
                        //print(counterRetrieval);
                    }
                }
                catch
                {
                    obj = null;
                }
                if (obj != null)
                {
                    try { obj.findMax(); } catch { print("couldnt find max"); }
                    try { obj.normalizeArray();} catch { print("couldnt normalize"); }
                   
                }
                if(obj != null)
                {
                    lock (data.mutex)
                    {
                        try { data.arr = obj.getArray(); } catch { print("couldnt get Array"); }
                        data.arrRaw = obj.rawData;
                        data.saver.addToHistory(obj.getArray());
                        data.messageString = recString;
                        data.newData = true;
                        workDone = data.setDone;
                    }
                }else
                {
                    lock (data.mutex)
                    {
                        //try { data.arr = JsonBrainObject.zeroValue; } catch { print("couldnt get Array for zeroes"); }
                        data.newData = false;
                        workDone = data.setDone;
                    }
                }
            }
            print("shutting down socket!");
            server.Dispose();
            NetMQConfig.Cleanup();
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

    float findMax(float[] arr, float max)
    {
        float maxVal = max;
        foreach(float value in arr)
        {
            if(Mathf.Abs(value) > maxVal)
            {
                maxVal = value;
            }
        }
        return maxVal;
    }

    public void playHistoryFrom(float percentage)
    {
        this.history = threadData.saver.getHistoryFrom(percentage);
    }

}
