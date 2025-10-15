using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string username;
    public int day;
    public int energy;
    public int paranoia;
    public int hunger;
    public int hygiene;



    [System.Serializable]
    public class LevelData
    {
        public string levelID;
        public int order;
        public bool isUnlocked;
        public bool isCompleted;

        public LevelData(string id, int order)
        {
            levelID = id;
            this.order = order;
            isUnlocked = false;
            isCompleted = false;
        }
    }

    public List<LevelData> levelsProgress = new List<LevelData>();
}