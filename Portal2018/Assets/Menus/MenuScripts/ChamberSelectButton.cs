using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChamberSelectButton : UIButtonFunction {
    public string targetCheckpoint;
    public string targetLevel;


    public void SetLevelLoadDetails(string newCheckpoint, string levelName)
    {
        targetCheckpoint = newCheckpoint;
        targetLevel = levelName;
    }

    public void SelectChamber()
    {
        GameLevelHandler.Instance.LoadTargetChamber(targetLevel, targetCheckpoint);
    }
}
