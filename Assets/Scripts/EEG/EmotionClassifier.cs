using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class EmotionClassifier
    {
        private static float[][] alphaBeta = new float[4][]; // First 4 pairs of alpha,beta
        private static float arousal = 0;
        private static float valence = 0;
        private static float maxArousal = 0;
        private static float maxValence = 0;
        private static float minArousal = 1000;
        private static float minValence = 1000;
        private static float fearLvl = 0;
        public static float fearRatio = 0;

        public static void fillAlphaBeta(float[][] arr) {
            for (int i = 0; i < 4; i++)
                alphaBeta[i] = new float[2]{arr[i][0], arr[i][1]};
        }

        public static void update()
        {
            updateArousal();
            updateValence();
            updateFear();
        }

        private static void updateArousal() // Prefrontal nodes alpha/beta
        {
            float Fp1 = alphaBeta[0][1] / alphaBeta[0][0];
            float Fp2 = alphaBeta[1][1] / alphaBeta[1][0];

            float avg = (Fp1 + Fp2) / 2;
            if (avg < 4 && avg > 0)
               arousal = avg; // 0 -> 4

            maxArousal = Mathf.Max(arousal, maxArousal);
            minArousal = Mathf.Min(arousal, minArousal);
            Debug.Log("Arousal : " + arousal + ", Min : " + minArousal + ", Max : " + maxArousal);

        }

        private static void updateValence() // Temporal nodes alpha
        {
            int margin = 2; // Valence margin
            float F3 = alphaBeta[2][0];
            float F4 = alphaBeta[3][0];

            float diff = F3 - F4 + margin;
            if (diff < 4 && diff > 0)
                valence = diff; //0 -> 4
            maxValence = Mathf.Max(valence, maxValence);
            minValence = Mathf.Min(valence, minValence);
            Debug.Log("Valence : " + valence + ", Min : " + minValence + ", Max : " + maxValence);
        }

        private static void updateFear() // Arousal/ Valence
        {
            fearLvl = arousal / valence;
            Debug.Log("Fear : " + fearLvl);
            GameManager.UpdateFear(fearLvl);
        }
    }
}
