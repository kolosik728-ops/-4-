using UnityEngine;
namespace TacticalArena {
    public static class SaveSystem {
        public static int Wins => PlayerPrefs.GetInt("TA.Wins",0);
        public static int Losses => PlayerPrefs.GetInt("TA.Losses",0);
        public static int Draws => PlayerPrefs.GetInt("TA.Draws",0);
        public static int Best => PlayerPrefs.GetInt("TA.Best",0);
        public static float Volume { get=>PlayerPrefs.GetFloat("TA.Volume",0.65f); set {PlayerPrefs.SetFloat("TA.Volume",Mathf.Clamp01(value));PlayerPrefs.Save();} }
        public static void Record(Outcome outcome,int round) {
            if(outcome==Outcome.Playing) return;
            string key=outcome==Outcome.Victory?"TA.Wins":outcome==Outcome.Defeat?"TA.Losses":"TA.Draws";
            PlayerPrefs.SetInt(key,PlayerPrefs.GetInt(key,0)+1);
            if(outcome==Outcome.Victory && (Best==0 || round<Best)) PlayerPrefs.SetInt("TA.Best",round);
            PlayerPrefs.Save();
        }
    }
}
