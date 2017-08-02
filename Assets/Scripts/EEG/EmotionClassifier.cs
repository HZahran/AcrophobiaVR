using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class EmotionClassifier
    {
        private float[][] alphaBeta = new float[4][]; // First 4 pairs of alpha,beta
        private float arousal = 0;
        private float valence = 0;
        private float maxArousal = 0;
        private float maxValence = 0;
        private float minArousal = 1000;
        private float minValence = 1000;
        private float fearLvl = 0;
        public float fearPercentage = 0;

        public void fillAlphaBeta(float[][] arr) {
            for (int i = 0; i < 4; i++)
                alphaBeta[i] = new float[2]{arr[i][0], arr[i][1]};
        }

        public void update()
        {
            updateArousal();
            updateValence();
            updateFear();
        }

        private void updateArousal() // Prefrontal nodes alpha/beta
        {
            float Fp1 = alphaBeta[0][0] / alphaBeta[0][1];
            float Fp2 = alphaBeta[1][0] / alphaBeta[1][1];

            float avg = (Fp1 + Fp2) / 2;
            if (avg < 4 && avg > 0)
               arousal = avg; // 0 -> 4
            maxArousal = Mathf.Max(arousal, maxArousal);
            minArousal = Mathf.Min(arousal, minArousal);
            Debug.Log("Arousal : " + arousal + ", Min : " + minArousal + ", Max : " + maxArousal);

        }

        private void updateValence() // Temporal nodes alpha
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

        private void updateFear() // Arousal/ Valence
        {
            fearLvl = arousal / valence;
            fearPercentage = fearLvl * 4 / 100;
            Debug.Log("Fear : " + fearLvl);
            GameManager.updateFear(fearPercentage);
        }
    }
}
