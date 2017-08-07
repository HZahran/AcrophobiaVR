using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class EmotionClassifier
    {
        private static float[][] alphaBeta = new float[2][]; // First 4 pairs of alpha,beta
        private static float arousal = 0;
        private static float valence = 0;
        private static float maxArousal = 0;
        private static float maxValence = 0;
        private static float minArousal = 1000;
        private static float minValence = 1000;
        public static float idleLvl = 4;
        private static float fearLvl = 0;
        public static bool isIdleRead = true;

        public static void fillAlphaBeta(float[][] arr) {
                alphaBeta[0] = new float[2]{arr[0][0], arr[0][1]}; // Fpz
                alphaBeta[1] = new float[2]{arr[2][0], arr[2][1] }; // F3 - F4
        }

        public static void update()
        {
            updateArousal();
            updateValence();
            if (isIdleRead) updateIdle();
            else updateFear();
        }

        private static void updateArousal() // Prefrontal nodes
        {
            float Fpz = alphaBeta[0][1] / alphaBeta[0][0]; // Fpz beta/alpha

            if (Fpz < 4 && Fpz > 0)
               arousal = Fpz; // 0 -> 4

            maxArousal = Mathf.Max(arousal, maxArousal);
            minArousal = Mathf.Min(arousal, minArousal);
            Debug.Log("Arousal : " + arousal + ", Min : " + minArousal + ", Max : " + maxArousal);
        }

        private static void updateValence() // Temporal nodes alpha
        {
           // int margin = 2; // Valence margin
            float f3_f4 = alphaBeta[1][1]/ alphaBeta[1][0]; // F3 - F4


            if (f3_f4 < 4 && f3_f4 > 0)
                valence = f3_f4; //0 -> 4
            maxValence = Mathf.Max(valence, maxValence);
            minValence = Mathf.Min(valence, minValence);
            Debug.Log("Valence : " + valence + ", Min : " + minValence + ", Max : " + maxValence);
        }

        public static void updateFear() // Arousal/ Valence
        {
            if(valence > 0)
                 fearLvl = arousal / valence;

            fearLvl = ((fearLvl - idleLvl) / idleLvl) * 100; // Percentage Fear Increase
            Debug.Log("Fear : " + fearLvl);
            GameManager.UpdateFear(fearLvl);
        }

        public static void updateIdle() // Arousal/ Valence
        {
            if (valence > 0)
                idleLvl = Mathf.Min(idleLvl, (arousal / valence));

            Debug.Log("Idle : " + idleLvl);
            GameManager.UpdateIdle(idleLvl);
        }

    }
}
