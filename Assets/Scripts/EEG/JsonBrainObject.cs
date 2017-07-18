using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets
{
    class JsonBrainObject
    {
        public static float[][] zeroValue = new float[30][];
        private float[] max = { 0, 0, 0 };
        [JsonProperty(PropertyName = "EEG")]
        public float[][] EEG = new float[30][];
        public float[][] rawData = new float[30][];


        public float[][] getArray()
        {
            if(EEG != null)
            {
                return EEG;
            }
            return null;
        }

        public void findMax()
        {
            bool newMax = false;
            float[] maxVal = this.max;
            foreach (float[] value in this.EEG)
            {
                for(int i = 0; i < 3; i++)
                {
                    if (Mathf.Abs(value[i]) > maxVal[i])
                    {
                        maxVal[i] = Mathf.Abs(value[i]);
                        newMax = true;
                    }
                }
            }
        }

        public void normalizeArray()
        {
            try
            {
                Array.Copy(EEG, rawData, EEG.Length);
            }
            catch
            {
                Debug.LogError("could not copy array to retai raw data");
            }
            int counter = 0;
            foreach (float[] value in this.EEG)
            {
                for(int i=0; i<3; i++)
                {
                    if (this.max[i] > 0)
                    {
                        this.EEG[counter][i] = Mathf.Abs((value[i] / this.max[i]));
                        
                    }
                }
                counter += 1;
            }
        }

        public static void createZeroValArr()
        {
            for(int i = 0; i < 30; i++)
            {
                JsonBrainObject.zeroValue[i] = new float[]{ 0, 0, 0 };
            }
        }
    }
}
