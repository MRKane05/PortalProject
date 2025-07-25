using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevel : MonoBehaviour {
	public string nextLevel;
	public string CheckpointTitle = "__";
	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.name == "Player")
		{
			//Set our player prefs to continue least we quit
			if (nextLevel != "Menu_Start")	//We don't want to write a save return to player files!
			{
				PlayerPrefs.SetString("CheckpointLevel", nextLevel);
				PlayerPrefs.SetString("CheckpointObject", "");
				PlayerPrefs.SetString("CheckpointTitle", CheckpointTitle);
				PlayerPrefs.Save();
			}
			//SceneManager.LoadScene(nextLevel);
			GameLevelHandler.Instance.LoadTargetChamber(nextLevel, "");
		}
	}
}
