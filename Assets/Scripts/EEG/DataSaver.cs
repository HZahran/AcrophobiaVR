using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Assets
{
    class DataSaver
    {
        private string path = "";
        private float historyLength = 1000f;
        ArrayList history = new ArrayList();

        public void addToHistory(float[][] obj)
        {
            lock (history)
            {
                if (history.Count >= historyLength)
                {
                    history.RemoveAt(0);
                    history.Add(obj);
                }
                else
                {
                    history.Add(obj);
                }
            }
        }

        public ArrayList getHistoryFrom(float percentage)
        {
            lock (history)
            {
                ArrayList part;
                int index = (int)((percentage) * historyLength);
                try {
                    part = history.GetRange(index, history.Count-index-1);
                }
                catch
                {
                    Debug.Log("history not long enough returning data from start of record");
                    Debug.Log(index);
                    part = history.GetRange(0, history.Count);
                }
                return part;
            }
        }

        public int getHistorySize()
        {
            lock (history)
            {
                return history.Count;
            }
        }
    }
}
