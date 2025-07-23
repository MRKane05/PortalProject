using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChamberSelectButton : UIButtonFunction {
    public string targetCheckpoint;
    public string targetLevel;
    public TextMeshProUGUI ButtonTitle;


    public void SetLevelLoadDetails(string newCheckpoint, string levelName)
    {
        if (ButtonTitle)
        {
            ButtonTitle.text = newCheckpoint;
        }
        targetCheckpoint = newCheckpoint;
        targetLevel = levelName;
    }

    public void SelectChamber()
    {
        GameLevelHandler.Instance.LoadTargetChamber(targetLevel, targetCheckpoint);
    }
}
